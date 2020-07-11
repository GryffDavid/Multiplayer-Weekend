using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Multiplayer1
{
    class AmmoPack
    {
        Texture2D Texture;
        Vector2 Position;
        Vector2 Velocity;
        public Level CurrentLevel;

        public Rectangle CollisionRectangle;

        public AmmoPack(Vector2 position, Texture2D texture)
        {
            Position = position;
            Texture = texture;
        }

        public void LoadContent(ContentManager content)
        {

        }

        public void Update(GameTime gameTime)
        {
            CollisionRectangle = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);

            CheckDownCollisions();

            Position += Velocity;
            Velocity.Y += 0.5f;

            //CheckRightCollisions();

            //CheckLeftCollisions();

            

            //CheckUpCollisions();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, Color.White);
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
                            Velocity.X = 0;
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
                            Velocity.X = 0;
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
                                Velocity.Y = 0;
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
                            Velocity.Y = 0;
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
