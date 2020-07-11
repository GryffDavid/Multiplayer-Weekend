using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Multiplayer1
{
    class Rocket
    {
        Texture2D RocketTexture;
        public Player SourcePlayer;
        public float Damage, BlastRadius;
        public Vector2 Velocity, Direction, Position;
        public float Rotation;
        public Rectangle DestinationRectangle;
        public bool Active = true;

        public Rocket(Vector2 position, Texture2D texture, float speed, Vector2 direction, Player sourcePlayer)
        {
            Position = position;
            RocketTexture = texture;
            Direction = direction;
            Velocity = Direction * speed;
            SourcePlayer = sourcePlayer;
        }

        public void Update(GameTime gameTime)
        {
            if (Active == true)
            {
                Position += Velocity;
                Rotation = (float)Math.Atan2(Velocity.Y, Velocity.X);
                DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, RocketTexture.Width, RocketTexture.Height);
            }
        }

        public void LoadContent(ContentManager contentManager)
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active == true)
            {
                    spriteBatch.Draw(RocketTexture, DestinationRectangle, null, Color.White, Rotation, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
            }
        }
    }
}
