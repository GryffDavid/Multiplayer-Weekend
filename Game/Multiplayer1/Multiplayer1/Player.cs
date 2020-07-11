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
    public enum GunState { GunState1 };

    public class Player
    {
        Texture2D PlayerTexture, GunTexture;

        public int CurrentHP;
        public float Gravity, AimAngle;
        public PlayerIndex PlayerIndex;
        public bool UseGamePad = false;
        public bool InAir = false;
        public bool DoubleJumped = false;

        public Vector2 Position, Velocity, AimDirection, Friction, MaxSpeed;
        public Rectangle DestinationRectangle, GunDestinationRectangle;
        public GamePadState CurrentGamePadState, PreviousGamePadState;
        public KeyboardState CurrentKeyboardState, PreviousKeyboardState;
        public MouseState CurrentMouseState, PreviousMouseState;

        GamePadThumbSticks Sticks;
        Vector2 MoveStick, AimStick;

        public Level CurrentLevel;

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

            if (UseGamePad == true)
            {
                #region Controller
                AimDirection = MoveStick * new Vector2(1, -1);
                AimAngle = (float)Math.Atan2(AimDirection.Y, AimDirection.X);

                Velocity.X += (MoveStick.X * 3f);

                #region Stop Moving
                if (MoveStick.X == 0)
                {
                    Velocity.X = 0;
                } 
                #endregion

                #region Jumping
                if (CurrentGamePadState.Buttons.A == ButtonState.Pressed &&
                    PreviousGamePadState.Buttons.A == ButtonState.Released &&
                    DoubleJumped == false)
                {
                    if (InAir == true)
                    {
                        DoubleJumped = true;
                    }

                    Velocity.Y -= 12f;
                } 
                #endregion

                #region Shooting
                if (CurrentGamePadState.Buttons.X == ButtonState.Pressed &&
                    PreviousGamePadState.Buttons.X == ButtonState.Released)
                {

                }
                #endregion
                #endregion

            }
            else
            {
                #region Keyboard + Mouse

                #endregion
            }
            #region Handle Physics
            Velocity.Y += Gravity;
            Position += Velocity;

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
                DoubleJumped = false;
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
