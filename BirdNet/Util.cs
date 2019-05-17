using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirdNet
{
    internal class Util
    {
        public static void DebugMsg(String strParam, bool writeLineParam = true, ConsoleColor cParam = ConsoleColor.White)
        {
            Console.ForegroundColor = cParam;

            if (writeLineParam)
            {
                Console.WriteLine(strParam);
            }
            else
            {
                Console.Write(strParam);
            }

            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Cursor(bool stateParam)
        {
            Console.CursorVisible = stateParam;
        }

        public static void C()
        {
            Console.Clear();
        }

        public static void Break(ConsoleColor cParam = ConsoleColor.Gray)
        {
            Console.ForegroundColor = cParam;
            DebugMsg("---------------------", true, Console.BackgroundColor);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void NewLine()
        {
            Console.WriteLine();
        }
    }
}
