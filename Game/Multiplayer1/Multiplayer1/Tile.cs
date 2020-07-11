using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Multiplayer1
{
    public class Tile
    {
        Vector2 Position, Size;
        Texture2D TileTexture;
        public Rectangle BoundingBox;
        public BoundingBox MyBox;

        public Tile(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
            BoundingBox = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        }

        public void LoadContent(ContentManager contentManager)
        {
            TileTexture = contentManager.Load<Texture2D>("BasicTile");
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TileTexture, new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y), Color.White);
        }
    }
}
