using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Multiplayer1
{
    class Bullet
    {
        Texture2D BulletTexture;
        public Player SourcePlayer;
        public float Damage, BlastRadius;
        public Vector2 Velocity, Direction, Position;
        public float Rotation;
        public Rectangle DestinationRectangle;
        public bool Active = true;

        public Bullet(Vector2 position, Texture2D texture, float speed, Vector2 direction, Player sourcePlayer)
        {
            Position = position;
            BulletTexture = texture;
            Direction = direction;
            Velocity = Direction * speed;
            SourcePlayer = sourcePlayer;
        }

        public void Update(GameTime gameTime)
        {
            if (Active == true)
            {
                Position += Velocity;
                DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, BulletTexture.Width, BulletTexture.Height);
            }
        }

        public void LoadContent(ContentManager contentManager)
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                if (Velocity.X < 0)
                    spriteBatch.Draw(BulletTexture, DestinationRectangle, Color.White);
                else
                    spriteBatch.Draw(BulletTexture, DestinationRectangle, null, Color.White, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
            }
        }
    }
}
