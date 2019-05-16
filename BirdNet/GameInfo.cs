using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
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
            PipeSpeed = 4.5f,
            BackgroundSpeed = 2.0f;
        public static double
            MutationRate = 0.15,
            CrossoverRate = 0.63;
        public static int
            Population = 250,
            InputNodes = 3,
            HiddenNodes = 2,
            OutputNodes = 1,
            RandomIndividuals = Population / 10,
            PipeInterval = 80,
            PipeTop = 285,
            PipeBottom = 285;
        public static bool
            FastMode = true, //TrainMode = false overrides this.
            TrainMode = true,
            SaveMode = true; //Determines if train mode will save the data.

        public static string NetFilePath = Environment.CurrentDirectory; //Debug folder
        public static string NetFileType = ".txt";
        public static string NetFileName = "training_data";
        public static string NetFullPath = Path.GetFullPath(NetFilePath + "\\" + NetFileName + NetFileType);
    }
}
