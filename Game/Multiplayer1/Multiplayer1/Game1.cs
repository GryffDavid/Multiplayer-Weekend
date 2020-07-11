using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

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

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D RocketTexture;

        Texture2D SparkTexture, RoundSparkTexture;

        Player[] Players = new Player[4];

        bool CheckedPlayers = false;
        Level Level1;

        List<Rocket> RocketList = new List<Rocket>();

        SpriteFont Font;

        List<Emitter> EmitterList = new List<Emitter>();

        public SoundEffect GunShot1, GunCollision1;
        

        public void OnButtonClick(object source, PlayerShootEventArgs e)
        {
            GunShot1.Play(0.25f, 0, 0);
            RocketList.Add(new Rocket(e.Player.Position + new Vector2(0, 10), RocketTexture, 15f, e.Player.AimDirection, e.Player));
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
            base.Initialize();
        }
                
        protected override void LoadContent()
        {
            Level1 = new Level("Level1");
            Level1.LoadContent(Content);

            GunShot1 = Content.Load<SoundEffect>("SoundEffects/GunShot1");
            GunCollision1 = Content.Load<SoundEffect>("SoundEffects/GunCollision1");

            SparkTexture = Content.Load<Texture2D>("ParticleTextures/Spark");

            RocketTexture = Content.Load<Texture2D>("GunTexture");

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
            foreach (Emitter emitter in EmitterList)
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

                if (Level1.CollisionTileList.Any(TileList => TileList.Any(Tile => Tile.BoundingBox.Intersects(rocket.DestinationRectangle))))
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
                    Players.First(Player => Player.DestinationRectangle.Intersects(rocket.DestinationRectangle)).CurrentHP = 0;
                    rocket.SourcePlayer.Score++;
                }
            }

            RocketList.RemoveAll(Rocket => Rocket.Active == false);

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkGray);

            spriteBatch.Begin();
            Level1.Draw(spriteBatch);           
            
            foreach (Player player in Players.Where(Player => Player != null))
            {
                player.Draw(spriteBatch);
            }

            foreach (Rocket rocket in RocketList)
            {
                rocket.Draw(spriteBatch);
            }

            foreach (Player player in Players.Where(Player => Player != null))
            {
                spriteBatch.DrawString(Font, player.PlayerIndex.ToString() + ": " + player.Score.ToString(), new Vector2(32, 32 + (int)player.PlayerIndex * 32), Color.White);
            }

            spriteBatch.End();


            spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Additive);
            foreach (Emitter emitter in EmitterList)
            {
                emitter.Draw(spriteBatch);
            }
            Level1.Draw(spriteBatch);
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
                    Players[i].Initialize(OnButtonClick);
                    Players[i].CurrentLevel = Level1;
                }
            }

            CheckedPlayers = true;            
        }
    }
}
