using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LevelEditor1
{
    enum ObjectType { Teleporter, SpawnPoint, BouncePad };
    class GameObject : Tile
    {
        public GameObject()
        {

        }

        public void LoadContent(ContentManager content)
        {

        }
    }
}
