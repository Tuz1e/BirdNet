using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace BirdNet
{
    internal class Data
    {
        public static double[,] Create(int num1, int num2, Random rand)
        {
            double[,] weight = new double[num1, num2];
            for (int i = 0; i < num1; i++)
            {
                for (int j = 0; j < num2; j++)
                {
                    weight[i, j] = rand.NextDouble();
                }
            }

            return weight;
        }

        public static List<Bird> CreateNet(
            int input, int hidden, int output, int population,
            Vector2 defaultBirdPosition, Vector2 defaultBirdMovement, Sprite birdSprite, Random rand)
        {
            List<Bird> birds = new List<Bird>();
            for (int i = 0; i < population; i++)
            {
                birds.Add(
                    new Bird(
                        new int[] { input, hidden, output },
                        Create(input, hidden, rand),
                        Create(hidden, output, rand),
                        0,
                        defaultBirdPosition,
                        defaultBirdMovement,
                        birdSprite));
            }

            return birds;
        }
    }
}