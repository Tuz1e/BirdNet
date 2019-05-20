using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BirdNet
{
    internal class Evolution
    {
        public int Population { get; set; }
        public int Generation { get; set; }
        public double MutationRate { get; set; }
        public double CrossoverRate { get; set; }

        public int[] Layers;

        private List<Bird> elitist;
        private Sprite defaultBirdSprite;
        private Vector2 defaultBirdPosition, defaultBirdMovement;

        private Random rand;

        private int oldHighScore;

        private float maxFitness, maxScore, totalFitness, oldMedian, firstAverageFit, oldAvgFitness;

        private List<Bird> newBirdList;

        public Bird SavedBird { get; set; }

        public Evolution(int[] layersParam, Sprite birdSpriteParam,
            Vector2 defaultPosParam, Vector2 defaultMovementParam,
            int popParam, double mRateParam, double cRateParam)
        {
            this.Layers = layersParam;
            this.defaultBirdSprite = birdSpriteParam;
            this.defaultBirdPosition = defaultPosParam;
            this.defaultBirdMovement = defaultMovementParam;

            this.Population = popParam;
            this.MutationRate = mRateParam;
            this.CrossoverRate = cRateParam;

            this.elitist = new List<Bird>();
            this.oldHighScore = 0;
            this.totalFitness = 0;

            rand = new Random();

            if (File.Exists(GameInfo.NetFullPath))
            {
                Bird _ = FileManager.Read(defaultPosParam, defaultMovementParam, birdSpriteParam);
                SavedBird = _;
            }
            else
            {
                SavedBird = new Bird();
            }
        }

        public List<Bird> BreedBirds(List<Bird> birdsParam, int hsParam)
        {
            float tempFitness = 0;
            List<Bird> birdsToReturn = birdsParam;

            for (int i = 0; i < birdsToReturn.Count; i++)
            {
                tempFitness += birdsToReturn[i].Fitness;
                totalFitness += tempFitness;
            }

            //Bird with Highest Fitness
            Bird bird = birdsToReturn.OrderByDescending(b => b.Fitness).First();

            //If theres no training data saved or bird has higher fitness than saved bird, Save
            if (GameInfo.SaveMode && bird.Fitness > SavedBird.Fitness)
            {
                FileManager.Write(bird, Layers, Generation);
                this.SavedBird = new Bird(bird);
            }

            //Bird with highest fitness, should theoretically have the highest score.
            //Although isn't always the case
            this.maxFitness = bird.Fitness;
            this.maxScore = bird.Score;

            PrintData(tempFitness, birdsToReturn.Count);

            newBirdList = new List<Bird>();

            #region Elitism

            birdsToReturn = birdsToReturn.OrderByDescending(b => b.Fitness).ToList();

            newBirdList.AddRange(birdsToReturn.Take(3)); //Take the 3 birds with the highest fitness
            for (int i = 0; i < newBirdList.Count; i++)
            {
                if (elitist.Count > 2)
                {
                    for (int j = 0; j < elitist.Count; j++)
                    {
                        if (newBirdList[i].Fitness > elitist[j].Fitness)
                        {
                            elitist.Add(newBirdList[i]);
                            elitist = elitist.OrderBy(b => b.Fitness).ToList();
                            elitist.RemoveAt(0); //Remove he who has the lowest fitness
                        }
                        elitist[j].SetAlive(defaultBirdPosition);
                    }
                }
                else { elitist.Add(newBirdList[i]); }
            }

            if (elitist.Count > 3)
            {
                elitist = elitist.OrderByDescending(e => e.Fitness).ToList();
                List<Bird> b = new List<Bird>();
                b.AddRange(elitist.Take(3));
                elitist.Clear();
                elitist.AddRange(b);
            }
            newBirdList.AddRange(elitist);

            #endregion Elitism

            for (int i = 0; i < GameInfo.RandomIndividuals; i++)
            {
                birdsToReturn.Add(GetRandomBird());
            }

            while (newBirdList.Count < Population)
            {
                Bird b1 = Pick(birdsToReturn);
                Bird b2 = Pick(birdsToReturn);

                List<double> gene1, gene2;
                gene1 = gene2 = new List<double>();

                Compute(b1.HiddenLayerWeights, gene1, true);
                Compute(b1.OutputLayerWeights, gene1, true);

                Compute(b2.HiddenLayerWeights, gene2, true);
                Compute(b2.OutputLayerWeights, gene2, true);

                if (rand.NextDouble() <= CrossoverRate)
                {
                    Crossover(gene1, gene2);
                    b1 = new Bird(this.Layers, this.defaultBirdPosition, this.defaultBirdMovement, this.defaultBirdSprite);
                    b2 = new Bird(this.Layers, this.defaultBirdPosition, this.defaultBirdMovement, this.defaultBirdSprite);
                }
                else
                {
                    if (Mutate(gene1))
                    {
                        b1 = new Bird(this.Layers, this.defaultBirdPosition, this.defaultBirdMovement, this.defaultBirdSprite);
                    }
                    if (Mutate(gene2))
                    {
                        b2 = new Bird(this.Layers, this.defaultBirdPosition, this.defaultBirdMovement, this.defaultBirdSprite);
                    }
                }

                Compute(b1.HiddenLayerWeights, gene1, false);
                Compute(b1.OutputLayerWeights, gene1, false);

                Compute(b2.HiddenLayerWeights, gene2, false);
                Compute(b2.OutputLayerWeights, gene2, false);

                newBirdList.Add(b1);
                newBirdList.Add(b2);
            }

            newBirdList = newBirdList.OrderBy(b => b.Fitness).ToList();
            birdsToReturn.Clear();
            birdsToReturn.AddRange(newBirdList);
            birdsToReturn = birdsToReturn.OrderByDescending(b => b.Fitness).ToList();
            newBirdList.Clear();

            Generation += 1;

            if (birdsToReturn.Count != GameInfo.Population)
            {
                while (true)
                {
                    Util.DebugMsg("BIRD COUNT IS BIGGER THAN POPULATION!!!", true, ConsoleColor.Red);
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
                }
            }

            return birdsToReturn;
        }

        private bool Mutate(List<double> geneParam)
        {
            bool _ = false;
            for (int i = 0; i < geneParam.Count; i++)
            {
                if (rand.NextDouble() <= MutationRate)
                {
                    geneParam[i] += (rand.NextDouble() * 2 - 1);
                    _ = true;
                }
            }

            return _;
        }

        /// <summary>
        /// Cross two birds into a new one
        /// </summary>
        /// <param name="g1Param"></param>
        /// <param name="g2Param"></param>
        private void Crossover(List<double> g1Param, List<double> g2Param)
        {
            List<double> parent1, parent2;
            parent1 = parent2 = new List<double>();

            for (int i = 0; i < g1Param.Count; i++)
            {
                parent1.Add((g1Param[i] + g2Param[i]) / 2.0);
                parent2.Add((g1Param[i] + g2Param[i]) / 2.0);
                //parent1.Add(
                //    (rand.NextDouble() > 0.5) ?
                //    (g1Param[rand.Next(0, g1Param.Count - 1)]) :
                //    (g2Param[rand.Next(0, g2Param.Count - 1)])
                //    );
                //parent2.Add(
                //    (rand.NextDouble() > 0.5) ?
                //    (g1Param[rand.Next(0, g1Param.Count - 1)]) :
                //    (g2Param[rand.Next(0, g2Param.Count - 1)])
                //    );
            }

            Bird b1, b2;
            b1 = b2 = new Bird(Layers, defaultBirdPosition, defaultBirdPosition, defaultBirdSprite);

            //Switch genes around
            Compute(b1.HiddenLayerWeights, parent1, false);
            Compute(b1.OutputLayerWeights, parent1, false);

            Compute(b2.HiddenLayerWeights, parent2, false);
            Compute(b2.OutputLayerWeights, parent2, false);
        }

        private Bird Pick(List<Bird> birdsParam)
        {
            //if (birds.Count > 0)
            //{
            //    birds = birds.OrderByDescending(b => b.Fitness).ToList();

            //    List<Bird> tempBirds = birds.Take(5).ToList(); //Take the best two birds

            //    return tempBirds[rand.Next(0, tempBirds.Count - 1)];
            //}
            //else
            //{
            //    return GetRandomBird();
            //}

            int v1, v2;
            v1 = v2 = 0;

            while (v1 == v2)
            {
                v1 = rand.Next(0, birdsParam.Count / 3);
                v2 = rand.Next(0, birdsParam.Count / 3);
            }

            return (birdsParam[v1].Fitness > birdsParam[v2].Fitness) ? birdsParam[v1] : birdsParam[v2];
        }

        private Bird GetRandomBird()
        {
            return new Bird(
                    new int[] { Layers[0], Layers[1], Layers[2] },
                    Data.Create(Layers[0], Layers[1], rand),
                    Data.Create(Layers[1], Layers[2], rand),
                    0,
                    this.defaultBirdPosition,
                    this.defaultBirdMovement,
                    defaultBirdSprite);
        }

        /// <summary>
        /// Compute values
        /// </summary>
        /// <param name="dataParam">Data</param>
        /// <param name="gParam">The gene to change</param>
        /// <param name="edParam">Encode (true) / Decode (false) </param>
        private void Compute(double[,] dataParam, List<double> gParam, bool edParam)
        {
            for (int i = 0; i < dataParam.GetLength(0); i++)
            {
                for (int j = 0; j < dataParam.GetLength(1); j++)
                {
                    if (edParam)
                    {
                        gParam.Add(dataParam[i, j]);
                    }
                    else if (!edParam)
                    {
                        dataParam[i, j] = gParam[0];
                        gParam.RemoveAt(0);
                    }
                }
            }
        }

        private void PrintData(float tempFitnessParam, int birdCount)
        {
            #region Basic Calculations

            float median = totalFitness / (Generation * GameInfo.Population);

            float diff = median - this.oldMedian;
            this.oldMedian = median;

            float averageFitness = tempFitnessParam / birdCount;

            float
                percentDifference = 0,
                avgDifference = 0,
                avgFitIncrease = 0,
                avgOldFitnessIncrease = 0,
                previousFitnessDifference = 0;
            if (Generation == 1)
            {
                this.firstAverageFit = averageFitness;
            }

            if (Generation == 0)
            {
                Util.DebugMsg($"Generation: 0 - Starting up...");
            }
            else if (Generation > 0)
            {
                //Median difference from last generation
                percentDifference = diff / median * 100;

                previousFitnessDifference = averageFitness - this.oldAvgFitness;
                avgOldFitnessIncrease = previousFitnessDifference / averageFitness * 100;

                avgDifference = averageFitness - this.firstAverageFit;
                avgFitIncrease = averageFitness / this.firstAverageFit * 100;

                Util.C();

                //Average fitness change from previous generation
                Util.DebugMsg($"Generation: ", false);
                Util.DebugMsg($"{Generation} ", true, ConsoleColor.Cyan);

                Util.DebugMsg($"Max Score: ", false);
                Util.DebugMsg($"{this.maxScore} ", true, ConsoleColor.Cyan);

                Util.NewLine();

                //Average fitness change from previous generation

                Util.DebugMsg($"Best Fitness: ", false);
                Util.DebugMsg($"{this.maxFitness} ", true, ConsoleColor.Cyan);
                Util.DebugMsg($"Average Fitness: ", false);
                Util.DebugMsg($"{averageFitness} ", true, ConsoleColor.Cyan);
                Util.DebugMsg($"Previous Avg Fitness: ", false);
                Util.DebugMsg($"{this.oldAvgFitness} ", true, ConsoleColor.Cyan);
                Util.DebugMsg($"Avg Fitness Change: ", false);
                Util.DebugMsg($"{avgOldFitnessIncrease}%", true,
                    (avgOldFitnessIncrease >= 0) ? ConsoleColor.Green : ConsoleColor.Red);

                Util.NewLine();

                //Average fitness change from generation 1.
                Util.DebugMsg($"GEN[1] Average Fitness: ", false);
                Util.DebugMsg($"{this.firstAverageFit} ", true, ConsoleColor.Cyan);
                Util.DebugMsg($"Fitness Increase/Decrease: ", false);
                Util.DebugMsg($"{avgFitIncrease}% ", true,
                    (avgFitIncrease >= 0) ? ConsoleColor.Green : ConsoleColor.Red);

                Util.NewLine();

                //Median
                Util.DebugMsg($"Median: ", false);
                Util.DebugMsg($"{median} ", true, ConsoleColor.Cyan);
                Util.DebugMsg("Median Difference: ", false);
                Util.DebugMsg($"{percentDifference}%", true,
                    (percentDifference >= 0) ? ConsoleColor.Green : ConsoleColor.Red);

                Util.Break();
            }
            oldAvgFitness = averageFitness;

            //Console.WriteLine(
            //    $"GEN {Generation} | AVGFIT {averageFitness} " +
            //    $"| BESTFIT {maxFitness} | MAXSCR {maxScore} | AVGFITINCR {avgFitIncrease}% " +
            //    $"| PERCDIFF {percentDiff}%");

            #endregion Basic Calculations
        }
    }
}