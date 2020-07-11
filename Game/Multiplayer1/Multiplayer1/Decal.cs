using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Multiplayer1
{
    public class Decal
    {
        public Texture2D Texture;
        public CollisionTile TetherTile;
        public Vector2 Size;
        public Vector2 Orienation;
        public float Rotation;
        static Random Random = new Random();

        public Decal()
        {

        }
        /// <summary>
        /// Initialize a new decal
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="tile"></param>
        /// <param name="orientation">(0, -1) = Up, (0, 1) = Down, (1, 0) = Right, (-1, 0) = Left</param>
        public void Initialize(Texture2D texture, CollisionTile tile, Vector2 orientation)
        {
            Texture = texture;
            TetherTile = tile;
            Orienation = orientation;

            Rotation = (float)Math.Atan2(Orienation.X, Orienation.Y);
        }
        
        public void LoadContent(ContentManager contentManager)
        {

        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture,
                    new Rectangle((int)TetherTile.DestinationRectangle.Center.X, (int)TetherTile.DestinationRectangle.Center.Y,
                                  (int)Size.X, (int)Size.Y),
                                 null, Color.White, Rotation, new Vector2(24, 24), SpriteEffects.None, 0);
        }
    }
}
