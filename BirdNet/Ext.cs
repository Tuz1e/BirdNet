using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirdNet
{
    /// <summary>
    /// Extension Methods
    /// </summary>
    public static class Ext
    {
        public static double Sigmoid(this double x)
        {
            //Sigmoid function on double
            return 1.0 / (1.0 + Math.Exp(-x));
        }
        public static double[] Sigmoid(this double[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i].Sigmoid();
            }
            return arr;
        }
        public static double[,] Sigmoid(this double[,] arr)
        {
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    arr[i, j].Sigmoid();
                }
            }

            return arr;
        }
    }
}