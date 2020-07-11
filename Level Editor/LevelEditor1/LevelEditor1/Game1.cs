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

namespace LevelEditor1
{
    public enum CollisionType { Solid, OneWay };
    enum PlacementMode { Tiles, Collision, Object, Foreground, Background };

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Vector2 SelectedTilePosition;
        Vector2 SelectedTileIndex;
        Vector2 PlaceTileSize = new Vector2(32, 32);
        Texture2D LevelSizeTemplate, TilesCollection, TileSelect, CollisionTiles, BasicTile;
        Level Level;
        SpriteFont Font;
        PlacementMode CurrentMode = PlacementMode.Tiles;
        

        KeyboardState CurrentKeyboardState, PreviousKeyboardState;
        MouseState CurrentMouseState, PreviousMouseState;

        bool CollisionMaskVisible, ForegroundVisible, TilesVisible, ObjectsVisible, BackgroundVisible;
        Tabs ModeTabs;

        Vector2 newTilePosition;

        Sprite CurrentTileSelection, LevelTemplateSprite;

        Toggle CollisionsToggle, TilesToggle, ObjectsToggle, ForegoundToggle, BackgroundToggle;
        SizeSelect xSizeSelect, ySizeSelect, xSnapSelect, ySnapSelect;

        //Background
        //Tiles
        //CollisionMask
        //Objects
        //Foreground
        //Levels are 640*384 PIXELS big
        //Levels are 48*24 TILES big
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 1080;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }
        
        protected override void Initialize()
        {
            
            CollisionMaskVisible = true;
            //BackgroundVisible = true;
            ForegroundVisible = true;
            TilesVisible = true;
            ObjectsVisible = true;

            
            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            LevelSizeTemplate = Content.Load<Texture2D>("LevelSize");
            TilesCollection = Content.Load<Texture2D>("TilesCollection");
            CollisionTiles = Content.Load<Texture2D>("CollisionTiles");
            TileSelect = Content.Load<Texture2D>("TileSelect");
            Font = Content.Load<SpriteFont>("Font");
            BasicTile = Content.Load<Texture2D>("BasicTile");

            Level = new Level();
            Level.Initialize();
            Level.LoadContent(Content);

            ModeTabs = new Tabs(new Vector2(0, LevelSizeTemplate.Height), BasicTile, Font, new Vector2(640, 32), "Tiles", "Collision", "Object", "Foreground", "Background");
            CurrentTileSelection = new Sprite(new Vector2(0, LevelSizeTemplate.Height + 32), TilesCollection);
            LevelTemplateSprite = new Sprite(new Vector2(0, 0), LevelSizeTemplate);

            CollisionsToggle = new Toggle(new Vector2(0, LevelSizeTemplate.Height + CurrentTileSelection.DestinationRectangle.Height + 32), "Collisions");
            CollisionsToggle.LoadContent(Content);

            TilesToggle = new Toggle(new Vector2(0, LevelSizeTemplate.Height + CurrentTileSelection.DestinationRectangle.Height + 32 + 16), "Tiles");
            TilesToggle.LoadContent(Content);

            ObjectsToggle = new Toggle(new Vector2(0, LevelSizeTemplate.Height + CurrentTileSelection.DestinationRectangle.Height + 32 + 32), "Objects");
            ObjectsToggle.LoadContent(Content);

            ForegoundToggle = new Toggle(new Vector2(0, LevelSizeTemplate.Height + CurrentTileSelection.DestinationRectangle.Height + 32 + 48), "Foreground");
            ForegoundToggle.LoadContent(Content);

            BackgroundToggle = new Toggle(new Vector2(0, LevelSizeTemplate.Height + CurrentTileSelection.DestinationRectangle.Height + 32 + 64), "Background");
            BackgroundToggle.LoadContent(Content);

            xSizeSelect = new SizeSelect(new Vector2(128, LevelSizeTemplate.Height + CurrentTileSelection.DestinationRectangle.Height + 32), "X Size");
            xSizeSelect.LoadContent(Content);

            ySizeSelect = new SizeSelect(new Vector2(128, LevelSizeTemplate.Height + CurrentTileSelection.DestinationRectangle.Height + 48), "Y Size");
            ySizeSelect.LoadContent(Content);

            xSnapSelect = new SizeSelect(new Vector2(128, LevelSizeTemplate.Height + CurrentTileSelection.DestinationRectangle.Height + 64 + 16), "X Snap");
            xSnapSelect.LoadContent(Content);

            ySnapSelect = new SizeSelect(new Vector2(128, LevelSizeTemplate.Height + CurrentTileSelection.DestinationRectangle.Height + 64 + 32), "Y Snap");
            ySnapSelect.LoadContent(Content);
        }
        
        protected override void UnloadContent()
        {

        }
        
        protected override void Update(GameTime gameTime)
        {
            CurrentMouseState = Mouse.GetState();
            CurrentKeyboardState = Keyboard.GetState();

            CollisionsToggle.Update();
            CollisionMaskVisible = CollisionsToggle.Selected;

            TilesToggle.Update();
            TilesVisible = TilesToggle.Selected;

            ObjectsToggle.Update();
            ObjectsVisible = ObjectsToggle.Selected;

            ForegoundToggle.Update();
            ForegroundVisible = ForegoundToggle.Selected;

            BackgroundToggle.Update();
            BackgroundVisible = BackgroundToggle.Selected;

            xSizeSelect.Update();
            ySizeSelect.Update();
            PlaceTileSize = new Vector2(xSizeSelect.CurrentSize, ySizeSelect.CurrentSize);

            #region Update the current mode
            ModeTabs.Update(gameTime);
            CurrentMode = (PlacementMode)(ModeTabs.SelectedIndex);
            #endregion

            #region Load level
            if (PreviousKeyboardState.IsKeyDown(Keys.Enter) &&
                CurrentKeyboardState.IsKeyUp(Keys.Enter))
            {
                //Level.MainTileList.Clear();
                Level = LoadLevel();
                Level.LoadContent(Content);
            }
            #endregion

            #region Save level
            if (PreviousKeyboardState.IsKeyDown(Keys.Space) &&
                CurrentKeyboardState.IsKeyUp(Keys.Space))
            {
                List<Level> LevelList = new List<Level>();
                LevelList.Add(Level);

                SaveLevel(Level);
            }
            #endregion

            switch (CurrentMode)
            {
                #region Tiles Mode
                case PlacementMode.Tiles:
                    CurrentTileSelection.Texture = TilesCollection;
                    CurrentTileSelection.Update();

                    #region Place a new tile
                    if (LevelTemplateSprite.DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)))
                    {
                        newTilePosition.X = (CurrentMouseState.X - CurrentMouseState.X % xSizeSelect.CurrentSize);
                    }

                    if (CurrentMouseState.LeftButton == ButtonState.Pressed &&
                        CurrentMouseState.Y < LevelSizeTemplate.Height &&
                        CurrentMouseState.Y > 0 &&
                        CurrentMouseState.X < LevelSizeTemplate.Width &&
                        CurrentMouseState.X > 0)
                    {
                        Vector2 tileXY = newTilePosition / 16;

                        for (int x = 0; x < xSizeSelect.CurrentSize/16; x++)
                        {
                            for (int y = 0; y < ySizeSelect.CurrentSize / 16; y++)
                            {
                                Tile newTile = new Tile();
                                newTile.Size = new Vector2(16, 16);
                                newTile.TileChar = new Vector2(SelectedTileIndex.X + x, SelectedTileIndex.Y + y);
                                newTile.Position = new Vector2((tileXY.X + x) * 16, (tileXY.Y + y) * 16);
                                newTile.LoadContent(Content);

                                Level.MainTileList[(int)tileXY.Y + y][(int)tileXY.X + x] = newTile;
                            }
                        }
                    }

                    #region Fill
                    if (CurrentKeyboardState.IsKeyUp(Keys.F) == true &&
                        PreviousKeyboardState.IsKeyDown(Keys.F) == true)
                    {

                    }
                    #endregion
                    #endregion

                    #region Select another tile
                    if (CurrentMouseState.LeftButton == ButtonState.Released &&
                        PreviousMouseState.LeftButton == ButtonState.Pressed &&
                        CurrentTileSelection.DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)))
                    {
                        if (CurrentMouseState.Y >= LevelSizeTemplate.Height + 32)
                        {
                            SelectedTilePosition = new Vector2(CurrentMouseState.X - CurrentMouseState.X % 16, CurrentMouseState.Y - CurrentMouseState.Y % 16);
                            SelectedTileIndex.X = (int)(SelectedTilePosition.X / 16);
                            SelectedTileIndex.Y = (int)(SelectedTilePosition.Y - LevelSizeTemplate.Height - 32) / 16;
                        }
                    }
                    #endregion
                    
                    #region Remove tile by right clicking
                    if (LevelTemplateSprite.DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)))
                    {
                        newTilePosition = new Vector2((CurrentMouseState.X - CurrentMouseState.X % xSizeSelect.CurrentSize), (CurrentMouseState.Y - CurrentMouseState.Y % ySizeSelect.CurrentSize));
                    }

                    if (CurrentMouseState.RightButton == ButtonState.Pressed &&
                        CurrentMouseState.Y < LevelSizeTemplate.Height &&
                        CurrentMouseState.Y > 0 &&
                        CurrentMouseState.X < LevelSizeTemplate.Width &&
                        CurrentMouseState.X > 0)
                    {
                        Vector2 tileXY = newTilePosition / 16;

                        for (int x = 0; x < xSizeSelect.CurrentSize/16; x++)
                        {
                            for (int y = 0; y < ySizeSelect.CurrentSize / 16; y++)
                            {
                                Tile newTile = new Tile();
                                newTile.TileChar = new Vector2(0, 0);
                                newTile.LoadContent(Content);

                                Level.MainTileList[(int)tileXY.Y + y][(int)tileXY.X + x] = newTile;
                            }
                        }
                    }
                    #endregion

                    break;
                #endregion

                #region Collisions Mode
                case PlacementMode.Collision:
                    CurrentTileSelection.Texture = CollisionTiles;
                    CurrentTileSelection.Update();

                    #region Place a new collision tile
                    if (LevelTemplateSprite.DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)))
                    {
                        newTilePosition = new Vector2((CurrentMouseState.X - CurrentMouseState.X % xSizeSelect.CurrentSize), (CurrentMouseState.Y - CurrentMouseState.Y % ySizeSelect.CurrentSize));
                    }

                    if (CurrentMouseState.LeftButton == ButtonState.Pressed &&
                        CurrentMouseState.Y < LevelSizeTemplate.Height &&
                        CurrentMouseState.Y > 0 &&
                        CurrentMouseState.X < LevelSizeTemplate.Width &&
                        CurrentMouseState.X > 0)
                    {
                        Vector2 tileXY = newTilePosition / 16;

                        CollisionTile newCollision = new CollisionTile();
                        newCollision.Position = new Vector2(tileXY.X * 16, tileXY.Y * 16);
                        newCollision.Size = new Vector2(xSizeSelect.CurrentSize, ySizeSelect.CurrentSize);
                        newCollision.TileChar = SelectedTileIndex;
                        newCollision.LoadContent(Content);

                        //Stops the ability to place multiple collision tiles on top of one another
                        if (Level.CollisionTileList.Find(Tile => Tile.Position == new Vector2(tileXY.X * 16, tileXY.Y * 16)) == null)
                        {
                            Level.CollisionTileList.Add(newCollision);
                        }
                        else
                        {
                            Level.CollisionTileList.RemoveAt(Level.CollisionTileList.FindIndex(Tile => Tile.Position == new Vector2(tileXY.X * 16, tileXY.Y * 16)));
                            Level.CollisionTileList.Add(newCollision);
                        }
                    }
                    #endregion

                    #region Select another tile
                    if (CurrentMouseState.LeftButton == ButtonState.Released &&
                        PreviousMouseState.LeftButton == ButtonState.Pressed &&
                        CurrentTileSelection.DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)))
                    {
                        if (CurrentMouseState.Y >= LevelSizeTemplate.Height)
                        {
                            SelectedTilePosition = new Vector2(CurrentMouseState.X - CurrentMouseState.X % 16, CurrentMouseState.Y - CurrentMouseState.Y % 16);
                            SelectedTileIndex.X = (int)(SelectedTilePosition.X / 16);
                            SelectedTileIndex.Y = (int)(SelectedTilePosition.Y - LevelSizeTemplate.Height - 16) / 16;
                        }
                    }
                    #endregion
                                        
                    if (CurrentMouseState.RightButton == ButtonState.Pressed &&
                        CurrentMouseState.Y < LevelSizeTemplate.Height &&
                        CurrentMouseState.Y > 0 &&
                        CurrentMouseState.X < LevelSizeTemplate.Width &&
                        CurrentMouseState.X > 0)
                    {
                        if (Level.CollisionTileList.Exists(Tile => Tile.DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y))))
                        {
                            int removeIndex = Level.CollisionTileList.FindIndex(Tile => Tile.DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)));
                            Level.CollisionTileList.RemoveAt(removeIndex);
                        }
                    }
                    break;
                #endregion

                #region Foreground
                case PlacementMode.Foreground:
                    CurrentTileSelection.Texture = TilesCollection;
                    CurrentTileSelection.Update();

                    #region Place a new tile
                    if (LevelTemplateSprite.DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)))
                    {
                        newTilePosition = new Vector2((CurrentMouseState.X - CurrentMouseState.X % xSizeSelect.CurrentSize), (CurrentMouseState.Y - CurrentMouseState.Y % ySizeSelect.CurrentSize));
                    }

                    if (CurrentMouseState.LeftButton == ButtonState.Pressed &&
                        CurrentMouseState.Y < LevelSizeTemplate.Height &&
                        CurrentMouseState.Y > 0 &&
                        CurrentMouseState.X < LevelSizeTemplate.Width &&
                        CurrentMouseState.X > 0)
                    {
                        Vector2 tileXY = newTilePosition / 16;

                        for (int x = 0; x < xSizeSelect.CurrentSize/16; x++)
                        {
                            for (int y = 0; y < ySizeSelect.CurrentSize / 16; y++)
                            {
                                Tile newTile = new Tile();
                                newTile.Size = new Vector2(16, 16);
                                newTile.TileChar = new Vector2(SelectedTileIndex.X + x, SelectedTileIndex.Y + y);
                                newTile.Position = new Vector2((tileXY.X + x) * 16, (tileXY.Y + y) * 16);
                                newTile.LoadContent(Content);

                                Level.ForegroundTileList[(int)tileXY.Y + y][(int)tileXY.X + x] = newTile;
                            }
                        }
                    }
                    #endregion

                    #region Select another tile
                    if (CurrentMouseState.LeftButton == ButtonState.Released &&
                        PreviousMouseState.LeftButton == ButtonState.Pressed &&
                        CurrentTileSelection.DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)))
                    {
                        if (CurrentMouseState.Y >= LevelSizeTemplate.Height + 32)
                        {
                            SelectedTilePosition = new Vector2(CurrentMouseState.X - CurrentMouseState.X % 16, CurrentMouseState.Y - CurrentMouseState.Y % 16);
                            SelectedTileIndex.X = (int)(SelectedTilePosition.X / 16);
                            SelectedTileIndex.Y = (int)(SelectedTilePosition.Y - LevelSizeTemplate.Height - 32) / 16;
                        }
                    }
                    #endregion

                    #region Remove tile by right clicking
                    if (LevelTemplateSprite.DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)))
                    {
                        newTilePosition = new Vector2((CurrentMouseState.X - CurrentMouseState.X % xSizeSelect.CurrentSize), (CurrentMouseState.Y - CurrentMouseState.Y % ySizeSelect.CurrentSize));
                    }

                    if (CurrentMouseState.RightButton == ButtonState.Pressed &&
                        CurrentMouseState.Y < LevelSizeTemplate.Height &&
                        CurrentMouseState.Y > 0 &&
                        CurrentMouseState.X < LevelSizeTemplate.Width &&
                        CurrentMouseState.X > 0)
                    {
                        Vector2 tileXY = newTilePosition / 16;

                        for (int x = 0; x < xSizeSelect.CurrentSize/16; x++)
                        {
                            for (int y = 0; y < ySizeSelect.CurrentSize / 16; y++)
                            {
                                Tile newTile = new Tile();
                                newTile.TileChar = new Vector2(0, 0);
                                newTile.LoadContent(Content);

                                Level.ForegroundTileList[(int)tileXY.Y + y][(int)tileXY.X + x] = newTile;
                            }
                        }
                    }
                    #endregion
                    break;
                #endregion

                #region Background
                case PlacementMode.Background:
                    CurrentTileSelection.Texture = TilesCollection;
                    CurrentTileSelection.Update();

                    #region Place a new tile
                    if (LevelTemplateSprite.DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)))
                    {
                        newTilePosition = new Vector2((CurrentMouseState.X - CurrentMouseState.X % xSizeSelect.CurrentSize), (CurrentMouseState.Y - CurrentMouseState.Y % ySizeSelect.CurrentSize));
                    }

                    if (CurrentMouseState.LeftButton == ButtonState.Pressed &&
                        CurrentMouseState.Y < LevelSizeTemplate.Height &&
                        CurrentMouseState.Y > 0 &&
                        CurrentMouseState.X < LevelSizeTemplate.Width &&
                        CurrentMouseState.X > 0)
                    {
                        Vector2 tileXY = newTilePosition / 16;

                        for (int x = 0; x < xSizeSelect.CurrentSize/16; x++)
                        {
                            for (int y = 0; y < ySizeSelect.CurrentSize / 16; y++)
                            {
                                Tile newTile = new Tile();
                                newTile.Size = new Vector2(16, 16);
                                newTile.TileChar = new Vector2(SelectedTileIndex.X + x, SelectedTileIndex.Y + y);
                                newTile.Position = new Vector2((tileXY.X + x) * 16, (tileXY.Y + y) * 16);
                                newTile.LoadContent(Content);

                                Level.BackgroundTileList[(int)tileXY.Y + y][(int)tileXY.X + x] = newTile;
                            }
                        }
                    }
                    #endregion

                    #region Select another tile
                    if (CurrentMouseState.LeftButton == ButtonState.Released &&
                        PreviousMouseState.LeftButton == ButtonState.Pressed &&
                        CurrentTileSelection.DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)))
                    {
                        if (CurrentMouseState.Y >= LevelSizeTemplate.Height + 32)
                        {
                            SelectedTilePosition = new Vector2(CurrentMouseState.X - CurrentMouseState.X % 16, CurrentMouseState.Y - CurrentMouseState.Y % 16);
                            SelectedTileIndex.X = (int)(SelectedTilePosition.X / 16);
                            SelectedTileIndex.Y = (int)(SelectedTilePosition.Y - LevelSizeTemplate.Height - 32) / 16;
                        }
                    }
                    #endregion

                    #region Remove tile by right clicking
                    if (LevelTemplateSprite.DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)))
                    {
                        newTilePosition = new Vector2((CurrentMouseState.X - CurrentMouseState.X % xSizeSelect.CurrentSize), (CurrentMouseState.Y - CurrentMouseState.Y % ySizeSelect.CurrentSize));
                    }

                    if (CurrentMouseState.RightButton == ButtonState.Pressed &&
                        CurrentMouseState.Y < LevelSizeTemplate.Height &&
                        CurrentMouseState.Y > 0 &&
                        CurrentMouseState.X < LevelSizeTemplate.Width &&
                        CurrentMouseState.X > 0)
                    {
                        Vector2 tileXY = newTilePosition / 16;

                        for (int x = 0; x < xSizeSelect.CurrentSize/16; x++)
                        {
                            for (int y = 0; y < ySizeSelect.CurrentSize / 16; y++)
                            {
                                Tile newTile = new Tile();
                                newTile.TileChar = new Vector2(0, 0);
                                newTile.LoadContent(Content);

                                Level.BackgroundTileList[(int)tileXY.Y + y][(int)tileXY.X + x] = newTile;
                            }
                        }
                    }
                    #endregion
                    break;
                #endregion
            }            

            PreviousKeyboardState = CurrentKeyboardState;
            PreviousMouseState = CurrentMouseState;
            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkGray);
            spriteBatch.Begin();

            //Draw the level size template
            LevelTemplateSprite.Draw(spriteBatch);

            //Draw the currently available tiles (Based on the placement mode)
            CurrentTileSelection.Draw(spriteBatch);

            ModeTabs.Draw(spriteBatch);

            if (BackgroundVisible == true)
                Level.DrawBackgroundTiles(spriteBatch);

            if (TilesVisible == true)
                Level.DrawTiles(spriteBatch);

            if (ForegroundVisible == true)
                Level.DrawForegroundTiles(spriteBatch);

            

            if (CollisionMaskVisible == true)
                Level.DrawCollisions(spriteBatch);

            spriteBatch.Draw(TileSelect, new Rectangle((int)SelectedTilePosition.X, (int)SelectedTilePosition.Y, xSizeSelect.CurrentSize, ySizeSelect.CurrentSize), Color.Lime);

            //Draw the white tile placement reticle
            if (LevelTemplateSprite.DestinationRectangle.Contains(new Point(CurrentMouseState.X, CurrentMouseState.Y)))
            {
                spriteBatch.Draw(TileSelect, new Rectangle((int)newTilePosition.X, (int)newTilePosition.Y, (int)PlaceTileSize.X, (int)PlaceTileSize.Y), Color.White);
            }

            TilesToggle.Draw(spriteBatch);
            CollisionsToggle.Draw(spriteBatch);
            ObjectsToggle.Draw(spriteBatch);
            ForegoundToggle.Draw(spriteBatch);
            BackgroundToggle.Draw(spriteBatch);

            xSizeSelect.Draw(spriteBatch);
            ySizeSelect.Draw(spriteBatch);

            xSnapSelect.Draw(spriteBatch);
            ySnapSelect.Draw(spriteBatch);

            spriteBatch.End();
            base.Draw(gameTime);
        }


        private Level LoadLevel()
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

        private void SaveLevel(Level level)
        {
            // Create a new XmlSerializer instance with the type of the test class
            XmlSerializer SerializerObj = new XmlSerializer(typeof(Level));

            // Create a new file stream to write the serialized object to a file
            TextWriter WriteFileStream = new StreamWriter("C:\\LevelData\\" + "NewLevelTest" + ".txt", false);
            SerializerObj.Serialize(WriteFileStream, Level);

            // Cleanup
            WriteFileStream.Close();
        }
    }
}
