using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Kakuro
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputFile =  @"E:\my projects\Kakuro-Solver-AI\Kakuro-Solver-AI\input.txt";
            Model[,] M = Modele_loader.Load_Modle(inputFile);
            Modele_loader.UI(M);
        }
    }
}
