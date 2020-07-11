using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace Multiplayer1
{
    public enum GunState { GunState1 };

    public class Player
    {
        public event PlayerShootHappenedEventHandler PlayerShootHappened;
        public void CreatePlayerShoot()
        {
            OnPlayerShootHappened();
        }
        protected virtual void OnPlayerShootHappened()
        {
            if (PlayerShootHappened != null)
                PlayerShootHappened(this, new PlayerShootEventArgs() { Player = this });
        }

        Texture2D PlayerTexture, GunTexture;

        public int CurrentHP, Score;
        public float Gravity, AimAngle, CurrentShootDelay, MaxShootDelay;
        public PlayerIndex PlayerIndex;
        public bool UseGamePad = true;
        public bool InAir = true;
        public bool DoubleJumped = false;

        public Vector2 Position, Velocity, AimDirection, Friction, MaxSpeed;
        public Rectangle DestinationRectangle, GunDestinationRectangle, CollisionRectangle;
        public GamePadState CurrentGamePadState, PreviousGamePadState;
        public KeyboardState CurrentKeyboardState, PreviousKeyboardState;
        public MouseState CurrentMouseState, PreviousMouseState;

        GamePadThumbSticks Sticks;
        Vector2 MoveStick, AimStick;

        public Level CurrentLevel;

        public GunState CurrentGunState;
        public Animation CurrentAnimation;

        public Animation RunRightAnimation, RunRightUpAnimation, RunRightDownAnimation,
                         RunLeftAnimation, RunLeftUpAnimation, RunLeftDownAnimation,
                         StandRightAnimation, StandRightUpAnimation, StandRightDownAnimation,
                         StandLeftAnimation, StandLeftUpAnimation, StandLeftDownAnimation,
                         JumpRightAnimation, JumpRightUpAnimation, JumpRightDownAnimation,
                         JumpLeftAnimation, JumpLeftUpAnimation, JumpLeftDownAnimation,
                         CrouchRightAnimation, CrouchLeftAnimation;

        public Texture2D RunRightTexture, RunRightUpTexture, RunRightDownTexture,
                         RunLeftTexture, RunLeftUpTexture, RunLeftDownTexture,
                         StandRightTexture, StandRightUpTexture, StandRightDownTexture,
                         StandLeftTexture, StandLeftUpTexture, StandLeftDownTexture,
                         JumpRightTexture, JumpRightUpTexture, JumpRightDownTexture,
                         JumpLeftTexture, JumpLeftUpTexture, JumpLeftDownTexture,
                         CrouchRightTexture, CrouchLeftTexture;

        public SoundEffect JumpLand1, Jump1;

        public DynamicSoundEffectInstance DynamicSound;

        public void Initialize(PlayerShootHappenedEventHandler thing)
        {
            if (PlayerShootHappened == null)
                PlayerShootHappened += thing;

            MaxShootDelay = 250;
            CurrentShootDelay = 0;

            Score = 0;
            CurrentHP = 1;
        }

        public Player(PlayerIndex myIndex)
        {
            PlayerIndex = myIndex;
            Position = new Vector2(500, 500);
            MaxSpeed = new Vector2(4, 6);
            Friction = new Vector2(0.75f, 0.75f);
            Gravity = 0.6f;
        }

        public void Update(GameTime gameTime)
        {
            CurrentGamePadState = GamePad.GetState(PlayerIndex);
            CollisionRectangle = new Rectangle((int)Position.X, (int)Position.Y, 39, 49);
            
            Sticks = CurrentGamePadState.ThumbSticks;
            MoveStick = Sticks.Left;
            AimStick = Sticks.Right;

            CurrentKeyboardState = Keyboard.GetState();
            CurrentMouseState = Mouse.GetState();

            if (CurrentShootDelay < MaxShootDelay)
            {
                CurrentShootDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            if (CurrentAnimation != null)
                CurrentAnimation.Update(gameTime);

            if (UseGamePad == true)
            {
                #region Controller
                if (MoveStick.X < 0f)
                {
                    if (CurrentAnimation != RunLeftAnimation)
                        CurrentAnimation = RunLeftAnimation;

                    AimDirection.X = -1f;
                }

                if (MoveStick.X > 0f)
                {
                    if (CurrentAnimation != RunRightAnimation)
                        CurrentAnimation = RunRightAnimation;

                    AimDirection.X = 1f;
                }

                AimAngle = (float)Math.Atan2(AimDirection.Y, AimDirection.X);

                Velocity.X += MoveStick.X * 3f;

                #region Stop Moving
                if (MoveStick.X == 0)
                {
                    Velocity.X = 0;
                } 
                #endregion

                #region Jumping
                if (CurrentGamePadState.Buttons.A == ButtonState.Pressed &&
                    PreviousGamePadState.Buttons.A == ButtonState.Released &&
                    DoubleJumped == false &&
                    Velocity.Y >= 0)
                {
                    if (InAir == true)
                    {
                        DoubleJumped = true;
                    }

                    if (InAir == false)
                    {
                        Jump1.Play(1.0f, 0f, 0f);
                    }

                    Velocity.Y -= 12f;
                } 
                #endregion

                #region Shooting
                if (CurrentGamePadState.Buttons.X == ButtonState.Pressed &&
                    PreviousGamePadState.Buttons.X == ButtonState.Released &&
                    CurrentShootDelay >= MaxShootDelay)
                {
                    CurrentShootDelay = 0;
                    CreatePlayerShoot();
                }
                #endregion

                #region Grenade
                if (CurrentGamePadState.Buttons.B == ButtonState.Pressed &&
                    PreviousGamePadState.Buttons.B == ButtonState.Released)
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
            
            //Moving right
            if (MoveStick.X > 0)
            {
                //Nothing in the way
                if (CheckRightCollisions() == false)
                {
                    Position.X += Velocity.X * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);
                }
            }

            //Moving left
            if (MoveStick.X < 0)
            {
                //Nothing in the way
                if (CheckLeftCollisions() == false)
                {
                    Position.X += Velocity.X * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);
                }
            }

            if (CheckUpCollisions() == true)
            {
                Velocity.Y = 0;
            }

            if (CheckDownCollisions() == false)
            {
                Position.Y += Velocity.Y * ((float)gameTime.ElapsedGameTime.TotalSeconds * 60f);
            }

            if (Velocity.X > MaxSpeed.X)
            {
                Velocity.X = MaxSpeed.X;
            }

            if (Velocity.X < -MaxSpeed.X)
            {
                Velocity.X = -MaxSpeed.X;
            }

            if (CheckDownCollisions() == false)
            {
                InAir = true;
            }
            else
            {
                if (InAir == true)
                    JumpLand1.Play(0.15f, 0f, 0f);

                Velocity.Y = 0;
                InAir = false;
                DoubleJumped = false;
            }

            if (CheckDownCollisions() == false)
                Velocity.Y += Gravity;
            #endregion


            if (Velocity.X == 0)
            {
                if (AimDirection.X > 0)
                    CurrentAnimation = StandRightAnimation;

                if (AimDirection.X < 0)
                    CurrentAnimation = StandLeftAnimation;
            }

            if (Velocity.Y != 0)
            {
                if (AimDirection.X > 0)
                    CurrentAnimation = JumpRightAnimation;

                if (AimDirection.X < 0)
                    CurrentAnimation = JumpLeftAnimation;                
            }
                
            if (CurrentHP == 0)
            {
                Position = new Vector2(32, 32);
                CurrentHP = 1;
            }

            //if (CurrentAnimation != null)
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)CurrentAnimation.FrameSize.X, (int)CurrentAnimation.FrameSize.Y);
            //DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, 50, 50);

            GunDestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, GunTexture.Width, GunTexture.Height);

            PreviousGamePadState = CurrentGamePadState;
            PreviousKeyboardState = CurrentKeyboardState;
            PreviousMouseState = CurrentMouseState;
        }

        public void LoadContent(ContentManager content)
        {
            JumpLand1 = content.Load<SoundEffect>("SoundEffects/Player/JumpLand1");
            Jump1 = content.Load<SoundEffect>("SoundEffects/Player/Jump1");

            DynamicSound = new DynamicSoundEffectInstance(48000, AudioChannels.Stereo);

            PlayerTexture = content.Load<Texture2D>("PlayerTexture");
            GunTexture = content.Load<Texture2D>("GunTexture");

            RunRightTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex+1) + "/Running/RunRight");
            RunRightUpTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Running/RunRightUp");
            RunRightDownTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Running/RunRightDown");

            RunLeftTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Running/RunLeft");
            RunLeftUpTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Running/RunLeftUp");
            RunLeftDownTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Running/RunLeftDown");

            StandRightTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Standing/StandRight");
            StandRightUpTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Standing/StandRightUp");
            StandRightDownTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Standing/StandRightDown");

            StandLeftTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Standing/StandLeft");
            StandLeftUpTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Standing/StandLeftUp");
            StandLeftDownTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Standing/StandLeftDown");
            
            JumpRightTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpRight");
            JumpRightUpTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpRight");
            JumpRightDownTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpRight");

            JumpLeftTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpLeft");

            RunRightAnimation = new Animation(RunRightTexture, 8, 80);
            RunLeftAnimation = new Animation(RunLeftTexture, 8, 80);

            StandLeftAnimation = new Animation(StandLeftTexture, 1, 80);
            StandRightAnimation = new Animation(StandRightTexture, 1, 80);

            JumpLeftAnimation = new Animation(JumpLeftTexture, 1, 80);
            JumpRightAnimation = new Animation(JumpRightTexture, 1, 80);

            CurrentAnimation = RunRightAnimation;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(PlayerTexture, DestinationRectangle, Color.Red);
            if (CurrentAnimation != null)
                CurrentAnimation.Draw(spriteBatch, Position);

            //spriteBatch.Draw(GunTexture, GunDestinationRectangle, null, Color.White, AimAngle, new Vector2(0, GunTexture.Height / 2), SpriteEffects.None, 0);
        }

        public bool CheckRightCollisions()
        {
            //foreach (List<Tile> subList in CurrentLevel.CollisionTileList)
            {
                foreach (CollisionTile tile in CurrentLevel.CollisionTileList)
                {
                    for (int i = 0; i < CollisionRectangle.Height; i++)
                    {
                        if (tile.BoundingBox.Contains(new Point((int)(CollisionRectangle.Right + Velocity.X), (int)(CollisionRectangle.Top + i))) == true)
                        {
                            Position.X -= (CollisionRectangle.Right - tile.BoundingBox.Left);
                            Velocity.X = 0;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool CheckLeftCollisions()
        {
            //foreach (List<Tile> subList in CurrentLevel.CollisionTileList)
            {
                foreach (CollisionTile tile in CurrentLevel.CollisionTileList)
                {
                    for (int i = 0; i < CollisionRectangle.Height; i++)
                    {
                        if (tile.BoundingBox.Contains(new Point((int)(CollisionRectangle.Left + Velocity.X - 1), (int)(CollisionRectangle.Top + i))) == true)
                        {
                            Position.X += (tile.BoundingBox.Right - CollisionRectangle.Left);
                            Velocity.X = 0;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool CheckUpCollisions()
        {
            //foreach (List<Tile> subList in CurrentLevel.CollisionTileList)
            {
                foreach (CollisionTile tile in CurrentLevel.CollisionTileList)
                {
                    for (int i = 0; i < CollisionRectangle.Width; i++)
                    {
                        if (Velocity.Y < 0)
                            if (tile.BoundingBox.Contains(new Point((int)(CollisionRectangle.Left + i), (int)(CollisionRectangle.Top + Velocity.Y - 1))) == true)
                            {
                                Position.Y += (tile.BoundingBox.Bottom - CollisionRectangle.Top);
                                Velocity.Y = 0;
                                return true;
                            }
                    }
                }
            }

            return false;
        }

        public bool CheckDownCollisions()
        {
            //foreach (List<Tile> subList in CurrentLevel.CollisionTileList)
            {
                foreach (CollisionTile tile in CurrentLevel.CollisionTileList)
                {
                    for (int i = 0; i < CollisionRectangle.Width; i++)
                    {
                        if (tile.BoundingBox.Contains(new Point((int)(CollisionRectangle.Left + i), 
                                                                (int)(CollisionRectangle.Bottom + Velocity.Y + 1))) == true)
                        {
                            Position.Y += (tile.BoundingBox.Top - CollisionRectangle.Bottom);
                            Velocity.Y = 0;                            
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
