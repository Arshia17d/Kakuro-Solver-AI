using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Kakuro
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputFile = @"E:\my projects\Kakuro-Solver-AI\Kakuro-Solver-AI\input.txt";
            Model[,] M = Modele_loader.Load_Modle(inputFile);

            Console.WriteLine("\n📋 Loaded Table:");
            Modele_loader.UI(M);

            Solver_BT_FC solver_BT_FC = new Solver_BT_FC(M);
            var swforBT_FC = Stopwatch.StartNew();
            bool solved_BT_FC = solver_BT_FC.Solve();
            swforBT_FC.Stop();

            Console.WriteLine("\n===========================");
            Console.WriteLine("BT-FC: ");
            if (solved_BT_FC)
            {
                Console.WriteLine("✅ Solved Table: ");
                Modele_loader.UI(M);
            }
            else
            {
                Console.WriteLine("❌ Not solved.");
            }

            Console.WriteLine($"⏱ Time: {swforBT_FC.ElapsedMilliseconds} ms");
            Console.WriteLine($"🔁 Nodes: {solver_BT_FC.NodeCount}");

            Console.WriteLine("\n===========================");
            Model[,] M1 = Modele_loader.Load_Modle(inputFile);

            Console.WriteLine("Min-conflicts: ");
            Solver_MinConflicts solver_Min_conflicts = new Solver_MinConflicts(M1);
            var swforMin_conflicts = Stopwatch.StartNew();
            bool solved_Min_conflicts = solver_Min_conflicts.Solve();
            swforMin_conflicts.Stop();

            if (solved_Min_conflicts)
            {
                Console.WriteLine("✅ Solved Table: ");
                Modele_loader.UI(M1);
            }
            else
            {
                Console.WriteLine("❌ Not solved.");
            }

            Console.WriteLine($"⏱ Time: {swforMin_conflicts.ElapsedMilliseconds} ms");
            //Console.WriteLine($"🔁 Nodes: {solver_Min_conflicts.NodeCount}");

            Console.WriteLine("\n===========================");
            Model[,] M2 = Modele_loader.Load_Modle(inputFile);

            Console.WriteLine("Solver_BT_FC_MRV ");
            Solver_BT_FC_MRV Solver_BT_FC_MRV = new Solver_BT_FC_MRV(M2);
            var swSolver_BT_FC_MRV = Stopwatch.StartNew();
            bool solved_BT_FC_MRV = Solver_BT_FC_MRV.Solve();
            swSolver_BT_FC_MRV.Stop();

            if (solved_BT_FC_MRV)
            {
                Console.WriteLine("✅ Solved Table: ");
                Modele_loader.UI(M2);
            }
            else
            {
                Console.WriteLine("❌ Not solved.");
            }

            Console.WriteLine($"⏱ Time: {swSolver_BT_FC_MRV.ElapsedMilliseconds} ms");
            Console.WriteLine($"🔁 Nodes: {Solver_BT_FC_MRV.NodeCount}");

            Console.WriteLine("\n===========================");
            Model[,] M3 = Modele_loader.Load_Modle(inputFile);

            Console.WriteLine("BT_FC_AC3: ");
            Solver_BT_FC_AC3 BT_FC_AC3 = new Solver_BT_FC_AC3(M3);
            var swBT_FC_AC3 = Stopwatch.StartNew();
            bool solved_BT_FC_AC3 = BT_FC_AC3.Solve();
            swBT_FC_AC3.Stop();

            if (solved_BT_FC_AC3)
            {
                Console.WriteLine("✅ Solved Table: ");
                Modele_loader.UI(M3);
            }
            else
            {
                Console.WriteLine("❌ Not solved.");
            }

            Console.WriteLine($"⏱ Time: {swBT_FC_AC3.ElapsedMilliseconds} ms");
            Console.WriteLine($"🔁 Nodes: {BT_FC_AC3.NodeCount}");

            Console.WriteLine("\n===========================");
            Model[,] M4 = Modele_loader.Load_Modle(inputFile);

            Console.WriteLine("BT_FC_MRV_AC: ");
            Solver_BT_FC_MRV BT_FC_MRV_AC = new Solver_BT_FC_MRV(M4);
            var swBT_FC_MRV_AC = Stopwatch.StartNew();
            bool solved_BT_FC_MRV_AC = BT_FC_MRV_AC.Solve();
            swBT_FC_MRV_AC.Stop();

            if (solved_BT_FC_MRV_AC)
            {
                Console.WriteLine("✅ Solved Table: ");
                Modele_loader.UI(M4);
            }
            else
            {
                Console.WriteLine("❌ Not solved.");
            }

            Console.WriteLine($"⏱ Time: {swBT_FC_MRV_AC.ElapsedMilliseconds} ms");
            Console.WriteLine($"🔁 Nodes: {BT_FC_MRV_AC.NodeCount}");
        }
    }
}
