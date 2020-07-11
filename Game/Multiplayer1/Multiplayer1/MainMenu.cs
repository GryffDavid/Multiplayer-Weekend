using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Multiplayer1
{
    class MainMenu
    {
        SpriteFont Font;
        public string[] Files;

        public MainMenu()
        {

        }

        public void LoadContent(ContentManager contentManager)
        {
            Font = contentManager.Load<SpriteFont>("SpriteFont");
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Font, "IT IS YOUR BIRTHDAY.", new Vector2(32, 32), Color.White);
        }
    }
}
