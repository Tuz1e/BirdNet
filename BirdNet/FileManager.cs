using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BirdNet
{
    internal class FileManager
    {
        /// <summary>
        /// Writes text into a previously created text document.
        /// </summary>
        /// <param name="aFileStream"></param>
        /// <param name="aStringToAdd"></param>
        public static void AddText(FileStream aFileStream, string aStringToAdd)
        {
            byte[] tempTextToWrite = new UTF8Encoding(true).GetBytes(aStringToAdd);
            aFileStream.Write(tempTextToWrite, 0, tempTextToWrite.Length);
        }

        public static void Write(Bird bird, int[] layers, int generation)
        {
            Debug.WriteLine("Saving bird...");

            using (FileStream fileStream = File.Create(GameInfo.NetFullPath))
            {
                AddText(fileStream, bird.Fitness + ";");
                AddText(fileStream, layers[0].ToString() + ";");
                AddText(fileStream, layers[1].ToString() + ";");
                AddText(fileStream, layers[2].ToString() + ";");

                for (int i = 0; i < bird.HiddenLayerWeights.GetLength(0); i++)
                {
                    for (int u = 0; u < bird.HiddenLayerWeights.GetLength(1); u++)
                    {
                        AddText(fileStream, bird.HiddenLayerWeights[i, u].ToString() + ";");
                    }
                }
                for (int i = 0; i < bird.OutputLayerWeights.GetLength(0); i++)
                {
                    for (int u = 0; u < bird.OutputLayerWeights.GetLength(1); u++)
                    {
                        AddText(fileStream, bird.OutputLayerWeights[i, u].ToString() + ";");
                    }
                }
            }
            Debug.WriteLine($"Bird saved. - GEN: {generation} - FIT: {bird.Fitness} - SCORE: {bird.Score}");
        }

        public static Bird Read(Vector2 defaultBirdPosition, Vector2 defaultBirdMovement, Sprite birdSprite)
        {
            string data = "X";
            using (FileStream tempFileStream = File.OpenRead(GameInfo.NetFullPath))
            {
                byte[] tempByteArray = new byte[1024];
                UTF8Encoding tempText = new UTF8Encoding(true);
                while (tempFileStream.Read(tempByteArray, 0, tempByteArray.Length) > 0)
                {
                    data = tempText.GetString(tempByteArray);
                }
            }

            string[] information = data.Split(';');
            float fitness = float.Parse(information[0]);

            Bird bird = new Bird(
                new int[] { Int32.Parse(information[1]), Int32.Parse(information[2]), Int32.Parse(information[3]) },
                defaultBirdPosition, defaultBirdMovement, birdSprite, fitness);

            bird.HiddenLayerWeights = InitData(bird.Layers[0], bird.Layers[1], information);
            bird.OutputLayerWeights = InitData(bird.Layers[1], bird.Layers[2], information);

            infoid = 4;

            return bird;
        }

        private static int infoid = 4;

        private static double[,] InitData(int num1, int num2, string[] info)
        {
            double[,] weight = new double[num1, num2];

            for (int i = 0; i < num1; i++)
            {
                for (int j = 0; j < num2; j++)
                {
                    if (!GameInfo.TrainMode)
                    {
                        Console.WriteLine(info[infoid]);
                    }
                    weight[i, j] = Double.Parse(info[infoid]);
                    infoid++;
                }
            }

            return weight;
        }
    }
}