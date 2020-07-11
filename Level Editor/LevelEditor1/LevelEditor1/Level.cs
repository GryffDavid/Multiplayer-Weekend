using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace LevelEditor1
{
    public enum LevelType { CTF, DeathMatch, TeamDeathMatch };

    public class Level
    {
        public List<List<Tile>> MainTileList = new List<List<Tile>>();
        public List<List<Tile>> ForegroundTileList = new List<List<Tile>>();
        public List<List<Tile>> BackgroundTileList = new List<List<Tile>>();
        public List<CollisionTile> CollisionTileList = new List<CollisionTile>();

        public Level()
        {    
            
        }

        public void Initialize()
        {
            for (int y = 0; y < 46; y++)
            {
                List<Tile> SubList = new List<Tile>();

                for (int x = 0; x < 80; x++)
                {
                    SubList.Add(new Tile());
                }

                MainTileList.Add(SubList);
            }

            for (int y = 0; y < 46; y++)
            {
                List<Tile> SubList = new List<Tile>();

                for (int x = 0; x < 80; x++)
                {
                    SubList.Add(new Tile());
                }

                ForegroundTileList.Add(SubList);
            }

            for (int y = 0; y < 46; y++)
            {
                List<Tile> SubList = new List<Tile>();

                for (int x = 0; x < 80; x++)
                {
                    SubList.Add(new Tile());
                }

                BackgroundTileList.Add(SubList);
            }
        }

        public void LoadContent(ContentManager contentManager)
        {
            #region Load  main tiles content
            for (int y = 0; y < MainTileList.Count; y++)
            {
                for (int x = 0; x < MainTileList[y].Count; x++)
                {
                    MainTileList[y][x].Position = new Vector2(x * 16, y * 16);
                    MainTileList[y][x].Size = new Vector2(16, 16);
                    MainTileList[y][x].LoadContent(contentManager);
                }
            }
            #endregion

            #region Load foreground content
            for (int y = 0; y < ForegroundTileList.Count; y++)
            {
                for (int x = 0; x < ForegroundTileList[y].Count; x++)
                {
                    ForegroundTileList[y][x].Position = new Vector2(x * 16, y * 16);
                    ForegroundTileList[y][x].Size = new Vector2(16, 16);
                    ForegroundTileList[y][x].LoadContent(contentManager);
                }
            }
            #endregion

            #region Load background content
            for (int y = 0; y < ForegroundTileList.Count; y++)
            {
                for (int x = 0; x < ForegroundTileList[y].Count; x++)
                {
                    BackgroundTileList[y][x].Position = new Vector2(x * 16, y * 16);
                    BackgroundTileList[y][x].Size = new Vector2(16, 16);
                    BackgroundTileList[y][x].LoadContent(contentManager);
                }
            }
            #endregion

            foreach (CollisionTile tile in CollisionTileList)
            {
                tile.LoadContent(contentManager);
            }
        }

        public void DrawTiles(SpriteBatch spriteBatch)
        {
            for (int y = 0; y < MainTileList.Count; y++)
            {
                for (int x = 0; x < MainTileList[y].Count; x++)
                {
                    MainTileList[y][x].Draw(spriteBatch);
                }
            }
        }

        public void DrawForegroundTiles(SpriteBatch spriteBatch)
        {
            for (int y = 0; y < ForegroundTileList.Count; y++)
            {
                for (int x = 0; x < ForegroundTileList[y].Count; x++)
                {
                    ForegroundTileList[y][x].Draw(spriteBatch);
                }
            }
        }

        public void DrawBackgroundTiles(SpriteBatch spriteBatch)
        {
            for (int y = 0; y < BackgroundTileList.Count; y++)
            {
                for (int x = 0; x < BackgroundTileList[y].Count; x++)
                {
                    BackgroundTileList[y][x].Draw(spriteBatch);
                }
            }
        }

        public void DrawCollisions(SpriteBatch spriteBatch)
        {
            foreach (CollisionTile tile in CollisionTileList)
            {
                tile.Draw(spriteBatch);
            }
        }
    }
}
