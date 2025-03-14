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
        public static Model Load_Modle(string file_path)
        {
            string[]? all_lines = File.ReadAllLines(file_path);
            Console.WriteLine(all_lines[2]);
            var Line1 = all_lines[0].Split(' ');
            int rows = Convert.ToInt32(Line1[0]);
            int cols = Convert.ToInt32(Line1[1]);
            Model M1 = new Model(rows, cols, Model_Type.Black);

            // پر کردن جدول با خانه‌های سفید
            for (int i = 1; i <= rows; i++)
            {
                for (int j = 1; j <= cols; j++)
                {
                    M1 = new Model(i, j, Model_Type.white);
                }
            }

            // اضافه کردن خانه های سیاه
            int blackCellCount = int.Parse(all_lines[1]);
            int lineIndex = 2;
            for (int i = 1; i <= blackCellCount; i++, lineIndex++)
            {
                string[] parts = all_lines[lineIndex].Split(' ');
                int r = int.Parse(parts[0]);
                int c = int.Parse(parts[1]);
                M1 = new Model(r, c, Model_Type.Black);
            }

            // اضافه کردن خانه های دارای دیتا
            int DataCellCount = int.Parse(all_lines[7]);
            int line_Index_for_data_cell = 8;
            for (int i = 1; i <= DataCellCount; i++, line_Index_for_data_cell++)
            {
                string[] parts = all_lines[line_Index_for_data_cell].Split(' ');
                int r = int.Parse(parts[0]);
                int c = int.Parse(parts[1]);
                int k_L = int.Parse(parts[2]);
                int K_T = int.Parse(parts[3]);
                M1 = new Model(r, c, Model_Type.Data)
                {
                    KeyLeft = k_L,
                    KeyTop = K_T
                };
            }
            return M1;
        }
    }
}
