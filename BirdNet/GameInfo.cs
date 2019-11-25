using System.IO;

namespace BirdNet
{
    internal class GameInfo
    {
        #region GameWindow

        public static int
            WindowWidth = 1280,
            WindowHeight = 720;

        #endregion GameWindow

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
            Population = 250,
            InputNodes = 3,
            HiddenNodes = 2,
            OutputNodes = 1,
            RandomIndividuals = Population / 10,
            PipeInterval = 100,
            PipeTop = 285,
            PipeBottom = 285;

        public static bool
            FastMode = true, //TrainMode = false overrides this.
            TrainMode = false,
            SaveMode = false, //Determines if train mode will save the data.
            IncludeSaved = false,
            CCursorState = false;

        public static string NetFilePath =
            Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;

        public static string NetFileType = ".txt";
        public static string NetFileName = "training_data"; //Change to training_data when training as to not override an already trained file training_data_working
        public static string NetFullPath = Path.GetFullPath($"{NetFilePath}\\{NetFileName}{NetFileType}");
    }
}