﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Multiplayer1
{
    public class Animation
    {
        public Texture2D Texture;
        public Rectangle DestinationRectangle, SourceRectangle;
        public Vector2 FrameSize;
        public int TotalFrames, CurrentFrame;
        public float FrameTime, CurrentFrameTime;
        public Vector2 Position;

        public Animation(Texture2D texture, int totalFrames, float frameDelay)
        {
            FrameTime = frameDelay;
            TotalFrames = totalFrames;
            Texture = texture;

            if (TotalFrames > 1)
                FrameSize = new Vector2(Texture.Width / TotalFrames, Texture.Height);
            else
                FrameSize = new Vector2(Texture.Width, Texture.Height);
        }

        public void Update(GameTime gameTime)
        {
            if (CurrentFrameTime < FrameTime)
            {
                CurrentFrameTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            if (CurrentFrameTime >= FrameTime)
            {
                CurrentFrameTime = 0;

                CurrentFrame++;

                if (CurrentFrame >= TotalFrames)
                {
                    CurrentFrame = 0;
                }
            }

            SourceRectangle = new Rectangle((int)(FrameSize.X * CurrentFrame), 0, (int)FrameSize.X, (int)FrameSize.Y);
            DestinationRectangle = new Rectangle((int)Position.X, (int)Position.Y, (int)FrameSize.X, (int)FrameSize.Y);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, DestinationRectangle, SourceRectangle, Color.White);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 pos)
        {
            spriteBatch.Draw(Texture, new Rectangle((int)pos.X, (int)pos.Y, (int)FrameSize.X, (int)FrameSize.Y) , SourceRectangle, Color.White, 0, new Vector2(FrameSize.X/2, FrameSize.Y), SpriteEffects.None, 0);
        }
    }
}
