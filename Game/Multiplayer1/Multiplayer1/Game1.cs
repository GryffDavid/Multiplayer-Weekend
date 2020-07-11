using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Xml;
using System.Xml.Serialization;
namespace Multiplayer1
{
    public enum GameType { Local, Network };
    public enum PlayerControls { GamePad, KeyboardMouse };
    public enum GameState { MainMenu, Connecting, Game, Paused, EndScreen };

    public delegate void PlayerShootHappenedEventHandler(object source, PlayerShootEventArgs e);
    public class PlayerShootEventArgs : EventArgs
    {
        public Player Player { get; set; }
    }

    public delegate void PlayerGrenadeHappenedEventHandler(object source, PlayerGrenadeEventArgs e);
    public class PlayerGrenadeEventArgs : EventArgs
    {
        public Player Player { get; set; }
    }

    public enum CollisionType { Solid, OneWay };

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Explosion
        public EventHandler<ExplosionEventArgs> ExplosionHappenedEvent;
        public class ExplosionEventArgs : EventArgs
        {
            public Explosion Explosion { get; set; }
        }
        protected virtual void CreateExplosion(Explosion explosion, object source)
        {
            if (ExplosionHappenedEvent != null)
                OnExplosionHappened(source, new ExplosionEventArgs() { Explosion = explosion });
        }
        #endregion


        static Random Random = new Random();
        GameState CurrentGameState;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D RocketTexture, GrenadeTexture;

        Texture2D SparkTexture, RoundSparkTexture, ExplosionParticle2;

        Player[] Players = new Player[4];

        bool CheckedPlayers = false;
        Level Level1;

        List<Rocket> RocketList = new List<Rocket>();
        List<Grenade> GrenadeList = new List<Grenade>();

        SpriteFont Font;

        List<Emitter> EmitterList = new List<Emitter>();
        List<Emitter> EmitterList2 = new List<Emitter>();

        public SoundEffect GunShot1, GunCollision1, GrenadeExplosion1, GrenadeExplosion2, GrenadeExplosion3;

        public Color FireColor = new Color(Color.DarkOrange.R, Color.DarkOrange.G, Color.DarkOrange.B, 150);
        public Color FireColor2 = new Color(255, Color.DarkOrange.G, Color.DarkOrange.B, 50);

        public Color ExplosionColor = new Color(255, 255, 255, 150);
        public Color ExplosionColor2 = new Color(255, 255, 255, 50);
        public Color ExplosionColor3 = Color.Lerp(Color.Red, new Color(255, Color.DarkOrange.G, Color.DarkOrange.B, 50), 0.5f);

        public Color DirtColor = new Color(51, 31, 0, 100);
        public Color DirtColor2 = new Color(51, 31, 0, 125);

        public Color SmokeColor1 = Color.Lerp(Color.DarkGray, Color.Transparent, 0.1f);
        public Color SmokeColor2 = Color.Lerp(Color.Gray, Color.Transparent, 0.1f);
        

        public void OnPlayerShoot(object source, PlayerShootEventArgs e)
        {
            GunShot1.Play(0.25f, 0, 0);
            RocketList.Add(new Rocket(e.Player.Position + new Vector2(0, 10), RocketTexture, 15f, e.Player.AimDirection, e.Player));
        }

        public void OnPlayerGrenade(object source, PlayerGrenadeEventArgs e)
        {
            Grenade newGrenade = new Grenade(GrenadeTexture, new Vector2(e.Player.DestinationRectangle.Center.X, e.Player.DestinationRectangle.Center.Y - 12), new Vector2(e.Player.AimDirection.X, -0.5f), 6 +  Math.Abs(e.Player.Velocity.X));
            newGrenade.CurrentLevel = Level1;
            GrenadeList.Add(newGrenade);
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 736;
            Content.RootDirectory = "Content";
        }
        
        protected override void Initialize()
        {
            //PlayerShootEvent += OnPlayerShoot;
            CurrentGameState = GameState.Game;

            ExplosionHappenedEvent += OnExplosionHappened;
            base.Initialize();
        }
                
