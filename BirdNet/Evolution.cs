using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
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

        private List<Bird> newBirdList;




        public Evolution(Sprite birdSprite, Vector2 defaultPosition, Vector2 defaultMovement, int population, double mutationRate, double crossoverRate)
        {
            this.defaultBirdSprite = birdSprite;
            this.defaultBirdPosition = defaultPosition;
            this.defaultBirdMovement = defaultMovement;

            this.Population = population;
            this.MutationRate = mutationRate;
            this.CrossoverRate = crossoverRate;

            this.Generation = 0;

            this.elitist = new List<Bird>();
            this.oldHighScore = 0;

            rand = new Random();
        }

        public List<Bird> BreedBirds(List<Bird> birds, int highScore)
        {
            foreach (Bird b in birds)
            {
                if (b.Score > oldHighScore)
                {
                    b.Fitness = ((b.Score + b.AliveTime) * 2);
                }
                else
                {
                    b.Fitness = (b.Score + b.AliveTime);
                }

                if (oldHighScore < highScore)
                {
                    oldHighScore = highScore;
                }
            }

            if (GetDeadBirds(birds) == birds.Count)
            {
                newBirdList = new List<Bird>();
                birds.OrderByDescending(bird => bird.Fitness).ToList();
                newBirdList.AddRange(birds.Take(3));

                for (int i = 0; i < newBirdList.Count; i++)
                {
                    if (elitist.Count > 2)
                    {
                        for (int j = 0; j < elitist.Count; j++)
                        {
                            if (newBirdList[i].Fitness > elitist[j].Fitness)
                            {
                                elitist.Add(newBirdList[i]);
                                elitist.OrderBy(bird => bird.Fitness);
                                elitist.RemoveAt(0);
                            }
                        }
                    }
                    else
                        elitist.Add(newBirdList[i]);
                }
                elitist.OrderByDescending(bird => bird.Fitness);

                for (int i = 0; i < GameInfo.RandomIndividuals; i++)
                {
                    newBirdList.Add(new Bird(this.Layers, this.defaultBirdPosition, this.defaultBirdMovement, this.defaultBirdSprite));
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

                newBirdList.OrderByDescending(bird => bird.Fitness).ToList();
                birds.Clear();
                birds.AddRange(newBirdList);
                newBirdList.Clear();
            }
            return birds;
        }

        private int GetDeadBirds(List<Bird> birds)
        {
            int deadBirds = 0;
            foreach(Bird b in birds)
            {
                if (!b.AliveFlag)
                {
                    deadBirds += 1;
                }
            }

            return deadBirds;
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
