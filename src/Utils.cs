// کلاس‌های کمکی (مثلاً خواندن و نوشتن فایل)

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
    class Modele_loader
    {
        public static Model[,] Load_Modle(string file_path)
        {
            string[] all_lines = File.ReadAllLines(file_path);
            var Line1 = all_lines[0].Split(' ');
            int rows = int.Parse(Line1[0]); // تعداد سطرها
            int cols = int.Parse(Line1[1]); // تعداد ستون‌ها

            Model[,] M1 = new Model[rows, cols];

            // پر کردن جدول با خانه‌های سفید
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    M1[i, j] = new Model(i + 1, j + 1, Model_Type.white); // مقداردهی با اندیس‌های ۱‌بیس
                }
            }

            // اضافه کردن خانه‌های سیاه
            int blackCellCount = int.Parse(all_lines[1]);
            int lineIndex = 2;
            for (int i = 0; i < blackCellCount; i++, lineIndex++)
            {
                string[] parts = all_lines[lineIndex].Split(' ');
                int r = int.Parse(parts[0]) - 1; 
                int c = int.Parse(parts[1]) - 1; 
                M1[r, c] = new Model(r + 1, c + 1, Model_Type.Black); 
            }

            // اضافه کردن خانه‌های دارای دیتا
            int DataCellCount = int.Parse(all_lines[lineIndex]);
            lineIndex++;
            for (int i = 0; i < DataCellCount; i++, lineIndex++)
            {
                string[] parts = all_lines[lineIndex].Split(' ');
                int r = int.Parse(parts[0]) - 1; 
                int c = int.Parse(parts[1]) - 1; 
                int k_L = int.Parse(parts[2]);
                int K_T = int.Parse(parts[3]);

                M1[r, c] = new Model(r + 1, c + 1, Model_Type.Data)
                {
                    KeyLeft = k_L,
                    KeyTop = K_T
                };
            }

            return M1;
        }

        public static void UI(Model[,] M)
        {
            int rows = M.GetLength(0);
            int cols = M.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Console.Write(M[i, j].ToString() + " ");
                }
                Console.WriteLine();
            }
        }
    }
}
