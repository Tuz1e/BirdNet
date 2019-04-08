﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace BirdNet
{
    class Bird
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

        float
            minValue = 0,
            minTowerY = 1,
            maxTowerY = 1,
            distanceToTower = 0,
            minDistanceToTower = 0,
            centerPos = 0;

        #endregion

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
        }
        public Bird(int[] layers, Vector2 pos, Vector2 movement, Sprite sprite)
        {
            this.Layers = layers;
            this.Fitness = 0;
            this.HiddenLayerWeights = new double[layers[0], layers[1]];
            this.OutputLayerWeights = new double[layers[1], layers[2]];

            this.Position = pos;
            this.Movement = movement;
            this.Sprite = sprite;

            this.AliveTime = 0;
            this.AliveFlag = true;
        }

        public void Update(GraphicsDevice gd, List<Pipe> pipeList)
        {
            if (!this.AliveFlag)
                return;

            if (this.Movement.Y > GameInfo.MaxPower) { this.Movement = new Vector2(0, GameInfo.MaxPower); }
            if (this.Movement.Y < -GameInfo.MaxPower) { this.Movement = new Vector2(0, GameInfo.MaxPower); }

            this.Position += this.Movement;
            this.Sprite.Update();

            this.Movement = new Vector2(this.Movement.X, this.Movement.Y + GameInfo.Gravity);
            
            if (FeedForward(gd, pipeList))
            {
                this.Movement = new Vector2(this.Movement.X, this.Movement.Y - GameInfo.Force);
            }
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

        double[,] Multiply(double[,] arr1, double[,] arr2)
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
