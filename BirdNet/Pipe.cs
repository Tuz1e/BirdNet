using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirdNet
{
    class Pipe
    {
        public Vector2 Position { get; set; }
        public Vector2 Movement { get; set; }
        public Texture2D Texture { get; set; }
        public bool Passed { get; set; } //If has been passed

        public bool OutOfBounds;

        public Rectangle Hitbox
        {
            get
            {
                return new Rectangle(
                    (int)this.Position.X, (int)this.Position.Y, 
                    this.Texture.Width, this.Texture.Height);
            }
        }

        public Pipe(Vector2 pos, Vector2 movement, Texture2D txtr)
        {
            this.Position = pos;
            this.Movement = movement;
            this.Texture = txtr;
        }

        public void Update(GraphicsDevice gd)
        {
            this.Position += this.Movement;
            if (this.Position.X + this.Texture.Width < 0)
                this.OutOfBounds = true;
        }

        public void SetPassed(bool state)
        {
            this.Passed = state;
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(this.Texture, this.Position, Color.White);
        }
    }
}
