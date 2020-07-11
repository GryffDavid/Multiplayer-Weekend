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
    enum Facing { Left, Right };

    public class Player
    {
        static Random Random = new Random();
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

        public event PlayerGrenadeHappenedEventHandler PlayerGrenadeHappened;
        public void CreatePlayerGrenade()
        {
            OnPlayerGrenadeHappened();
        }
        protected virtual void OnPlayerGrenadeHappened()
        {
            if (PlayerGrenadeHappened != null)
                PlayerGrenadeHappened(this, new PlayerGrenadeEventArgs() { Player = this });
        }

        Texture2D PlayerTexture, GunTexture;

        public int CurrentHP, Score, GrenadeAmmo;
        public float Gravity, AimAngle, CurrentShootDelay, MaxShootDelay;
        public PlayerIndex PlayerIndex;
        public bool UseGamePad = true;
        public bool InAir = true;
        public bool DoubleJumped = false;
        public bool Active = false;
        public bool IsCrouching = false;

        public bool DPadUp, DPadDown, PressedStart;

        public Vector2 Position, Velocity, AimDirection, Friction, MaxSpeed;
        public Rectangle DestinationRectangle, GunDestinationRectangle, CollisionRectangle;
        public GamePadState CurrentGamePadState, PreviousGamePadState;
        public KeyboardState CurrentKeyboardState, PreviousKeyboardState;
        public MouseState CurrentMouseState, PreviousMouseState;


        GamePadThumbSticks Sticks;
        Vector2 MoveStick, AimStick;

        public Level CurrentLevel;

        public GunState CurrentGunState;
        public int CurrentGunID = 0;

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
                         CrouchRightTexture, CrouchLeftTexture,
                         HeadTexture;

        public Texture2D DustTexture, GrenadeIcon;

        public SoundEffect JumpLand1, Jump1,
                           Throw1, Throw2, Throw3, Throw4;

        public float RumbleTime, MaxRumbleTime;
        Vector2 RumbleValues;

        List<Emitter> EmitterList = new List<Emitter>();

        Facing CurrentFacing = Facing.Right;

        public void Initialize(PlayerShootHappenedEventHandler thing, PlayerGrenadeHappenedEventHandler thing2)
        {
            if (PlayerShootHappened == null)
                PlayerShootHappened += thing;

            if (PlayerGrenadeHappened == null)
                PlayerGrenadeHappened += thing2;

            MaxShootDelay = 250;
            CurrentShootDelay = 0;

            Score = 0;
            CurrentHP = 1;
            GrenadeAmmo = 3;

            MaxRumbleTime = 500;
            RumbleTime = 500;
            RumbleValues = new Vector2(0.5f, 0.5f);

            CurrentGunState = (GunState)CurrentGunID;
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
            CurrentKeyboardState = Keyboard.GetState();
            CurrentMouseState = Mouse.GetState();

            if (CurrentGamePadState.DPad.Up == ButtonState.Released && 
                PreviousGamePadState.DPad.Up == ButtonState.Pressed)
            {
                DPadUp = true;
            }
            else
            {
                DPadUp = false;
            }


            if (CurrentGamePadState.DPad.Down == ButtonState.Released &&
                PreviousGamePadState.DPad.Down == ButtonState.Pressed)
            {
                DPadDown = true;
            }
            else
            {
                DPadDown = false;
            }

            if (CurrentGamePadState.Buttons.Start == ButtonState.Released &&
                PreviousGamePadState.Buttons.Start == ButtonState.Pressed)
            {
                PressedStart = true;
            }
            else
            {
                PressedStart = false;
            }

            if (Active == true)
            {
                if (IsCrouching == false)
                    CollisionRectangle = new Rectangle((int)Position.X + 6, (int)Position.Y, 26, 49);

                Sticks = CurrentGamePadState.ThumbSticks;
                MoveStick = Sticks.Left;
                AimStick = Sticks.Right;

                if (RumbleTime <= MaxRumbleTime)
                    RumbleTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                if (RumbleTime >= MaxRumbleTime)
                {
                    GamePad.SetVibration(PlayerIndex, 0, 0);
                }

                if (CurrentShootDelay < MaxShootDelay)
                {
                    CurrentShootDelay += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                }

                if (CurrentAnimation != null)
                    CurrentAnimation.Update(gameTime);

                foreach (Emitter emitter in EmitterList)
                {
                    emitter.Update(gameTime);
                }

                EmitterList.RemoveAll(Emitter => Emitter.AddMore == false && Emitter.ParticleList.Count == 0);


                if (UseGamePad == true)
                {
                    #region Controller
                    if (MoveStick.X < 0f)
                    {
                        if (CurrentAnimation != RunLeftAnimation)
                            CurrentAnimation = RunLeftAnimation;

                        AimDirection.X = -1f;
                        CurrentFacing = Facing.Left;
                    }

                    if (MoveStick.X > 0f)
                    {
                        if (CurrentAnimation != RunRightAnimation)
                            CurrentAnimation = RunRightAnimation;

                        AimDirection.X = 1f;
                        CurrentFacing = Facing.Right;
                    }

                    AimAngle = (float)Math.Atan2(AimDirection.Y, AimDirection.X);

                    if (IsCrouching == false)
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
                            //if (Math.Abs(Velocity.Y) <= 2)
                            //{
                            //    Velocity.Y -= 14f;
                            //}
                            //else
                            //{
                            //    Velocity.Y -= 12f;
                            //}

                            Velocity.Y -= 12f;
                            DoubleJumped = true;
                        }
                        else
                        {
                            Velocity.Y -= 12f;
                        }

                        if (InAir == false)
                        {
                            Emitter DustEmitter = new Emitter(DustTexture,
                                          new Vector2(DestinationRectangle.Center.X, DestinationRectangle.Bottom),
                                          new Vector2(80, 100), new Vector2(2f, 4f), new Vector2(1120, 1600), 0.25f, true, new Vector2(0, 360),
                                          new Vector2(0.5f, 1f), new Vector2(0.02f, 0.05f), Color.White, Color.Gray, 0.03f, 0.02f, 5, 2, false,
                                          new Vector2(0, 1080), false, 0,
                                          null, null, null, null, null, null, new Vector2(0.08f, 0.08f), true, true);
                            EmitterList.Add(DustEmitter);

                            Jump1.Play(1.0f, 0f, 0f);
                        }

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
                        if (GrenadeAmmo > 0)
                        {
                            GrenadeAmmo--;
                            PlayRandomSound(Throw1, Throw2, Throw3, Throw4);
                            CurrentShootDelay = 0;
                            CreatePlayerGrenade();
                        }
                    }
                    #endregion

                    //Select previous weapon
                    if (CurrentGamePadState.Buttons.LeftShoulder == ButtonState.Released &&
                        PreviousGamePadState.Buttons.LeftShoulder == ButtonState.Pressed)
                    {
                        CurrentGunID--;

                        if (CurrentGunID < 0)
                        {
                            CurrentGunID = Enum.GetValues(typeof(GunState)).Length - 1;
                        }

                        CurrentGunState = (GunState)CurrentGunID;
                    }

                    //Select next weapon
                    if (CurrentGamePadState.Buttons.RightShoulder == ButtonState.Released &&
                        PreviousGamePadState.Buttons.RightShoulder == ButtonState.Pressed)
                    {
                        CurrentGunID++;

                        if (CurrentGunID > Enum.GetValues(typeof(GunState)).Length - 1)
                        {
                            CurrentGunID = 0;
                        }

                        CurrentGunState = (GunState)CurrentGunID;
                    }
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


                if (MoveStick.Y < 0f)
                {
                    IsCrouching = true;
                    Velocity.X = 0;
                    
                    if (CurrentFacing == Facing.Right)
                    {
                        CurrentAnimation = CrouchRightAnimation;
                    }

                    if (CurrentFacing == Facing.Left)
                    {
                        CurrentAnimation = CrouchLeftAnimation;
                    }

                    CollisionRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)CurrentAnimation.FrameSize.X, (int)CurrentAnimation.FrameSize.Y);
                }
                else
                {
                    IsCrouching = false;
                }

                if (Velocity.X == 0)
                {
                    if (IsCrouching == false)
                    {
                        if (AimDirection.X > 0)
                            CurrentAnimation = StandRightAnimation;

                        if (AimDirection.X < 0)
                            CurrentAnimation = StandLeftAnimation;
                    }


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

            }

            PreviousGamePadState = CurrentGamePadState;
            PreviousKeyboardState = CurrentKeyboardState;
            PreviousMouseState = CurrentMouseState;
        }

        public void LoadContent(ContentManager content)
        {
            JumpLand1 = content.Load<SoundEffect>("SoundEffects/Player/JumpLand1");
            Jump1 = content.Load<SoundEffect>("SoundEffects/Player/Jump1");

            Throw1 = content.Load<SoundEffect>("SoundEffects/Throw/Throw1");
            Throw2 = content.Load<SoundEffect>("SoundEffects/Throw/Throw2");
            Throw3 = content.Load<SoundEffect>("SoundEffects/Throw/Throw3");
            Throw4 = content.Load<SoundEffect>("SoundEffects/Throw/Throw4");

            GrenadeIcon = content.Load<Texture2D>("GrenadeIcon");

            PlayerTexture = content.Load<Texture2D>("PlayerTexture");
            GunTexture = content.Load<Texture2D>("GunTexture");
            DustTexture = content.Load<Texture2D>("ParticleTextures/DustTexture");

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
            JumpRightUpTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpRightUp");
            JumpRightDownTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpRightDown");

            JumpLeftTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpLeft");
            JumpLeftUpTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpLeftUp");
            JumpLeftDownTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Jumping/JumpLeftDown");

            CrouchRightTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/CrouchRight");
            CrouchLeftTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/CrouchLeft");

            HeadTexture = content.Load<Texture2D>("Player" + ((int)PlayerIndex + 1) + "/Head");

            RunRightAnimation = new Animation(RunRightTexture, 8, 50);
            RunRightUpAnimation = new Animation(RunRightUpTexture, 8, 50);
            RunRightDownAnimation = new Animation(RunRightDownTexture, 8, 50);

            RunLeftAnimation = new Animation(RunLeftTexture, 8, 50);
            RunLeftUpAnimation = new Animation(RunLeftUpTexture, 8, 50);
            RunLeftDownAnimation = new Animation(RunLeftDownTexture, 8, 50);

            StandLeftAnimation = new Animation(StandLeftTexture, 1, 50);
            StandLeftUpAnimation = new Animation(StandLeftUpTexture, 1, 50);
            StandLeftDownAnimation = new Animation(StandLeftDownTexture, 1, 50);            

            StandRightAnimation = new Animation(StandRightTexture, 1, 50);
            StandRightUpAnimation = new Animation(StandRightUpTexture, 1, 50);
            StandRightDownAnimation = new Animation(StandRightDownTexture, 1, 50);            

            JumpLeftAnimation = new Animation(JumpLeftTexture, 1, 50);
            JumpLeftUpAnimation = new Animation(JumpLeftUpTexture, 1, 50);
            JumpLeftDownAnimation = new Animation(JumpLeftDownTexture, 1, 50);            

            JumpRightAnimation = new Animation(JumpRightTexture, 1, 50);
            JumpRightUpAnimation = new Animation(JumpRightUpTexture, 1, 50);
            JumpRightDownAnimation = new Animation(JumpRightDownTexture, 1, 50);

            CrouchRightAnimation = new Animation(CrouchRightTexture, 1, 50);
            CrouchLeftAnimation = new Animation(CrouchLeftTexture, 1, 50);

            CurrentAnimation = StandRightAnimation;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(PlayerTexture, DestinationRectangle, Color.Red);
            if (CurrentAnimation != null)
                CurrentAnimation.Draw(spriteBatch, Position);
            
            foreach (Emitter emitter in EmitterList)
            {
                emitter.Draw(spriteBatch);
            }

            for (int i = 0; i < GrenadeAmmo; i++)
            {
                spriteBatch.Draw(GrenadeIcon, new Vector2(Position.X + (i * 10), Position.Y - GrenadeIcon.Height - 4), Color.White);
            }

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

        public void PlayRandomSound(params SoundEffect[] soundEffect)
        {
            soundEffect[Random.Next(0, soundEffect.Length)].Play(0.5f, 0, 0);
        }

        public void MakeRumble(float time, Vector2 value)
        {
            GamePad.SetVibration(PlayerIndex, value.X, value.Y);
            RumbleTime = 0;
            MaxRumbleTime = time;
        }
    }
}
