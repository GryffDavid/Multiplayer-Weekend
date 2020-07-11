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
    public enum MultiPlayerType { Local, Network };
    public enum PlayerControls { GamePad, KeyboardMouse };
    public enum GameState { MainMenu, Connecting, Game, Paused, EndScreen };
    public enum GunState { Bullet, Rocket };

    //DeathMatch = 10 minute time limit with victor being most kills by the end
    //First to 20 is the first person to get 20 kills
    //Could also make a mode consisting of rounds like TowerFall
    //Capture the flag
    public enum GameType { DeathMatch, FirstTo20 };

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

        MainMenu MainMenu = new MainMenu();

        static Random Random = new Random();
        GameState CurrentGameState;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D RocketTexture, GrenadeTexture;

        Texture2D SparkTexture, RoundSparkTexture, ExplosionParticle2, Splodge,
                  DecalTexture1, DecalTexture2, DecalTexture3, DecalTexture4,
                  Player1Head, Background1, AmmoPackTexture, HealthPackTexture;

        Player[] Players = new Player[4];

        bool CheckedPlayers = false;
        Level Level1;

        List<Rocket> RocketList = new List<Rocket>();
        List<Bullet> BulletList = new List<Bullet>();

        List<Grenade> GrenadeList = new List<Grenade>();
        List<Gib> GibList = new List<Gib>();

        SpriteFont Font;

        List<Emitter> EmitterList = new List<Emitter>();
        List<Emitter> EmitterList2 = new List<Emitter>();

        public SoundEffect GunShot1, GunCollision1, GrenadeExplosion1, 
                           GrenadeExplosion2, GrenadeExplosion3,
                           Gore1, Gore2, Gore3, Gore4, Gore5;

        public Color FireColor = new Color(Color.DarkOrange.R, Color.DarkOrange.G, Color.DarkOrange.B, 150);
        public Color FireColor2 = new Color(255, Color.DarkOrange.G, Color.DarkOrange.B, 50);

        public Color ExplosionColor = new Color(255, 255, 255, 150);
        public Color ExplosionColor2 = new Color(255, 255, 255, 50);
        public Color ExplosionColor3 = Color.Lerp(Color.Red, new Color(255, Color.DarkOrange.G, Color.DarkOrange.B, 50), 0.5f);

        public Color DirtColor = new Color(51, 31, 0, 100);
        public Color DirtColor2 = new Color(51, 31, 0, 125);

        public Color SmokeColor1 = Color.Lerp(Color.DarkGray, Color.Transparent, 0.1f);
        public Color SmokeColor2 = Color.Lerp(Color.Gray, Color.Transparent, 0.1f);
         
        List<AmmoPack> AmmoList = new List<AmmoPack>();
        List<HealthPack> HealthList = new List<HealthPack>();

        float TimeToBonus, MaxTimeToBonus;

        int MenuIndex = 0;
        
        public void OnPlayerShoot(object source, PlayerShootEventArgs e)
        {
            switch (e.Player.CurrentGunState)
            {
                case GunState.Rocket:
                    {
                        GunShot1.Play(0.25f, 0, 0);
                        RocketList.Add(new Rocket(e.Player.Position + new Vector2(0, 10), RocketTexture, 15f, e.Player.AimDirection, e.Player));
                        e.Player.MakeRumble(100, new Vector2(0.2f, 0.2f));

                    }
                    break;

                case GunState.Bullet:
                    {
                        GunShot1.Play(0.25f, 0, 0);
                        BulletList.Add(new Bullet(e.Player.Position + new Vector2(0, 4), RocketTexture, 15f, e.Player.AimDirection, e.Player));
                        e.Player.MakeRumble(100, new Vector2(0.2f, 0.2f));

                    }
                    break;
            }
        }

        public void OnPlayerGrenade(object source, PlayerGrenadeEventArgs e)
        {
            Grenade newGrenade = new Grenade(GrenadeTexture, 
                new Vector2(e.Player.DestinationRectangle.Center.X, e.Player.DestinationRectangle.Center.Y - 12), 
                new Vector2(e.Player.AimDirection.X, -0.5f), 6 +  Math.Abs(e.Player.Velocity.X), source);
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
            CurrentGameState = GameState.MainMenu;

            MaxTimeToBonus = Random.Next(4000, 10000);
            ExplosionHappenedEvent += OnExplosionHappened;
            base.Initialize();
        }
                
        protected override void LoadContent()
        {
            MainMenu.LoadContent(Content);

            //Level1 = new Level();
            //Level1 = Load();
            //Level1.LoadContent(Content);

            GunShot1 = Content.Load<SoundEffect>("SoundEffects/GunShot1");
            GunCollision1 = Content.Load<SoundEffect>("SoundEffects/GunCollision1");
            GrenadeExplosion1 = Content.Load<SoundEffect>("SoundEffects/Grenade/GrenadeExplosion1");
            GrenadeExplosion2 = Content.Load<SoundEffect>("SoundEffects/Grenade/GrenadeExplosion2");
            GrenadeExplosion3 = Content.Load<SoundEffect>("SoundEffects/Grenade/GrenadeExplosion3");

            Player1Head = Content.Load<Texture2D>("Player1/Head");

            Background1 = Content.Load<Texture2D>("Background/Background1");
            AmmoPackTexture = Content.Load<Texture2D>("AmmoPack");
            HealthPackTexture = Content.Load<Texture2D>("HealthPack");

            Gore1 = Content.Load<SoundEffect>("SoundEffects/Gore/Gore1");
            Gore2 = Content.Load<SoundEffect>("SoundEffects/Gore/Gore2");
            Gore3 = Content.Load<SoundEffect>("SoundEffects/Gore/Gore3");
            Gore4 = Content.Load<SoundEffect>("SoundEffects/Gore/Gore4");
            Gore5 = Content.Load<SoundEffect>("SoundEffects/Gore/Gore5");

            DecalTexture1 = Content.Load<Texture2D>("Decals/TileBloodDecal1");
            DecalTexture2 = Content.Load<Texture2D>("Decals/TileBloodDecal2");
            DecalTexture3 = Content.Load<Texture2D>("Decals/TileBloodDecal3");
            DecalTexture4 = Content.Load<Texture2D>("Decals/TileBloodDecal4");

            Splodge = Content.Load<Texture2D>("ParticleTextures/Splodge");
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
                #region GAME
                case GameState.Game:
                    {
                        if (Players.Any(Player => Player.Score >= 5))
                        {
                            CurrentGameState = GameState.EndScreen;
                        }

                        TimeToBonus += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                        if (TimeToBonus >= MaxTimeToBonus)
                        {
                            if (Random.NextDouble() > 0.5f)
                            {
                                if (AmmoList.Count <= Players.Count())
                                {
                                    //Add ammo pack

                                    //Get list of tiles which are not solid
                                    List<List<Tile>> emptyList;
                                    List<Tile> deadList = new List<Tile>();

                                    emptyList = Level1.MainTileList;

                                    foreach (List<Tile> tileList in emptyList)
                                    {
                                        foreach (Tile tile in tileList)
                                        {
                                            if (Level1.CollisionTileList.Any(Collision => Collision.DestinationRectangle.Intersects(tile.DestinationRectangle)))
                                            {
                                                deadList.Add(tile);
                                            }
                                        }
                                    }

                                    Vector2 newPos = new Vector2(Random.Next(33, 1247), Random.Next(33, 703));

                                    while (deadList.Any(Tile => Tile.DestinationRectangle.Contains(new Point((int)newPos.X, (int)newPos.Y))))
                                    {
                                        newPos = new Vector2(Random.Next(33, 1247), Random.Next(33, 703));
                                    }

                                    AmmoPack newPack = new AmmoPack(newPos, AmmoPackTexture);
                                    newPack.CurrentLevel = Level1;
                                    AmmoList.Add(newPack);
                                }
                            }
                            else
                            {
                                //Add health pack
                            }

                            TimeToBonus = 0;
                            MaxTimeToBonus = Random.Next(4000, 10000);
                        }

                        foreach (AmmoPack pack in AmmoList)
                        {
                            pack.Update(gameTime);                            
                        }

                        foreach (HealthPack pack in HealthList)
                        {
                            pack.Update(gameTime);
                        }

                        foreach (Gib gib in GibList)
                        {
                            gib.Update(gameTime);
                        }

                        GibList.RemoveAll(Gib => Gib.Active == false && Gib.EmitterList.All(Emitter => Emitter.Active == false && Emitter.ParticleList.Count == 0));

                        foreach (Emitter emitter in EmitterList)
                        {
                            emitter.Update(gameTime);
                        }

                        foreach (Emitter emitter in EmitterList2)
                        {
                            emitter.Update(gameTime);
                        }

                        EmitterList.RemoveAll(Emitter => Emitter.AddMore == false && Emitter.ParticleList.Count == 0);
                        EmitterList2.RemoveAll(Emitter => Emitter.AddMore == false && Emitter.ParticleList.Count == 0);


                        foreach (Player player in Players.Where(Player => Player != null))
                        {
                            player.Update(gameTime);

                            if (AmmoList.Any(Pack => Pack.CollisionRectangle.Intersects(player.DestinationRectangle)) && player.GrenadeAmmo < 3)
                            {
                                player.GrenadeAmmo = 3;
                            }

                            AmmoList.RemoveAll(Pack => Pack.CollisionRectangle.Intersects(player.DestinationRectangle));
                        }

                        #region Update Rockets
                        foreach (Rocket rocket in RocketList)
                        {
                            rocket.Update(gameTime);

                            //Home to nearest player
                            Player[] otherPlayers;

                            otherPlayers = Players.Where(Player => Player != rocket.SourcePlayer).ToArray();
                            Player nearestPlayer = otherPlayers.OrderBy(Player => Vector2.Distance(Player.Position, rocket.StartPosition)).First();
                            Vector2 dirToPlayer = rocket.StartPosition - nearestPlayer.Position;
                            dirToPlayer.Normalize();

                            rocket.Velocity.Y -= MathHelper.Clamp(dirToPlayer.Y * 0.5f, -0.1f, 0.1f);

                            if (Level1.CollisionTileList.Any(Tile => Tile.BoundingBox.Intersects(rocket.DestinationRectangle)))
                            {
                                GunCollision1.Play(0.5f, 0, 0);
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
                                Player hitPlayer = Players.First(Player => Player.DestinationRectangle.Intersects(rocket.DestinationRectangle) && Player != rocket.SourcePlayer);
                                
                                if (hitPlayer.CurrentHP > 0)
                                {
                                    GunCollision1.Play(0.2f, 0, 0);

                                    for (int i = 0; i < 20; i++)
                                    {
                                        float angle = (float)Math.Atan2(rocket.Velocity.Y, rocket.Velocity.X) + MathHelper.ToRadians((float)RandomDouble(-100, 100));
                                        Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                                        dir.Normalize();

                                        Gib newGib = new Gib(Splodge, new Vector2(hitPlayer.DestinationRectangle.Center.X, 
                                                                                  hitPlayer.DestinationRectangle.Center.Y), 
                                                             dir, 10, RandomTexture(DecalTexture1, DecalTexture2, DecalTexture3, DecalTexture4), Splodge, Color.Maroon);
                                        newGib.CurrentLevel = Level1;
                                        GibList.Add(newGib);                                        
                                    }


                                    //Hit in the head
                                    if (rocket.DestinationRectangle.Bottom < hitPlayer.DestinationRectangle.Top + 13)
                                    {

                                        float angle2 = (float)Math.Atan2(rocket.Velocity.Y, rocket.Velocity.X) + MathHelper.ToRadians((float)RandomDouble(-100, 100));
                                        Vector2 dir2 = new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2));
                                        dir2.Normalize();

                                        Gib newGib2 = new Gib(Player1Head, new Vector2(hitPlayer.DestinationRectangle.Center.X,
                                                                                      hitPlayer.DestinationRectangle.Center.Y),
                                                                 dir2, 10, RandomTexture(DecalTexture1, DecalTexture2, DecalTexture3, DecalTexture4), Splodge, Color.White);
                                        newGib2.CurrentLevel = Level1;
                                        GibList.Add(newGib2);
                                    }
                                }

                                hitPlayer.MakeRumble(250, new Vector2(0.1f, 0.9f));

                                hitPlayer.CurrentHP = 0;

                                //Gore1.Play();
                                PlayRandomSound(Gore2);
                                rocket.SourcePlayer.Score++;
                            }
                        }
                        #endregion

                        #region Update Bullets
                        foreach (Bullet bullet in BulletList)
                        {
                            bullet.Update(gameTime);

                            if (Level1.CollisionTileList.Any(Tile => Tile.BoundingBox.Intersects(bullet.CollisionRectangle)))
                            {
                                GunCollision1.Play(0.5f, 0, 0);
                                bullet.Active = false;

                                Emitter sparkEmitter = new Emitter(SparkTexture, bullet.Position, new Vector2(0, 360), new Vector2(2, 3), new Vector2(250, 450), 1f, true, new Vector2(0, 0), new Vector2(0, 0), Vector2.One, Color.Orange, Color.OrangeRed, 0.2f, 0.1f, 1, 2, false, new Vector2(0, 720), false, 0f, false, false, null, null, null, true);
                                EmitterList.Add(sparkEmitter);
                            }

                            if (Players.Any(Player =>
                                Player != null &&
                                bullet.SourcePlayer != Player &&
                                bullet.Active == true &&
                                Player.DestinationRectangle.Intersects(bullet.DestinationRectangle)))
                            {
                                bullet.Active = false;
                                Player hitPlayer = Players.First(Player => Player.DestinationRectangle.Intersects(bullet.DestinationRectangle) && Player != bullet.SourcePlayer);

                                if (hitPlayer.CurrentHP > 0)
                                {
                                    GunCollision1.Play(0.2f, 0, 0);

                                    for (int i = 0; i < 20; i++)
                                    {
                                        float angle = (float)Math.Atan2(bullet.Velocity.Y, bullet.Velocity.X) + MathHelper.ToRadians((float)RandomDouble(-100, 100));
                                        Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                                        dir.Normalize();

                                        Gib newGib = new Gib(Splodge, new Vector2(hitPlayer.DestinationRectangle.Center.X,
                                                                                  hitPlayer.DestinationRectangle.Center.Y),
                                                             dir, 10, RandomTexture(DecalTexture1, DecalTexture2, DecalTexture3, DecalTexture4), Splodge, Color.Maroon);
                                        newGib.CurrentLevel = Level1;
                                        GibList.Add(newGib);
                                    }


                                    //Hit in the head
                                    if (bullet.DestinationRectangle.Bottom < hitPlayer.DestinationRectangle.Top + 13)
                                    {

                                        float angle2 = (float)Math.Atan2(bullet.Velocity.Y, bullet.Velocity.X) + MathHelper.ToRadians((float)RandomDouble(-100, 100));
                                        Vector2 dir2 = new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2));
                                        dir2.Normalize();

                                        Gib newGib2 = new Gib(hitPlayer.HeadTexture, new Vector2(hitPlayer.DestinationRectangle.Center.X,
                                                                                      hitPlayer.DestinationRectangle.Center.Y),
                                                                 dir2, 10, RandomTexture(DecalTexture1, DecalTexture2, DecalTexture3, DecalTexture4), Splodge, Color.White);
                                        newGib2.CurrentLevel = Level1;
                                        GibList.Add(newGib2);
                                    }
                                }

                                hitPlayer.CurrentHP = 0;
                                hitPlayer.MakeRumble(250, new Vector2(0.1f, 0.9f));

                                //Gore1.Play();
                                PlayRandomSound(Gore2);
                                bullet.SourcePlayer.Score++;
                            }
                        }
                        #endregion

                        foreach (Grenade grenade in GrenadeList)
                        {
                            grenade.Update(gameTime);

                            if (grenade.Active == false)
                            {
                                CreateExplosion(new Explosion() 
                                { 
                                    Position = grenade.Position, 
                                    BlastRadius = grenade.BlastRadius, 
                                    Damage = 1,
                                    Source = grenade.Source
                                }, grenade);
                            }
                        }

                        RocketList.RemoveAll(Rocket => Rocket.Active == false);
                        BulletList.RemoveAll(Bullet => Bullet.Active == false);
                        GrenadeList.RemoveAll(Grenade => Grenade.Active == false);
                    }
                    break;
                #endregion

                #region PAUSED
                case GameState.Paused:
                    {

                    }
                    break;
                #endregion

                #region MAIN MENU
                case GameState.MainMenu:
                    {
                        foreach (Player player in Players)
                        {
                            player.Update(gameTime);

                            if (player.DPadDown == true)
                            {                                
                                MenuIndex++;
                            }

                            if (player.DPadUp == true)
                            {
                                MenuIndex--;
                            }

                            if (player.PressedStart)
                            {
                                

                                Level1 = new Level();
                                Level1 = Load(MainMenu.Files[MenuIndex]);
                                Level1.LoadContent(Content);

                                CheckPlayers();

                                foreach (Player otherPlayers in Players)
                                {
                                    otherPlayers.Active = true;
                                    otherPlayers.LoadContent(Content);
                                }

                                CurrentGameState = GameState.Game;
                            }
                        }
                    }
                    break;
                #endregion

                #region END SCREEN
                case GameState.EndScreen:
                    {
                        foreach (Player player in Players)
                        {
                            player.Active = false;

                            player.Update(gameTime);

                            if (player.DPadDown == true)
                            {
                                MenuIndex++;
                            }

                            if (player.DPadUp == true)
                            {
                                MenuIndex--;
                            }

                            if (player.PressedStart)
                            {
                                foreach (Player otherPlayers in Players)
                                {
                                    otherPlayers.Score = 0;
                                    otherPlayers.GrenadeAmmo = 3;
                                    otherPlayers.Active = false;
                                }

                                GibList.Clear();
                                Level1 = null;
                                EmitterList.Clear();
                                EmitterList2.Clear();
                                AmmoList.Clear();
                                HealthList.Clear();

                                CurrentGameState = GameState.MainMenu;
                            }
                        }
                    }
                    break;
                #endregion
            }            

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            
            switch (CurrentGameState)
            {
                #region GAME
                case GameState.Game:
                    {
                        #region First Batch
                        spriteBatch.Begin();

                        spriteBatch.Draw(Background1, Vector2.Zero, Color.White);
                        //Level1.Draw(spriteBatch);           
                        Level1.DrawTiles(spriteBatch);
                        Level1.DrawCollisions(spriteBatch);
                        Level1.DrawBackgroundTiles(spriteBatch);

                        foreach (Player player in Players.Where(Player => Player != null))
                        {
                            player.Draw(spriteBatch);
                        }

                        foreach (AmmoPack pack in AmmoList)
                        {
                            pack.Draw(spriteBatch);
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

                        foreach (Gib gib in GibList)
                        {
                            gib.Draw(spriteBatch);
                        }
                        spriteBatch.End(); 
                        #endregion

                        #region Second Batch
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

                        foreach (Bullet bullet in BulletList)
                        {
                            bullet.Draw(spriteBatch);
                        }
                        spriteBatch.End();
                        #endregion
                    }
                    break;
                #endregion

                #region MAIN MENU

                case GameState.MainMenu:
                    {
                        spriteBatch.Begin();
                        MainMenu.Draw(spriteBatch);

                        string[] files = Directory.GetFiles("C:\\LevelData\\");
                        MainMenu.Files = files;

                        for (int i = 0; i < files.Length; i++)
                        {
                            if (i == MenuIndex)
                                spriteBatch.DrawString(Font, files[i], new Vector2(128, 128 + (32 * i)), Color.White);
                            else
                                spriteBatch.DrawString(Font, files[i], new Vector2(128, 128 + (32 * i)), Color.Gray);
                        }
                         
                        spriteBatch.End();
                    }
                    break;
                #endregion

                #region END SCREEN
                case GameState.EndScreen:
                    {
                        spriteBatch.Begin();
                        spriteBatch.DrawString(Font, "GAME OVER.", new Vector2(1280 / 2, 64), Color.White);

                        //Determine winner
                        Player[] endPlayers = Players.OrderByDescending(Player => Player.Score).ToArray();

                        string append = "";

                        Color nameColor = Color.White;

                        for (int i = 0; i < endPlayers.Count(); i++)
                        {
                            if (i == 0)
                            {
                                append = "Winner";
                                nameColor = Color.Gold;
                            }
                            else
                            {
                                append = "Loser";
                                nameColor = Color.White;
                            }

                            //string countKills = "";

                            //for (int p = 0; p < endPlayers[i].Score; p++)
                            //{
                            //    countKills += "|";
                            //}

                            spriteBatch.DrawString(Font, append + " Player " + endPlayers[i].PlayerIndex.ToString() + " : " + endPlayers[i].Score.ToString() + " kills", new Vector2(1280 / 2, 250 + (i * 64)), nameColor);
                            GamePad.SetVibration((PlayerIndex)i, 0f, 0f);

                            endPlayers[i].StandRightAnimation.Draw(spriteBatch, new Vector2(1280/2 - 40, 250 + (i * 64) - 16));
                        }
                        spriteBatch.End();
                    }
                    break;
                #endregion
            }

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

            Players = Players.Where(Player => Player != null).ToArray();

            CheckedPlayers = true;            
        }

        private Level Load(string path)
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
                                        Color.Black, -0.2f, 0.1f, 5, 1, false, new Vector2(0, 720), false, 0f,
                                        null, null, null, null, null, null, new Vector2(0.1f, 0.2f), true, true, null, null, null, true);
            EmitterList2.Add(ExplosionEmitter);

            Emitter ExplosionEmitter3 = new Emitter(ExplosionParticle2,
                    new Vector2(explosion.Position.X, explosion.Position.Y+16),
                    new Vector2(85, 95), new Vector2(2, 4), new Vector2(400, 640), 0.35f, true, new Vector2(0, 0),
                    new Vector2(0, 0), new Vector2(0.085f, 0.2f), FireColor, ExplosionColor3, -0.1f, 0.05f, 5, 1, false,
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
                    Gore1.Play();
                    player.CurrentHP = 0;
                    

                    for (int i = 0; i < 40; i++)
                    {
                        Vector2 myDir = new Vector2(player.DestinationRectangle.Center.X, player.DestinationRectangle.Center.Y) - explosion.Position;
                        myDir.Normalize();

                        float angle = (float)Math.Atan2(myDir.Y, myDir.X) + MathHelper.ToRadians((float)RandomDouble(-130, 130));
                        Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                        dir.Normalize();

                        Gib newGib = new Gib(Splodge, new Vector2(player.DestinationRectangle.Center.X,
                                                                  player.DestinationRectangle.Center.Y),
                                             dir, 10, RandomTexture(DecalTexture1, DecalTexture2, DecalTexture3, DecalTexture4), Splodge, Color.Maroon );
                        newGib.CurrentLevel = Level1;
                        GibList.Add(newGib);
                    }

                    if (e.Explosion.Source == player)
                    {
                        player.Score--;
                    }
                    else
                    {
                        (e.Explosion.Source as Player).Score++;
                    }

                    player.MakeRumble(250, new Vector2(0.9f, 0.1f));
                }
            }

        }

        public void PlayRandomSound(params SoundEffect[] soundEffect)
        {
            soundEffect[Random.Next(0, soundEffect.Length)].Play(0.5f, (float)RandomDouble(-0.25, 0.25), 0);
        }

        public Texture2D RandomTexture(params Texture2D[] textures)
        {
            return textures[Random.Next(0, textures.Length)];
        }

        public double RandomDouble(double a, double b)
        {
            return a + Random.NextDouble() * (b - a);
        }

    }
}
