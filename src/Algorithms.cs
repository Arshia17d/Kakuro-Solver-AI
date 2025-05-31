using System;
using System.Collections.Generic;
using System.Linq;

namespace Kakuro
{
    class Solver_BT_FC
    {
        // ماتریسی که وضعیت فعلی جدول کاکورو رو نگه می‌داره
        private Model[,] grid;

        // لیستی از خونه‌های سفید (که هنوز مقدار نگرفتن)
        private List<Model> unassigned;

        // نگه‌دارندهٔ هر خانه و لیست بلاک‌های عمودی و افقی مربوط بهش
        private Dictionary<Model, List<Entry>> cellEntries;

        // شمارندهٔ تعداد گره‌هایی که درخت جستجو پیمایش کرده
        private int nodeCount;

        // Property فقط خواندنی برای گرفتن nodeCount
        public int NodeCount => nodeCount;

        // سازنده کلاس: مقداردهی اولیه ماتریس، خانه‌های سفید، و بلاک‌ها
        public Solver_BT_FC(Model[,] grid)
        {
            this.grid = grid;
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            // استخراج همهٔ خونه‌های سفید از جدول و اضافه‌کردن به لیست unassigned
            unassigned = new List<Model>();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (grid[i, j].Type == Model_Type.white)
                    {
                        unassigned.Add(grid[i, j]);
                    }
                }
            }

