using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace LevelEditor1
{
    public class CollisionTile : Tile
    {
        CollisionType CollisionType;

        public CollisionTile()
        { 
            CollisionType = CollisionType.Solid;
        }

        public void LoadContent(ContentManager contentManager)
        {
            TileTexture = contentManager.Load<Texture2D>("CollisionTiles");

            if (TileChar.X == 0)
                CollisionType = CollisionType.Solid;

            if (TileChar.X == 1)
                CollisionType = CollisionType.OneWay;

            if (TileChar.X == 2)
                CollisionType = CollisionType.Death;

            switch (CollisionType)
            {
                case CollisionType.Solid:
                    SourceRectangle = new Rectangle(0, 0, 16, 16);
                    break;

                case CollisionType.OneWay:
                    SourceRectangle = new Rectangle(16, 0, 16, 16);
                    break;

                case CollisionType.Death:
                    SourceRectangle = new Rectangle(32, 0, 16, 16);
                    break;
            }
            
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(TileTexture, DestinationRectangle, SourceRectangle, Color.Lerp(Color.White, Color.Transparent, 0.5f));
        }
    }
}
