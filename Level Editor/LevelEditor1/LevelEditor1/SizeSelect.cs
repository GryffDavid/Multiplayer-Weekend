using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace LevelEditor1
{
    class SizeSelect
    {
        public int CurrentSize = 32;
        public Vector2 CurrentPosition;
        public SpriteFont Font;
        public Texture2D BoxTexture;
        public string Name;
        MouseState CurrentMouseState, PreviousMouseState;

        public SizeSelect(Vector2 position, string name)
        {
            CurrentPosition = position;
            Name = name;
        }

        public void LoadContent(ContentManager contentManager)
        {
            Font = contentManager.Load<SpriteFont>("Font");
            BoxTexture = contentManager.Load<Texture2D>("BasicTile");
        }

        public void Update()
        {
            CurrentMouseState = Mouse.GetState();

            if (new Rectangle((int)CurrentPosition.X, (int)CurrentPosition.Y, 16, 16).Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)))
            {
                if (CurrentMouseState.LeftButton == ButtonState.Released && 
                    PreviousMouseState.LeftButton == ButtonState.Pressed)
                {
                    CurrentSize = ChangeValue(CurrentSize);
                }
            }

            PreviousMouseState = CurrentMouseState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(BoxTexture, new Rectangle((int)CurrentPosition.X, (int)CurrentPosition.Y, 16, 16), Color.White);
            
            
            spriteBatch.DrawString(Font, CurrentSize.ToString(), new Vector2(CurrentPosition.X, CurrentPosition.Y), Color.Red);

            spriteBatch.DrawString(Font, Name, new Vector2(CurrentPosition.X + 16, CurrentPosition.Y), Color.Black);

            //spriteBatch.DrawString(Font, Name, new Vector2(CurrentPosition.X + 32, 0), Color.Black);
        }

        public int ChangeValue(int currentValue)
        {
            if (currentValue == 16)
                return 32;

            if (currentValue == 32)
                return 48;

            if (currentValue == 48)
                return 16;

            return 16;
        }
    }
}
