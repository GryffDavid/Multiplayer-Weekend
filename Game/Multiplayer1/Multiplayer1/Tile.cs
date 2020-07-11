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
        public Vector2 Position, Size, TileChar;
        public Texture2D TileTexture;
        public Rectangle BoundingBox, DestinationRectangle, SourceRectangle;

        public Tile()
        {
            //Vector2 position, Vector2 size, Vector2 tileChar
            //TileChar = tileChar;
            //Position = position;
            //Size = size;
            //BoundingBox = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        }

        public void LoadContent(ContentManager contentManager)
        {
            TileTexture = contentManager.Load<Texture2D>("Tiles/TilesCollection");

            SourceRectangle = new Rectangle((int)TileChar.X * 16, (int)TileChar.Y * 16, 16, 16);
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TileTexture, DestinationRectangle, SourceRectangle, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0f);
        }
    }
}
