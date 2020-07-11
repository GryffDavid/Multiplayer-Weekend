using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Multiplayer1
{
    class Gib
    {
        Texture2D Texture, DecalTexture;
        public List<Emitter> EmitterList = new List<Emitter>();

        static Random Random = new Random();
        public Level CurrentLevel;

        public Vector2 Position, Velocity;
        public float Speed;

        public float CurrentTime, MaxTime;
        public int BlastRadius = 96;

        public float Rotation;
        public Rectangle DestinationRectangle, CollisionRectangle;

        public bool Active = true;

        public Color GibColor = Color.Maroon;

        public Gib(Texture2D texture, Vector2 position, Vector2 direction, float speed, Texture2D decalTexture, Texture2D emitterTexture, Color gibColor)
        {
            DecalTexture = decalTexture;

            GibColor = gibColor;

            Texture = texture;
            Position = position;
            Speed = speed;
            Velocity = direction * Speed;
            Rotation = MathHelper.ToRadians(Random.Next(0, 360));
            MaxTime = Random.Next(1000, 2000);

            Emitter newEmitter = new Emitter(emitterTexture, Position, new Vector2(0, 360), new Vector2(0, 2), new Vector2(500, 1500),
                                             0.85f, true, new Vector2(0, 360), new Vector2(-3, 3), new Vector2(0.5f, 1f), Color.DarkRed, Color.DarkRed,
                                             0.2f, MaxTime/1000f, 16, 1, false, new Vector2(0, 720));
            EmitterList.Add(newEmitter);
        }

        public void Update(GameTime gameTime)
        {
            
            CurrentTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (CurrentTime >= MaxTime)
            {
                
                Active = false;
            }

            Position += Velocity;

            Velocity.Y += 0.6f;

            Velocity *= new Vector2(0.99f, 0.95f);

            foreach (Emitter emitter in EmitterList)
            {
                if (Math.Abs(Velocity.X) >= 1 || Math.Abs(Velocity.Y) >= 1)
                {
                    float dir = (float)Math.Atan2(-Velocity.Y, -Velocity.X);
                    emitter.AngleRange = new Vector2(dir, dir);
                    emitter.AddMore = true;
                }
                else
                {
                    emitter.AddMore = false;
                }

                emitter.Position = Position;
                emitter.Update(gameTime);
            }

            EmitterList.RemoveAll(Emitter => Emitter.AddMore == false && Emitter.ParticleList.Count == 0);


            CollisionRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);

            CheckRightCollisions();
            CheckDownCollisions();
            CheckLeftCollisions();
            CheckUpCollisions();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y+4, (int)(Texture.Width*1.5f), (int)(Texture.Height*1.5f)), null,
                             GibColor, Rotation, new Vector2(Texture.Width / 2, Texture.Height / 2), SpriteEffects.None, 0);

            foreach (Emitter emitter in EmitterList)
            {
                emitter.Draw(spriteBatch);
            }
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
                            if (Velocity.X > 6 && tile.LeftDecal == null && (tile as CollisionTile) != null)
                            {
                                Decal newDecal = new Decal();
                                newDecal.Size = tile.Size;
                                newDecal.Initialize(DecalTexture, tile, new Vector2(-1, 0));
                                tile.LeftDecal = newDecal;
                            }

                            Position.X -= (CollisionRectangle.Right - tile.BoundingBox.Left);
                            Velocity.X = -Velocity.X * 0.5f;
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
                            if (Velocity.X < -6 && tile.RightDecal == null && (tile as CollisionTile) != null)
                            {
                                Decal newDecal = new Decal();
                                newDecal.Size = tile.Size;
                                newDecal.Initialize(DecalTexture, tile, new Vector2(1, 0));
                                tile.RightDecal = newDecal;
                            }

                            Position.X += (tile.BoundingBox.Right - CollisionRectangle.Left);
                            Velocity.X = -Velocity.X * 0.5f;
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
                                if (Velocity.Y < -4 && tile.BottomDecal == null && (tile as CollisionTile) != null)
                                {
                                    Decal newDecal = new Decal();
                                    newDecal.Size = tile.Size;
                                    newDecal.Initialize(DecalTexture, tile, new Vector2(0, -1));
                                    tile.BottomDecal = newDecal;
                                }

                                Position.Y += (tile.BoundingBox.Bottom - CollisionRectangle.Top);
                                Velocity.Y = -Velocity.Y * 0.5f;
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
                            if (Velocity.Y > 8 && tile.TopDecal == null && (tile as CollisionTile) != null)
                            {
                                Decal newDecal = new Decal();
                                newDecal.Size = tile.Size;
                                newDecal.Initialize(DecalTexture, tile, new Vector2(0,1));
                                tile.TopDecal = newDecal;
                            }

                            Position.Y += (tile.BoundingBox.Top - CollisionRectangle.Bottom);
                            Velocity.Y = -Velocity.Y * 0.5f;
                            Velocity.X *= 0.95f;
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
