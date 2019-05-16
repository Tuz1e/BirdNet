using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace BirdNet
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        enum GameState
        {
            Menu,
            Network
        }

        enum NetworkState
        {
            Normal,
            Fast
        }

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D background, birdTexture;
        Random rand;
        float backgroundScroll;

        Evolution evo;

        int population;

        List<Bird> birds;
        List<Pipe> pipes;

        static int pipeTimer;
        int highScore;
        private SpriteFont font;
        private Sprite birdSprite;

        private int[] layers;

        private Vector2 defaultBirdPosition, defaultBirdMovement;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        protected override void Initialize()
        {
            this.rand = new Random(Guid.NewGuid().GetHashCode());


            graphics.PreferredBackBufferHeight = GameInfo.WindowHeight;
            graphics.PreferredBackBufferWidth = GameInfo.WindowWidth;
            graphics.ApplyChanges();



            this.population = (GameInfo.TrainMode) ? GameInfo.Population : 1;
            this.defaultBirdPosition = new Vector2(560, 300);
            this.defaultBirdMovement = Vector2.Zero;

            this.layers = new int[] {GameInfo.InputNodes, GameInfo.HiddenNodes, GameInfo.OutputNodes };

            birds = new List<Bird>();

            base.Initialize();

            if (GameInfo.FastMode && GameInfo.TrainMode == true)
            {
                while (true)
                {
                    Update(null);
                }
            }
        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            spriteBatch = new SpriteBatch(GraphicsDevice);

            this.background = Content.Load<Texture2D>("background");
            this.birdTexture = Content.Load<Texture2D>("bird");

            birdSprite = new Sprite(birdTexture, 4, 8);

            //this.font = Content.Load<SpriteFont>("font");

            if (GameInfo.TrainMode)
            {
                birds = Network.CreateNet(GameInfo.InputNodes, GameInfo.HiddenNodes, GameInfo.OutputNodes,
                    GameInfo.Population, this.defaultBirdPosition, this.defaultBirdMovement, this.birdSprite, this.rand);

                evo = new Evolution(this.layers, this.birdSprite, this.defaultBirdPosition, this.defaultBirdMovement, this.population, GameInfo.MutationRate, GameInfo.CrossoverRate);
            }
            else
            {
                birds.Add(FileManager.Read(this.defaultBirdPosition, this.defaultBirdMovement, this.birdSprite));
            }

            Reset();
        }





        protected override void UnloadContent()
        {
        }
        protected override void Update(GameTime gameTime)
        {
            if (GetDeadBirds() < birds.Count)
            {
                for (int i = 0; i < birds.Count; i++)
                {
                    birds[i].Update(GraphicsDevice, pipes);

                    if (
                        birds[i].Position.Y >
                        (this.GraphicsDevice.PresentationParameters.BackBufferHeight
                        - birds[i].Sprite.Texture.Height - 10))
                    {
                        birds[i].Position = new Vector2(
                            birds[i].Position.X,
                            this.GraphicsDevice.PresentationParameters.BackBufferHeight
                            - birds[i].Sprite.Texture.Height - 10);
                    }
                    else if (birds[i].Position.Y <= 0)
                    {
                        birds[i].Position = new Vector2(birds[i].Position.X, 0);
                    }
                }
            }
            else
            {
                Reset();
            }

            bool pipePassed = false;
            for (int i = 0; i < pipes.Count; i++)
            {
                pipes[i].Update(GraphicsDevice);
                for (int j = 0; j < birds.Count; j++)
                {
                    if (birds[j].Position.X > pipes[i].Position.X && !pipes[i].Passed)
                    {
                        birds[j].ModifyScore(1);
                        pipePassed = true;
                    }

                    if (birds[j].Hitbox.Intersects(pipes[i].Hitbox))
                    {
                        birds[j].SetDead();
                    }
                }

                if (pipePassed)
                {
                    pipes[i].SetPassed(true);
                }
            }

            this.backgroundScroll += GameInfo.BackgroundSpeed;
            if (backgroundScroll > background.Width)
                backgroundScroll = 0;

            if (pipeTimer == 0)
            {
                PipeGenerator(this.pipes, 1, GameInfo.PipeTop, GameInfo.PipeBottom);
            }
            else
            {
                pipeTimer -= 1;
            }

            for (int i = 0; i < this.pipes.Count; i++)
            {
                if (this.pipes[i].OutOfBounds)
                    this.pipes.RemoveAt(i);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Get the amount of dead birds.
        /// </summary>
        /// <param name="birds"></param>
        /// <returns></returns>
        private int GetDeadBirds()
        {
            int deadBirds = 0;
            for (int i = 0; i < birds.Count; i++)
            {
                if (!birds[i].AliveFlag)
                {
                    deadBirds += 1;
                }
            }

            return deadBirds;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            for (int i = 0; i < 4; i++)
            {
                spriteBatch.Draw(
                    this.background,
                    new Vector2(background.Width * i - this.backgroundScroll, 0), 
                    Color.White);
            }

            foreach(Pipe pipe in pipes)
            {
                pipe.Draw(spriteBatch);
            }

            foreach(Bird b in birds)
            {
                b.Draw(spriteBatch);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void Reset()
        {
            this.pipes = new List<Pipe>();
            PipeGenerator(this.pipes, 1, GameInfo.PipeTop, GameInfo.PipeBottom);
            this.rand = new Random(Guid.NewGuid().GetHashCode());
            this.backgroundScroll = 0;

            if (GameInfo.TrainMode)
            {
                evo.BreedBirds(this.birds, this.highScore, this.birdSprite);
            }
            else if (!GameInfo.TrainMode)
            {
                Console.WriteLine(birds[0].Score);
                birds[0].SetAlive(defaultBirdPosition);
            }
        }

        private void PipeGenerator(List<Pipe> pipeList, int chance, int minY, int maxY)
        {
            int pipeHeight = Content.Load<Texture2D>("pipe").Height;
            // Top pipe
            pipeList.Add(new Pipe(new Vector2(graphics.PreferredBackBufferWidth + 100,
                rand.Next(-pipeHeight + 100, minY - pipeHeight)),
                new Vector2(-GameInfo.PipeSpeed, 0),
                Content.Load<Texture2D>("pipe_flipped")));

            // Bottom pipe
            pipeList.Add(new Pipe(new Vector2(graphics.PreferredBackBufferWidth + 100,
                rand.Next(graphics.PreferredBackBufferHeight - maxY, graphics.PreferredBackBufferHeight - 100)),
                new Vector2(-GameInfo.PipeSpeed, 0),
                Content.Load<Texture2D>("pipe")));

            pipeTimer = GameInfo.PipeInterval;

            pipeTimer = GameInfo.PipeInterval;
        }
    }
}
