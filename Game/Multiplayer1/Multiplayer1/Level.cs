using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Multiplayer1
{
    public class Level
    {
        public List<List<Tile>> MainTileList = new List<List<Tile>>();
        public List<string> LinesList = new List<string>();

        public Level(string levelName)
        {
            using (StreamReader r = new StreamReader("C:\\LevelData\\" + levelName + ".txt"))
            {
                string Line;

                while ((Line = r.ReadLine()) != null)
                {
                    LinesList.Add(Line);
                }

                for (int y = 0; y < LinesList.Count; y++)
                {
                    List<Tile> SubList = new List<Tile>();

                    for (int x = 0; x < LinesList[y].Length; x++)
                    {
                        if (LinesList[y][x] != '0')
                        {
                            SubList.Add(new Tile(new Vector2(x * 32, y * 32), new Vector2(32, 32)));
                        }
                    }

                    MainTileList.Add(SubList);
                }
            }
        }

        public void LoadContent(ContentManager contentManager)
        {
            for (int y = 0; y < MainTileList.Count; y++)
            {
                for (int x = 0; x < MainTileList[y].Count; x++)
                {
                    MainTileList[y][x].LoadContent(contentManager);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int y = 0; y < MainTileList.Count; y++)
            {
                for (int x = 0; x < MainTileList[y].Count; x++)
                {
                    MainTileList[y][x].Draw(spriteBatch);
                }
            }
        }
    }
}
