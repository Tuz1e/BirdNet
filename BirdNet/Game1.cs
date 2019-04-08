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
        int aliveTimer, score, highScore;
        private SpriteFont font;
        private Sprite birdSprite;

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

            evo = new Evolution(this.birdSprite, this.defaultBirdPosition, this.defaultBirdMovement, this.population, GameInfo.MutationRate, GameInfo.CrossoverRate);

            Reset();
        }
        protected override void UnloadContent()
        {
        }
        protected override void Update(GameTime gameTime)
        {
            foreach (Bird b in birds)
            {
                b.Update(GraphicsDevice, pipes);

                if (
                    b.Position.Y >
                    (this.GraphicsDevice.PresentationParameters.BackBufferHeight
                    - b.Sprite.Texture.Height - 10))
                {
                    b.Position = new Vector2(
                        b.Position.X,
                        this.GraphicsDevice.PresentationParameters.BackBufferHeight
                        - b.Sprite.Texture.Height - 10);
                }
                else if (b.Position.Y <= 0)
                {
                    b.Position = new Vector2(b.Position.X, 0);
                }
            }

            foreach(Pipe pipe in pipes)
            {
                pipe.Update(GraphicsDevice);

                foreach(Bird b in birds)
                {

                    if (b.Position.X > pipe.Position.X && !pipe.Passed)
                    {
                        b.Score += 1;
                        pipe.Passed = true;
                    }
                    if (b.Hitbox.Intersects(pipe.Hitbox))
                        b.AliveFlag = false;
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

            birds = evo.BreedBirds(birds, 0);

            base.Update(gameTime);
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
