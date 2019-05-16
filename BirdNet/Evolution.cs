using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirdNet
{
    class Evolution
    {
        public int Population { get; set; }
        public int Generation { get; set; }
        public double MutationRate { get; set; }
        public double CrossoverRate { get; set; }

        public int[] Layers;

        List<Bird> elitist;
        Sprite defaultBirdSprite;
        Vector2 defaultBirdPosition, defaultBirdMovement;

        Random rand;

        int oldHighScore;

        float maxFitness, maxScore;

        private List<Bird> newBirdList;

        Bird bestBird;




        public Evolution(int[] layers, Sprite birdSprite, Vector2 defaultPosition, Vector2 defaultMovement, int population, double mutationRate, double crossoverRate)
        {
            this.Layers = layers;
            this.defaultBirdSprite = birdSprite;
            this.defaultBirdPosition = defaultPosition;
            this.defaultBirdMovement = defaultMovement;

            this.Population = population;
            this.MutationRate = mutationRate;
            this.CrossoverRate = crossoverRate;

            this.elitist = new List<Bird>();
            this.oldHighScore = 0;

            rand = new Random();
            
            if (File.Exists(GameInfo.NetFullPath))
            {
                bestBird = FileManager.Read(defaultPosition, defaultMovement, birdSprite);
            }
            else
            {
                bestBird = new Bird();
            }
        }

        public void BreedBirds(List<Bird> birds, int highScore, Sprite birdSprite)
        {
            float tempFitness = 0;

            for (int i = 0; i < birds.Count; i++)
            {
                if (birds[i].Score > oldHighScore)
                {
                    birds[i].SetFitness(birds[i].Fitness *1.5f);
                    highScore = oldHighScore;
                }
                if (oldHighScore < highScore)
                {
                    oldHighScore = highScore;
                }
                tempFitness += birds[i].Fitness;
            }
            Generation += 1;

            Bird bird = birds.OrderByDescending(b => b.Fitness).First();

            //If theres no training data saved, Save
            if (GameInfo.SaveMode && bird.Fitness > bestBird.Fitness)
            {
                FileManager.Write(bird, Layers, Generation);
                bestBird = new Bird(bird);
            }

            maxFitness = bird.Fitness;
            maxScore = bird.Score;

            Console.WriteLine(
                "GEN " + Generation + 
                " | AVGFIT " + (tempFitness / birds.Count) + 
                " | BESTFIT " + maxFitness +
                " | MAXSCR " + maxScore);

            newBirdList = new List<Bird>();
            birds.OrderByDescending(b => b.Fitness).ToList();

            for (int i = 0; i < newBirdList.Count; i++)
            {
                if (elitist.Count > 2)
                {
                    for (int j = 0; j < elitist.Count; j++)
                    {
                        if (newBirdList[i].Fitness > elitist[j].Fitness)
                        {
                                elitist.Add(newBirdList[i]);
                                elitist.OrderBy(b => b.Fitness);
                                elitist.RemoveAt(0);
                        }
                        elitist[j].SetAlive(defaultBirdPosition);
                    }
                }
                else { elitist.Add(newBirdList[i]); }
            }

                elitist.OrderByDescending(b => b.Fitness);

                for (int i = 0; i < GameInfo.RandomIndividuals; i++)
                {
                birds.Add(
                    new Bird(
                        new int[] { Layers[0], Layers[1], Layers[2] },
                        Network.Create(Layers[0], Layers[1], rand),
                        Network.Create(Layers[1], Layers[2], rand),
                        0,
                        this.defaultBirdPosition,
                        this.defaultBirdMovement,
                        birdSprite));
            }

                if (elitist.Count > 3)
                {
                    List<Bird> b = new List<Bird>();

                    for (int i = 0; i < 3; i++)
                    {
                        b.Add(elitist[i]);
                    }
                    elitist.Clear();
                    elitist.AddRange(b);
                }
                newBirdList.AddRange(elitist);

                while(newBirdList.Count < Population)
                {
                    Bird b1 = Pick(birds);
                    Bird b2 = Pick(birds);

                    List<double> gene1, gene2;
                    gene1 = gene2 = new List<double>();

                    Compute(b1.HiddenLayerWeights, gene1, true);
                    Compute(b1.OutputLayerWeights, gene1, true);

                    Compute(b2.HiddenLayerWeights, gene2, true);
                    Compute(b2.OutputLayerWeights, gene2, true);

                    if (rand.NextDouble() <= CrossoverRate)
                        Crossover(gene1, gene2);
                    else
                    {
                        if (Mutate(gene1))
                            b1 = new Bird(this.Layers, this.defaultBirdPosition, this.defaultBirdMovement, this.defaultBirdSprite);
                        if (Mutate(gene2))
                            b2 = new Bird(this.Layers, this.defaultBirdPosition, this.defaultBirdMovement, this.defaultBirdSprite);

                        Compute(b1.HiddenLayerWeights, gene1, false);
                        Compute(b1.OutputLayerWeights, gene1, false);

                        Compute(b2.HiddenLayerWeights, gene2, false);
                        Compute(b2.OutputLayerWeights, gene2, false);

                        newBirdList.Add(b1);
                        newBirdList.Add(b2);
                    }
                }

                newBirdList.OrderBy(b => b.Fitness).ToList();
                birds.Clear();
                birds.AddRange(newBirdList);
                birds.RemoveAt(birds.Count()-1);
                newBirdList.Clear();
        }

        private bool Mutate(List<double> gene)
        {
            bool _ = false;

            for (int i = 0; i < gene.Count; i++)
            {
                if (rand.NextDouble() < MutationRate)
                {
                    gene[i] += (rand.NextDouble() * 2 - 1);
                    _ = true;
                }
            }

            return _;
        }

        private void Crossover(List<double> gene1, List<double> gene2)
        {
            List<double> parent1, parent2;
            parent1 = parent2 = new List<double>();

            for (int i = 0; i < gene1.Count; i++)
            {
                parent1.Add((gene1[i] + gene2[i]) / 2.0);
                parent2.Add((gene1[i] + gene2[i]) / 2.0);
            }

            Bird b1, b2;
            b1 = b2 = new Bird(Layers, defaultBirdPosition, defaultBirdPosition, defaultBirdSprite);

            Compute(b1.HiddenLayerWeights, parent1, false);
            Compute(b1.OutputLayerWeights, parent1, false);

            Compute(b2.HiddenLayerWeights, parent2, false);
            Compute(b2.OutputLayerWeights, parent2, false);


        }

        private Bird Pick(List<Bird> birds)
        {
            int v1, v2;
            v1 = v2 = 0;

            while(v1 == v2)
            {
                v1 = rand.Next(0, birds.Count / 3);
                v2 = rand.Next(0, birds.Count / 3);
            }

            return (birds[v1].Fitness > birds[v2].Fitness) ? birds[v1] : birds[v2];
        }


        /// <summary>
        /// Compute values
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="gene">The gene to change</param>
        /// <param name="ed">Encode (true) / Decode (false) </param>
        private void Compute(double[,] data, List<double> gene, bool ed)
        {
            for (int i = 0; i < data.GetLength(0); i++)
            {
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    if (ed)
                    {
                        gene.Add(data[i, j]);
                    }
                    else if (!ed)
                    {
                        data[i, j] = gene[0];
                        gene.RemoveAt(0);
                    }
                }
            }
        }
    }
}
