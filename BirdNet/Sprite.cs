using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirdNet
{
    class Sprite
    {
        public Texture2D Texture { get; set; }
        public Rectangle Frame
        {
            get
            {
                return new Rectangle(this.frameCounter * (this.Texture.Width / this.frames), 0, this.Texture.Width / this.frames, this.Texture.Height);
            }
        }

        private int frameDelay, frameDelayCounter, frames, frameCounter;

        public Sprite(Texture2D texture, int frames, int frameDelay)
        {
            this.Texture = texture;
            this.frames = frames;
            this.frameDelay = frameDelay;
            this.frameDelayCounter = 0;
            this.frameCounter = 0;
        }

        public void Update()
        {
            this.frameDelayCounter += 1;

            if (this.frameDelayCounter == frameDelay)
            {
                this.frameCounter += 1;
                frameDelayCounter = 0;
                if (this.frameCounter == this.frames)
                    this.frameCounter = 0;
            }
        }

        public void Draw(SpriteBatch sb, Vector2 pos)
        {
            sb.Draw(this.Texture, pos, this.Frame, Color.White);
        }
    }
}
