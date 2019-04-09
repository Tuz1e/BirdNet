using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirdNet
{
    class GameInfo
    {
        #region GameWindow
        public static int 
            WindowWidth = 1280,
            WindowHeight = 720;
        #endregion

        public static float
            Gravity = 0.55f,
            Force = 17.5f,
            MaxPower = 10f,
            PipeSpeed = 3.5f,
            BackgroundSpeed = 2.0f;
        public static double
            MutationRate = 0.15,
            CrossoverRate = 0.53;
        public static int
            Population = 25,
            InputNodes = 3,
            HiddenNodes = 2,
            OutputNodes = 1,
            RandomIndividuals = Population / 10,
            PipeInterval = 100,
            PipeTop = 285,
            PipeBottom = 285;
    }
}
