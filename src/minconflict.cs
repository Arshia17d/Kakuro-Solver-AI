using System;
using System.Collections.Generic;
using System.Linq;

namespace Kakuro
{
    class Solver_MinConflicts
    {
        private Model[,] grid; // ماتریس جدول کاکورو
        private List<Model> whiteCells; // لیست خانه‌های سفید (قابل مقداردهی)
        private Dictionary<Model, List<Entry>> cellEntries; // نگاشت هر سلول سفید به ورودی‌هایی که در آن‌ها نقش دارد
        private int maxSteps; // حداکثر تعداد گام‌ها برای تلاش جهت حل مسئله
        private int stepCount; // تعداد گام‌هایی که تاکنون طی شده
        private Random rand = new Random(); // برای تولید اعداد تصادفی

        public int StepCount => stepCount;

        // سازنده کلاس که گرید ورودی و تعداد گام مجاز را دریافت می‌کند
        public Solver_MinConflicts(Model[,] grid, int maxSteps = 100000)
        {
            this.grid = grid;
            this.maxSteps = maxSteps;
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            // جمع‌آوری خانه‌های سفید
            whiteCells = new List<Model>();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (grid[i, j].Type == Model_Type.white)
                    {
                        whiteCells.Add(grid[i, j]);
                    }
                }
            }

            // پیش‌پردازش برای ساخت لیست Entryها برای هر خانه
            cellEntries = PreprocessEntries();
        }

        // ساخت لیست Entryها و نگاشت آن‌ها به خانه‌های سفید
        private Dictionary<Model, List<Entry>> PreprocessEntries()
        {
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);
            List<Entry> allEntries = new List<Entry>();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Model cell = grid[i, j];
                    if (cell.Type == Model_Type.Data)
                    {
                        // ساخت ورودی افقی
                        if (cell.RightKey > 0)
                        {
                            Entry entry = new Entry() { Key = cell.RightKey };
                            int col = j + 1;
                            while (col < cols && grid[i, col].Type == Model_Type.white)
                            {
                                entry.Cells.Add(grid[i, col]);
                                col++;
                            }
                            if (entry.Cells.Count > 0)
                                allEntries.Add(entry);
                        }

                        // ساخت ورودی عمودی
                        if (cell.BottonKey > 0)
                        {
                            Entry entry = new Entry() { Key = cell.BottonKey };
                            int row = i + 1;
                            while (row < rows && grid[row, j].Type == Model_Type.white)
                            {
                                entry.Cells.Add(grid[row, j]);
                                row++;
                            }
                            if (entry.Cells.Count > 0)
                                allEntries.Add(entry);
                        }
                    }
                }
            }

            // نگاشت هر خانه سفید به ورودی‌هایی که در آن نقش دارد
            Dictionary<Model, List<Entry>> cellMap = new Dictionary<Model, List<Entry>>();
            foreach (var cell in whiteCells)
            {
                cellMap[cell] = allEntries.Where(e => e.Cells.Contains(cell)).ToList();
            }

            return cellMap;
        }

        // تابع اصلی حل معما با استفاده از الگوریتم Min-Conflicts
        public bool Solve()
        {
            Initialize(); // مقداردهی اولیه تصادفی
            stepCount = 0;

            while (stepCount < maxSteps)
            {
                stepCount++;

                Model conflictedCell = GetConflictedCell(); // یافتن یک خانه دارای تضاد
                if (conflictedCell == null)
                {
                    Console.WriteLine("✅ Solution found in " + stepCount + " steps.");
                    return true; // اگر هیچ خانه‌ای تضاد نداشت، مسئله حل شده
                }

                int bestValue = GetBestValue(conflictedCell); // یافتن بهترین مقدار برای کاهش تضاد
                if (bestValue != conflictedCell.Value)
                {
                    conflictedCell.Value = bestValue; // مقداردهی جدید
                }
            }

            // اگر به سقف گام‌ها رسیدیم و حل نشد
            Console.WriteLine("❌ Max steps reached. No solution found.");
            return false;
        }

        // مقداردهی اولیه تصادفی به تمام خانه‌های سفید
        private void Initialize()
        {
            foreach (var cell in whiteCells)
            {
                cell.Value = rand.Next(1, 10); // مقدار تصادفی بین ۱ تا ۹
            }
        }

        // یافتن یک سلول دارای تضاد به‌صورت تصادفی از بین همه سلول‌های دارای تضاد
        private Model GetConflictedCell()
        {
            List<Model> conflicted = new List<Model>();
            foreach (var cell in whiteCells)
            {
                int conflicts = CountConflicts(cell);
                if (conflicts > 0)
                {
                    conflicted.Add(cell);
                }
            }

            return conflicted.Count == 0 ? null : conflicted[rand.Next(conflicted.Count)];
        }

        // شمارش تعداد تضادهای یک سلول خاص
        private int CountConflicts(Model cell)
        {
            int totalConflicts = 0;
            foreach (var entry in cellEntries[cell])
            {
                // بررسی تکراری بودن مقادیر
                HashSet<int> seen = new HashSet<int>();
                bool hasDuplicate = false;
                int sum = 0;
                int assignedCount = 0;
                foreach (var c in entry.Cells)
                {
                    if (c.Value == 0)
                        continue;

                    if (seen.Contains(c.Value))
                    {
                        hasDuplicate = true;
                        break;
                    }
                    seen.Add(c.Value);
                    sum += c.Value;
                    assignedCount++;
                }

                if (hasDuplicate)
                {
                    totalConflicts++;
                    continue;
                }

                // بررسی مجموع
                int remainingCells = entry.Cells.Count - assignedCount;
                if (remainingCells > 0)
                {
                    // حداقل مجموع ممکن
                    int minPossible = remainingCells * (remainingCells + 1) / 2;
                    // حداکثر مجموع ممکن
                    int maxPossible =
                        (9 * remainingCells) - (remainingCells * (remainingCells - 1)) / 2;

                    // اگر مجموع فعلی + کمترین/بیشترین مقدار ممکن، از کل بیشتر/کمتر شد، تضاد داریم
                    if (sum + minPossible > entry.Key || sum + maxPossible < entry.Key)
                    {
                        totalConflicts++;
                    }
                }
                else if (sum != entry.Key) // اگر تمام خانه‌ها مقدار دارند ولی جمع نادرست است
                {
                    totalConflicts++;
                }
            }

            return totalConflicts;
        }

        // بهترین مقدار ممکن برای کاهش تضاد برای یک سلول خاص را پیدا می‌کند
        private int GetBestValue(Model cell)
        {
            int currentConflicts = CountConflicts(cell);
            int bestValue = cell.Value;
            int minConflicts = currentConflicts;

            for (int val = 1; val <= 9; val++)
            {
                if (val == cell.Value)
                    continue;

                cell.Value = val; // موقتی مقدار می‌گذاریم
                int newConflicts = CountConflicts(cell);
                cell.Value = bestValue; // بازگرداندن مقدار اصلی

                if (newConflicts < minConflicts)
                {
                    minConflicts = newConflicts;
                    bestValue = val;
                }
                else if (newConflicts == minConflicts && val < bestValue)
                {
                    // اگر تضاد برابر بود، عدد کوچکتر را ترجیح می‌دهیم
                    bestValue = val;
                }
            }

            return bestValue;
        }
    }
}