        protected override void LoadContent()
        {
            Level1 = new Level();
            Level1 = Load();
            Level1.LoadContent(Content);

            GunShot1 = Content.Load<SoundEffect>("SoundEffects/GunShot1");
            GunCollision1 = Content.Load<SoundEffect>("SoundEffects/GunCollision1");
            GrenadeExplosion1 = Content.Load<SoundEffect>("SoundEffects/Grenade/GrenadeExplosion1");
            GrenadeExplosion2 = Content.Load<SoundEffect>("SoundEffects/Grenade/GrenadeExplosion2");
            GrenadeExplosion3 = Content.Load<SoundEffect>("SoundEffects/Grenade/GrenadeExplosion3");

            SparkTexture = Content.Load<Texture2D>("ParticleTextures/Spark");
            RoundSparkTexture = Content.Load<Texture2D>("ParticleTextures/RoundSpark");
            ExplosionParticle2 = Content.Load<Texture2D>("ParticleTextures/ExplosionParticle2");

            RocketTexture = Content.Load<Texture2D>("GunTexture");
            GrenadeTexture = Content.Load<Texture2D>("GrenadeTexture");

            Font = Content.Load<SpriteFont>("SpriteFont");

            if (CheckedPlayers == false)
                CheckPlayers();
            
            spriteBatch = new SpriteBatch(GraphicsDevice);

            foreach (Player player in Players.Where(Player => Player != null))
            {
                player.LoadContent(Content);
            }
        }
        
        protected override void UnloadContent()
        {

        }
        
        protected override void Update(GameTime gameTime)
        {
            switch (CurrentGameState)
            {
                case GameState.Game:
                    {
                        foreach (Emitter emitter in EmitterList)
                        {
                            emitter.Update(gameTime);
                        }

                        foreach (Emitter emitter in EmitterList2)
                        {
                            emitter.Update(gameTime);
                        }

                        EmitterList.RemoveAll(Emitter => Emitter.AddMore == false && Emitter.ParticleList.Count == 0);

                        foreach (Player player in Players.Where(Player => Player != null))
                        {
                            player.Update(gameTime);
                        }
                        
                        foreach (Rocket rocket in RocketList)
                        {
                            rocket.Update(gameTime);

                            if (Level1.CollisionTileList.Any(Tile => Tile.BoundingBox.Intersects(rocket.DestinationRectangle)))
                            {
                                GunCollision1.Play(0.25f, 0, 0);
                                rocket.Active = false;

                                Emitter sparkEmitter = new Emitter(SparkTexture, rocket.Position, new Vector2(0, 360), new Vector2(2, 3), new Vector2(250, 450), 1f, true, new Vector2(0, 0), new Vector2(0, 0), Vector2.One, Color.Orange, Color.OrangeRed, 0.2f, 0.1f, 1, 2, false, new Vector2(0, 720), false, 0f, false, false, null, null, null, true);
                                EmitterList.Add(sparkEmitter);
                            }

                            if (Players.Any(Player =>
                                Player != null &&
                                rocket.SourcePlayer != Player &&
                                rocket.Active == true &&
                                Player.DestinationRectangle.Intersects(rocket.DestinationRectangle)))
                            {
                                rocket.Active = false;
                                Players.First(Player => Player.DestinationRectangle.Intersects(rocket.DestinationRectangle) && Player != rocket.SourcePlayer).CurrentHP = 0;
                                rocket.SourcePlayer.Score++;
                            }
                        }

                        foreach (Grenade grenade in GrenadeList)
                        {
                            grenade.Update(gameTime);

                            if (grenade.Active == false)
                            {
                                CreateExplosion(new Explosion() { Position = grenade.Position, BlastRadius = grenade.BlastRadius, Damage = 1 }, grenade);
                            }
                        }

                        RocketList.RemoveAll(Rocket => Rocket.Active == false);
                        GrenadeList.RemoveAll(Grenade => Grenade.Active == false);
                    }
                    break;

                case GameState.Paused:
                    {

                    }
                    break;
            }            

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkGray);

            spriteBatch.Begin();
            //Level1.Draw(spriteBatch);           
            Level1.DrawTiles(spriteBatch);

            Level1.DrawBackgroundTiles(spriteBatch);

            foreach (Player player in Players.Where(Player => Player != null))
            {
                player.Draw(spriteBatch);
            }

            Level1.DrawForegroundTiles(spriteBatch);

            foreach (Player player in Players.Where(Player => Player != null))
            {
                spriteBatch.DrawString(Font, player.PlayerIndex.ToString() + ": " + player.Score.ToString(), new Vector2(32, 32 + (int)player.PlayerIndex * 32), Color.White);
            }

            foreach (Grenade grenade in GrenadeList)
            {
                grenade.Draw(spriteBatch);
            }

            foreach (Emitter emitter in EmitterList2)
            {
                emitter.Draw(spriteBatch);
            }


            spriteBatch.End();


            spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Additive);
            foreach (Emitter emitter in EmitterList)
            {
                emitter.Draw(spriteBatch);
            }

            foreach (Emitter emitter in EmitterList2)
            {
                emitter.Draw(spriteBatch);
            }

            foreach (Rocket rocket in RocketList)
            {
                rocket.Draw(spriteBatch);
            }
            
            //Level1.DrawTiles(spriteBatch);
            //Level1.DrawBackgroundTiles(spriteBatch);
            spriteBatch.End();
           
            base.Draw(gameTime);
        }

        protected void CheckPlayers()
        {
            GamePadState[] gamePad = new GamePadState[4];

            for (int i = 0; i < 4; i++)
            {
                gamePad[i] = GamePad.GetState((PlayerIndex)i);

                if (gamePad[i].IsConnected == true)
                {
                    Players[i] = new Player((PlayerIndex)i);
                    Players[i].Initialize(OnPlayerShoot, OnPlayerGrenade);
                    Players[i].CurrentLevel = Level1;
                }
            }

            CheckedPlayers = true;            
        }

        private Level Load()
        {
            XmlSerializer SerializerObj = new XmlSerializer(typeof(Level));

            // Create a new file stream for reading the XML file
            FileStream ReadFileStream = new FileStream("C:\\LevelData\\" + "NewLevelTest" + ".txt", FileMode.Open, FileAccess.Read, FileShare.Read);

            // Load the object saved above by using the Deserialize function
            Level LoadedObj = (Level)SerializerObj.Deserialize(ReadFileStream);

            // Cleanup
            ReadFileStream.Close();

            return LoadedObj;
        }

        public void OnExplosionHappened(object source, ExplosionEventArgs e)
        {
//            GrenadeExplosion1.Play(0.5f, 0, 0);

            PlayRandomSound(GrenadeExplosion1, GrenadeExplosion2, GrenadeExplosion3);

            Explosion explosion = e.Explosion;

            //for (int i = 0; i < 10; i++)
            //{
            //    Emitter SparkEmitter = new Emitter(RoundSparkTexture,
            //            new Vector2(explosion.Position.X, explosion.Position.Y),
            //            new Vector2(0, 0),
            //            new Vector2(1, 2), new Vector2(180 * 16, 200 * 16), 0.8f, true, new Vector2(0, 360), new Vector2(1, 3),
            //            new Vector2(0.1f, 0.3f), Color.Orange, Color.OrangeRed, 0.05f, 0.5f, 10, 5, false,
            //            new Vector2(explosion.Position.Y + 16, explosion.Position.Y + 16), null, 0,
            //            false, false, new Vector2(3, 5), new Vector2(90 - 40, 90 + 40), 0.2f, true);
            //    EmitterList.Add(SparkEmitter);
            //}

            Emitter ExplosionEmitter = new Emitter(ExplosionParticle2,
                                        new Vector2(explosion.Position.X, explosion.Position.Y+16),
                                        new Vector2(20, 160), new Vector2(0.3f, 0.8f), new Vector2(500, 1000), 0.85f, true, new Vector2(-2, 2),
                                        new Vector2(-1, 1), new Vector2(0.15f, 0.25f), FireColor,
                                        Color.Black, -0.2f, 0.1f, 10, 1, false, new Vector2(0, 720), false, 0f,
                                        null, null, null, null, null, null, new Vector2(0.1f, 0.2f), true, true, null, null, null, true);
            EmitterList2.Add(ExplosionEmitter);

            Emitter ExplosionEmitter3 = new Emitter(ExplosionParticle2,
                    new Vector2(explosion.Position.X, explosion.Position.Y+16),
                    new Vector2(85, 95), new Vector2(2, 4), new Vector2(400, 640), 0.35f, true, new Vector2(0, 0),
                    new Vector2(0, 0), new Vector2(0.085f, 0.2f), FireColor, ExplosionColor3, -0.1f, 0.05f, 10, 1, false,
                    new Vector2(0, 720), true, 0f,
                    null, null, null, null, null, null, new Vector2(0.0025f, 0.0025f), true, true, 50);
            EmitterList2.Add(ExplosionEmitter3);

            

            //Emitter DebrisEmitter = new Emitter(SplodgeParticle,
            //       new Vector2(explosion.Position.X, explosion.Position.Y),
            //       new Vector2(70, 110), new Vector2(5, 7), new Vector2(480, 1760), 1f, true, new Vector2(0, 360),
            //       new Vector2(1, 3), new Vector2(0.007f, 0.05f), Color.DarkSlateGray, Color.DarkSlateGray,
            //       0.2f, 0.2f, 5, 1, true, new Vector2(0, 720), null, 0f);
            //EmitterList.Add(DebrisEmitter);

            foreach (Player player in Players.Where(Player => Player != null))
            {
                float dist = Vector2.Distance(new Vector2(player.DestinationRectangle.Center.X, player.DestinationRectangle.Center.Y), explosion.Position);

                if (dist < explosion.BlastRadius)
                {
                    player.CurrentHP = 0;
                }
            }

        }

        public void PlayRandomSound(params SoundEffect[] soundEffect)
        {
            soundEffect[Random.Next(0, soundEffect.Length)].Play(0.5f, 0, 0);
        }
    }
}
