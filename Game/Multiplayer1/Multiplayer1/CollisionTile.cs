using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace Multiplayer1
{
    public class CollisionTile : Tile
    {
        CollisionType CollisionType;

        public CollisionTile()
        {
            CollisionType = CollisionType.Solid;
        }

        new public void LoadContent(ContentManager contentManager)
        {
            TileTexture = contentManager.Load<Texture2D>("Tiles/TilesCollection");

            if (TileChar.X == 0)
                CollisionType = CollisionType.Solid;

            if (TileChar.X == 1)
                CollisionType = CollisionType.OneWay;

            SourceRectangle = new Rectangle(336, 0, 16, 16);
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
            BoundingBox = DestinationRectangle;
        }

        new public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TileTexture, DestinationRectangle, SourceRectangle, Color.Lerp(Color.White, Color.Transparent, 0.0f));
        }
    }
}
