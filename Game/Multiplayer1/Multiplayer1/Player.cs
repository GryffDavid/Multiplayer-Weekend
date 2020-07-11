using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Multiplayer1
{
    public enum GunState { GunState1, GunState2, GunState3, Grenade };

    class Player
    {
        Texture2D PlayerTexture, GunTexture;

        public float Gravity, AimAngle;
        public PlayerIndex PlayerIndex;
        public bool UseGamePad = false;
        public bool InAir = false;

        public Vector2 Position, Velocity, AimDirection, Friction, MaxSpeed;
        public Rectangle DestinationRectangle, GunDestinationRectangle;
        public GamePadState CurrentGamePadState, PreviousGamePadState;
        public KeyboardState CurrentKeyboardState, PreviousKeyboardState;
        public MouseState CurrentMouseState, PreviousMouseState;

        GamePadThumbSticks Sticks;
        Vector2 MoveStick, AimStick;


        public GunState CurrentGunState;

        public Player(PlayerIndex myIndex)
        {
            PlayerIndex = myIndex;
            Position = new Vector2(500, 500);
            MaxSpeed = new Vector2(6, 6);
            Friction = new Vector2(0.75f, 0.75f);
            Gravity = 0.6f;
            
        }

        public void Update(GameTime gameTime)
        {
            CurrentGamePadState = GamePad.GetState(PlayerIndex);

            Sticks = CurrentGamePadState.ThumbSticks;
            MoveStick = Sticks.Left;
            AimStick = Sticks.Right;

            CurrentKeyboardState = Keyboard.GetState();
            CurrentMouseState = Mouse.GetState();

            #region Controller

            AimDirection = MoveStick * new Vector2(1, -1);
            AimAngle = (float)Math.Atan2(AimDirection.Y, AimDirection.X);

            if (InAir == false)
            {
                Velocity.X += (MoveStick.X * 3f);

                if (MoveStick.X == 0)
                {
                    Velocity.X *= Friction.X;
                }
            }

            if (CurrentGamePadState.Buttons.A == ButtonState.Pressed &&
                PreviousGamePadState.Buttons.A == ButtonState.Released)
            {
                //JUMP
                Velocity.Y -= 10f;
            }
            #endregion

            #region Mouse

            #endregion

            #region Keyboard

            #endregion

            Position += Velocity;

            #region Handle Physics
            Velocity.Y += Gravity;

            if (Velocity.X > MaxSpeed.X)
            {
                Velocity.X = MaxSpeed.X;
            }

            if (Velocity.X < -MaxSpeed.X)
            {
                Velocity.X = -MaxSpeed.X;
            }

            if (Position.Y >= 500)
            {
                Position.Y = 500;
                Velocity.Y = 0;
                InAir = false;
            }
            else
            {
                InAir = true;
            }
            #endregion

            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, PlayerTexture.Width, PlayerTexture.Height);
            GunDestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, GunTexture.Width, GunTexture.Height);

            PreviousGamePadState = CurrentGamePadState;
            PreviousKeyboardState = CurrentKeyboardState;
            PreviousMouseState = CurrentMouseState;
        }

        public void LoadContent(ContentManager contentManager)
        {
            PlayerTexture = contentManager.Load<Texture2D>("PlayerTexture");
            GunTexture = contentManager.Load<Texture2D>("GunTexture");
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(PlayerTexture, DestinationRectangle, Color.Red);
            spriteBatch.Draw(GunTexture, GunDestinationRectangle, null, Color.White, AimAngle, new Vector2(0, GunTexture.Height / 2), SpriteEffects.None, 0);
        }
    }
}
