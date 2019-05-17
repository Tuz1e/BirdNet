using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace BirdNet
{
    internal class Bird
    {
        #region Network specific

        public double[,] HiddenLayerWeights { get; set; }
        public double[,] OutputLayerWeights { get; set; }
        public float Fitness { get; set; } //Bird fitness

        public double[,] Input { get; set; }
        public double[,] Output { get; set; }
        public int[] Layers { get; set; }
        public int AliveTime;
        public bool AliveFlag;
        public int Score { get; set; }

        private float
            minValue = 0,
            minTowerY = 1,
            maxTowerY = 1,
            distanceToTower = 0,
            minDistanceToTower = 0,
            centerPos = 0;

        #endregion Network specific

        public Vector2 Position { get; set; }
        public Vector2 Movement { get; set; }
        public Sprite Sprite { get; set; }

        public Rectangle Hitbox
        {
            get
            {
                return new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Sprite.Frame.Width, this.Sprite.Frame.Height);
            }
        }

        public Bird(int[] layers, double[,] hidden, double[,] output, float fitness, Vector2 pos, Vector2 movement, Sprite sprite)
        {
            this.Layers = layers;
            this.HiddenLayerWeights = hidden;
            this.OutputLayerWeights = output;
            this.Fitness = fitness;

            this.Position = pos;
            this.Movement = movement;
            this.Sprite = sprite;
            this.AliveTime = 0;
            this.AliveFlag = true;

            this.Score = 0;
            this.Fitness = 0;
        }

        public Bird(int[] layers, Vector2 pos, Vector2 movement, Sprite sprite, float fitness = 0)
        {
            this.Layers = layers;
            this.Fitness = 0;
            this.HiddenLayerWeights = new double[layers[0], layers[1]];
            this.OutputLayerWeights = new double[layers[1], layers[2]];

            this.Position = pos;
            this.Movement = movement;
            this.Sprite = sprite;

            this.Score = 0;
            this.Fitness = fitness;

            this.AliveTime = 0;
            this.AliveFlag = true;
        }

        public Bird(Bird bird)
        {
            this.Layers = bird.Layers;
            this.Fitness = bird.Fitness;
            this.HiddenLayerWeights = bird.HiddenLayerWeights;
            this.OutputLayerWeights = bird.OutputLayerWeights;
            this.Position = bird.Position;
            this.Sprite = bird.Sprite;
            this.Score = bird.Score;
            this.Fitness = bird.Fitness;
            this.AliveTime = bird.AliveTime;
            this.AliveFlag = bird.AliveFlag;
        }

        public Bird()
        {
            this.Fitness = 0;
        }

        public void Update(GraphicsDevice gd, List<Pipe> pipeList)
        {
            if (!this.AliveFlag)
            {
                return;
            }

            if (this.Movement.Y > GameInfo.MaxPower) { this.Movement = new Vector2(0, GameInfo.MaxPower); }
            if (this.Movement.Y < -GameInfo.MaxPower) { this.Movement = new Vector2(0, -GameInfo.MaxPower); }

            this.Movement = new Vector2(this.Movement.X, this.Movement.Y + GameInfo.Gravity);

            if (FeedForward(gd, pipeList))
            {
                this.Movement = new Vector2(this.Movement.X, this.Movement.Y - GameInfo.Force);
            }

            this.Position += this.Movement;
            this.Sprite.Update();

            this.AliveTime += 1;
        }

        public void ModifyScore(int score)
        {
            this.Score += score;
        }

        public void SetAlive(Vector2 pos)
        {
            AliveFlag = true;
            this.Position = pos;
            this.AliveTime = 0;

            this.Score = 0;
        }

        public void SetDead()
        {
            AliveFlag = false;
            Fitness = (float)Math.Sqrt(AliveTime+Score);
        }

        public void SetFitness(float aValue)
        {
            Fitness = aValue;
        }

        public bool FeedForward(GraphicsDevice gd, List<Pipe> pipeList)
        {
            minValue = float.MaxValue;

            for (int i = 0; i < pipeList.Count - 1; i++)
            {
                distanceToTower = Math.Abs(pipeList[i].Position.X - this.Position.X - this.Hitbox.Width);

                if (distanceToTower < minValue)
                {
                    minValue = minDistanceToTower = distanceToTower;
                    maxTowerY = pipeList[i].Position.Y;

                    if (pipeList[i].Position.Y < pipeList[i + 1].Position.Y
                        && pipeList[i].Position.X == pipeList[i + 1].Position.X)
                    {
                        minTowerY = pipeList[i + 1].Position.Y;
                    }
                    else
                        minTowerY = maxTowerY - 3;

                    centerPos = (maxTowerY + minTowerY) / 2;
                }
            }

            Input = new double[1, Layers[0]];

            //Inputs, moved down for readability
            Input[0, 0] =
                1 - minDistanceToTower
                / (gd.PresentationParameters.BackBufferWidth - this.Position.X - this.Hitbox.Width);
            Input[0, 1] =
                (this.Position.Y + this.Hitbox.Height - maxTowerY)
                / gd.PresentationParameters.BackBufferHeight;
            Input[0, 2] =
                (this.Position.Y - minTowerY)
                / gd.PresentationParameters.BackBufferHeight;

            double[,] hiddenInputs = Multiply(Input, HiddenLayerWeights);
            double[,] hiddenOutputs = hiddenInputs.Sigmoid();

            Output = (Multiply(hiddenOutputs, OutputLayerWeights)).Sigmoid();

            return Output[0, 0] > 0.5;
        }

        public void Draw(SpriteBatch sb)
        {
            if (!this.AliveFlag)
                return;
            this.Sprite.Draw(sb, this.Position);
        }

        private double[,] Multiply(double[,] arr1, double[,] arr2)
        {
            double[,] arr = new double[arr1.GetLength(0), arr2.GetLength(1)];

            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    arr[i, j] = 0;
                    for (int k = 0; k < arr1.GetLength(1); k++)
                    {
                        arr[i, j] = arr[i, j] + arr1[i, k] * arr2[k, j];
                    }
                }
            }

            return arr;
        }
    }
}