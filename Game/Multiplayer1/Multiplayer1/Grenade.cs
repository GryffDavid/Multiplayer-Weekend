﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Multiplayer1
{
    class Grenade
    {
        static Random Random = new Random();
        public Level CurrentLevel;
        public Texture2D GrenadeTexture;

        public Vector2 Position, Velocity;
        public float Speed;

        public float CurrentTime, MaxTime;
        public int BlastRadius = 144;

        public float Rotation;
        public Rectangle DestinationRectangle, CollisionRectangle;

        public bool Active = true;
        public object Source;

        public Grenade(Texture2D texture, Vector2 position, Vector2 direction, float speed, object source)
        {
            GrenadeTexture = texture;
            Position = position;
            Speed = speed;
            Velocity = direction * Speed;            
            Source = source;

            MaxTime = 1000;
        }

        public void Update(GameTime gameTime)
        {
            CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (CurrentTime >= MaxTime)
            {
                Active = false;
            }

            Position += (Velocity * (float)(gameTime.ElapsedGameTime.TotalSeconds * 60f));

            if (Velocity.X > 30 || Velocity.Y > 30)
            {
                int p = 0;
            }

            Velocity.Y += 0.6f;

            Velocity *= new Vector2(0.99f, 0.95f);

            CollisionRectangle = new Rectangle((int)Position.X, (int)Position.Y, GrenadeTexture.Width, GrenadeTexture.Height);

            if (Velocity.X > 0)
            {
                CheckRightCollisions();
            }

            if (Velocity.X < 0)
            {
                CheckLeftCollisions();
            }

            if (Velocity.Y > 0)
            {
                CheckDownCollisions();
            }

            if (Velocity.Y < 0)
            {
                CheckUpCollisions();
            }            

            //CheckRightCollisions();
            //CheckDownCollisions();
            //CheckLeftCollisions();
            //CheckUpCollisions();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(GrenadeTexture, Position, Color.White);
        }

        public bool CheckRightCollisions()
        {
            //foreach (List<Tile> subList in CurrentLevel.CollisionTileList)
            {
                foreach (CollisionTile tile in CurrentLevel.CollisionTileList)
                {
                    for (int i = 0; i < CollisionRectangle.Height; i++)
                    {
                        if (tile.BoundingBox.Contains(new Point((int)(CollisionRectangle.Right + Velocity.X), (int)(CollisionRectangle.Top + i))) == true)
                        {
                            Position.X -= (CollisionRectangle.Right - tile.BoundingBox.Left);
                            Velocity.X = -Velocity.X * 0.85f;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool CheckLeftCollisions()
        {
            //foreach (List<Tile> subList in CurrentLevel.CollisionTileList)
            {
                foreach (CollisionTile tile in CurrentLevel.CollisionTileList)
                {
                    for (int i = 0; i < CollisionRectangle.Height; i++)
                    {
                        if (tile.BoundingBox.Contains(new Point((int)(CollisionRectangle.Left + Velocity.X - 1), (int)(CollisionRectangle.Top + i))) == true)
                        {
                            Position.X += (tile.BoundingBox.Right - CollisionRectangle.Left);
                            Velocity.X = -Velocity.X * 0.85f;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool CheckUpCollisions()
        {
            //foreach (List<Tile> subList in CurrentLevel.CollisionTileList)
            {
                foreach (CollisionTile tile in CurrentLevel.CollisionTileList)
                {
                    for (int i = 0; i < CollisionRectangle.Width; i++)
                    {
                        if (Velocity.Y < 0)
                            if (tile.BoundingBox.Contains(new Point((int)(CollisionRectangle.Left + i), (int)(CollisionRectangle.Top + Velocity.Y - 1))) == true)
                            {
                                Position.Y += (tile.BoundingBox.Bottom - CollisionRectangle.Top);
                                Velocity.Y = -Velocity.Y * 0.85f;
                                return true;
                            }
                    }
                }
            }

            return false;
        }

        public bool CheckDownCollisions()
        {
            //foreach (List<Tile> subList in CurrentLevel.CollisionTileList)
            {
                foreach (CollisionTile tile in CurrentLevel.CollisionTileList)
                {
                    for (int i = 0; i < CollisionRectangle.Width; i++)
                    {
                        if (tile.BoundingBox.Contains(new Point((int)(CollisionRectangle.Left + i),
                                                                (int)(CollisionRectangle.Bottom + Velocity.Y + 1))) == true)
                        {
                            Position.Y += (tile.BoundingBox.Top - CollisionRectangle.Bottom);
                            Velocity.Y = -Velocity.Y * 0.85f;
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
