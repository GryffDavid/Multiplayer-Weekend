using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace LevelEditor1
{
    public class Decal
    {
        public Texture2D Texture;
        public CollisionTile TetherTile;
        public Vector2 Size;
        public Vector2 Orienation;

        public Decal(Texture2D texture, CollisionTile tile)
        {

        }

        public void LoadContent(ContentManager contentManager)
        {

        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, new Rectangle((int)TetherTile.Position.X, (int)TetherTile.Position.Y, Texture.Width, Texture.Height), Color.White);
        }
    }
}
