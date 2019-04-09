using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

        bool playing;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        protected override void Initialize()
        {
            this.playing = true;

            this.rand = new Random(Guid.NewGuid().GetHashCode());


            graphics.PreferredBackBufferHeight = GameInfo.WindowHeight;
            graphics.PreferredBackBufferWidth = GameInfo.WindowWidth;
            graphics.ApplyChanges();

            this.population = GameInfo.Population;
            this.defaultBirdPosition = new Vector2(560, 300);
            this.defaultBirdMovement = Vector2.Zero;

            this.layers = new int[] {GameInfo.InputNodes, GameInfo.HiddenNodes, GameInfo.OutputNodes };

            birds = new List<Bird>();

            base.Initialize();
        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            spriteBatch = new SpriteBatch(GraphicsDevice);

            this.background = Content.Load<Texture2D>("background");
            this.birdTexture = Content.Load<Texture2D>("bird");

            birdSprite = new Sprite(birdTexture, 4, 8);

            //this.font = Content.Load<SpriteFont>("font");

            InitGen(GameInfo.InputNodes, GameInfo.HiddenNodes, GameInfo.OutputNodes);

            evo = new Evolution(this.layers, this.birdSprite, this.defaultBirdPosition, this.defaultBirdMovement, this.population, GameInfo.MutationRate, GameInfo.CrossoverRate);

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

            for (int i = 0; i < pipes.Count; i++)
            {
                pipes[i].Update(GraphicsDevice);
                for (int j = 0; j < birds.Count; j++)
                {
                    if (birds[j].Position.X > pipes[i].Position.X && !pipes[i].Passed)
                    {
                        birds[i].ModifyScore(1);
                    }
                    if (birds[j].Hitbox.Intersects(pipes[i].Hitbox))
                        birds[j].AliveFlag = false;
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
                pipeTimer -= 1;

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

            evo.BreedBirds(this.birds, this.highScore);
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
        }

        /// <summary>
        /// Gets a randomized weight.
        /// </summary>
        /// <param name="anInputAmnt">Amount of nodes in the first layer.</param>
        /// <param name="aHiddenAmnt">Amount of nodes in the second layer.</param>
        /// <returns>A new randomized weight</returns>
        private double[,] Create(int num1, int num2)
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

        public void InitGen(int input, int hidden, int output)
        {
            CreateNet(input, hidden, output);
        }

        private void CreateNet(int input, int hidden, int output)
        {
            for (int i = 0; i < population; i++)
            {
                birds.Add(
                    new Bird(
                        new int[] { input, hidden, output }, 
                        Create(input, hidden), 
                        Create(hidden, output), 
                        0, 
                        defaultBirdPosition, 
                        defaultBirdMovement, 
                        birdSprite));
            }
        }
    }
}