            // آماده‌سازی اطلاعات ورودها (بلاک‌های افقی و عمودی)
            cellEntries = PreprocessEntries();
        }

        // تابعی که برای هر خانه سفید، بلاک‌های افقی و عمودی مربوطه رو مشخص می‌کنه
        private Dictionary<Model, List<Entry>> PreprocessEntries()
        {
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);
            List<Entry> allEntries = new List<Entry>(); // لیست همه بلاک‌ها

            // پیمایش کل جدول برای پیدا کردن خانه‌های داده‌ای (حاوی کلید)
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Model cell = grid[i, j];
                    if (cell.Type == Model_Type.Data)
                    {
                        // اگر خانه کلید افقی دارد، بلاک افقی بساز
                        if (cell.RightKey > 0)
                        {
                            Entry entry = new Entry();
                            entry.Key = cell.RightKey;
                            int col = j + 1;

                            // تا زمانی که خانه سفید هست، به بلاک اضافه کن
                            while (col < cols && grid[i, col].Type == Model_Type.white)
                            {
                                entry.Cells.Add(grid[i, col]);
                                col++;
                            }

                            if (entry.Cells.Count > 0)
                                allEntries.Add(entry);
                        }

                        // اگر خانه کلید عمودی دارد، بلاک عمودی بساز
                        if (cell.BottonKey > 0)
                        {
                            Entry entry = new Entry();
                            entry.Key = cell.BottonKey;
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

            // نگاشت هر خانه سفید به بلاک‌هایی که در آن حضور دارد
            Dictionary<Model, List<Entry>> cellMap = new Dictionary<Model, List<Entry>>();
            foreach (var cell in unassigned)
            {
                cellMap[cell] = allEntries.Where(e => e.Cells.Contains(cell)).ToList();
            }

            return cellMap;
        }

        // تابع اصلی حل معما، با شروع از گره اول
        public bool Solve()
        {
            return Backtrack(0);
        }

        // الگوریتم بازگشتی backtracking
        private bool Backtrack(int index)
        {
            nodeCount++; // ثبت تعداد گره‌های بازدید شده

            // اگر همه خانه‌ها مقدار گرفتن، حل کامل شده
            if (index >= unassigned.Count)
                return true;

            // گرفتن خانه فعلی از لیست
            Model cell = unassigned[index];
            List<Entry> entries = cellEntries[cell]; // بلاک‌های مربوط به این خانه

            // پیمایش همه مقادیر ممکن برای این خانه
            foreach (int value in PossibleValues(entries, cell))
            {
                cell.Value = value; // مقداردهی موقت

                if (IsConsistent(cell, entries)) // اگر شرط‌ها نقض نشد
                {
                    if (Backtrack(index + 1)) // برو به خانه بعدی
                        return true;
                }

                cell.Value = 0; // اگر جواب نداد، مقدار رو حذف کن (backtrack)
            }

            return false; // هیچ مقداری جواب نداد، عقب‌گرد
        }

        // تابعی برای محاسبه مقادیر ممکن که هنوز در بلاک‌ها استفاده نشده
        private List<int> PossibleValues(List<Entry> entries, Model cell)
        {
            HashSet<int> used = new HashSet<int>(); // اعدادی که در بلاک‌ها استفاده شدن

            foreach (var entry in entries)
            {
                foreach (var c in entry.Cells)
                {
                    if (c != cell && c.Value != 0)
                    {
                        used.Add(c.Value);
                    }
                }
            }

            List<int> possible = new List<int>();
            for (int v = 1; v <= 9; v++)
            {
                if (!used.Contains(v)) // فقط اعدادی که قبلاً استفاده نشدن
                    possible.Add(v);
            }

            return possible;
        }

        // بررسی سازگاری مقدار انتخاب شده با قوانین بلاک‌ها
        private bool IsConsistent(Model cell, List<Entry> entries)
        {
            foreach (var entry in entries)
            {
                HashSet<int> values = new HashSet<int>();
                int sum = 0;
                int assignedCount = 0;

                foreach (var c in entry.Cells)
                {
                    if (c.Value != 0)
                    {
                        if (values.Contains(c.Value))
                            return false; // تکراری ممنوع
                        values.Add(c.Value);
                        sum += c.Value;
                        assignedCount++;
                    }
                }

                if (sum > entry.Key) // اگر جمع فعلی از کلید بیشتر شد، ناسازگار
                    return false;

                int remainingCells = entry.Cells.Count - assignedCount;

                if (remainingCells > 0)
                {
                    List<int> availableDigits = new List<int>();
                    for (int d = 1; d <= 9; d++)
                    {
                        if (!values.Contains(d))
                            availableDigits.Add(d);
                    }

                    // بررسی اینکه آیا میشه مجموع باقی‌مانده رو با اعداد آزاد ساخت؟
                    if (
                        !CanAchieveSum(
                            availableDigits,
                            remainingCells,
                            entry.Key - sum,
                            out int minSum,
                            out int maxSum
                        )
                    )
                        return false;

                    // چک می‌کنیم مجموع باقی‌مانده داخل بازه مجاز هست یا نه
                    if (!(minSum <= (entry.Key - sum) && (entry.Key - sum) <= maxSum))
                        return false;
                }
                else if (sum != entry.Key) // اگر همه پر شده ولی مجموع درست نیست
                {
                    return false;
                }
            }

            return true; // همه چیز درسته
        }

        // بررسی اینکه آیا می‌تونیم مجموع هدف رو با ترکیب اعداد داده شده بسازیم
        private bool CanAchieveSum(
            List<int> digits,
            int count,
            int target,
            out int minSum,
            out int maxSum
        )
        {
            if (digits.Count < count)
            {
                minSum = maxSum = 0;
                return false;
            }

            var sorted = digits.OrderBy(d => d).ToList(); // مرتب کردن از کوچک به بزرگ
            minSum = 0;
            maxSum = 0;

            // کمترین مجموع ممکن با انتخاب کوچکترین اعداد
            for (int i = 0; i < count; i++)
                minSum += sorted[i];

            // بیشترین مجموع ممکن با انتخاب بزرگترین اعداد
            for (int i = digits.Count - 1; i >= digits.Count - count; i--)
                maxSum += sorted[i];

            // بررسی اینکه هدف در این بازه هست یا نه
            return target >= minSum && target <= maxSum;
        }
    }
}
