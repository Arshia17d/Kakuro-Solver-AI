using System;
using System.Collections.Generic;
using System.Linq;

namespace Kakuro
{
    // کلاس حل‌کننده سودوکو Kakuro با الگوریتم BT + FC + MRV + AC3
    class Solver_BT_FC_MRV_AC
    {
        private Model[,] grid; // ماتریس جدول بازی
        private List<Model> unassigned; // لیست خانه‌های سفید حل‌نشده
        private Dictionary<Model, List<Entry>> cellEntries; // نگاشت هر خانه سفید به لیست گروه‌های مرتبط (افقی و عمودی)
        private int nodeCount; // تعداد گره‌های بررسی شده (برای آمار)

        public int NodeCount => nodeCount; // ویژگی فقط خواندنی برای مشاهده nodeCount

        // سازنده کلاس: مقداردهی اولیه جدول و آماده‌سازی خانه‌ها و گروه‌ها
        public Solver_BT_FC_MRV_AC(Model[,] grid)
        {
            this.grid = grid;
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            unassigned = new List<Model>();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Model cell = grid[i, j];
                    if (cell.Type == Model_Type.white && cell.Value == 0)
                    {
                        cell.Domain = Enumerable.Range(1, 9).ToList(); // بازنشانی دامنه خانه سفید (1 تا 9)
                        unassigned.Add(cell);
                    }
                }
            }

            // ساخت نقشهٔ اتصال خانه‌ها به گروه‌هایشان (افقی و عمودی)
            cellEntries = PreprocessEntries();
        }

        // ساخت لیست گروه‌های افقی و عمودی و نگاشت هر خانه سفید به گروه‌های مربوطه‌اش
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
                        // گروه افقی
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

                        // گروه عمودی
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

            // نگاشت هر خانه سفید به گروه‌هایش
            Dictionary<Model, List<Entry>> cellMap = new Dictionary<Model, List<Entry>>();
            foreach (var cell in unassigned)
            {
                cellMap[cell] = allEntries.Where(e => e.Cells.Contains(cell)).ToList();
            }

            return cellMap;
        }

        // تابع شروع حل
        public bool Solve()
        {
            nodeCount = 0;

            // مرحله اولیه: اعمال AC3 برای کاهش دامنه‌ها
            if (!AC3())
            {
                Console.WriteLine("❌ AC-3 failed during initialization.");
                return false;
            }

            return Backtrack();
        }

        // الگوریتم عقب‌گرد (Backtracking)
        private bool Backtrack()
        {
            nodeCount++;

            if (unassigned.Count == 0)
                return true; // همه خانه‌ها مقداردهی شدند

            Model cell = SelectMRVCell(); // انتخاب خانه‌ای با کمترین دامنه

            List<int> values = new List<int>(cell.Domain);

            foreach (int value in values)
            {
                cell.Value = value; // مقداردهی آزمایشی

                if (IsConsistent(cell))
                {
                    unassigned.Remove(cell);

                    Dictionary<Model, List<int>> savedDomains = SaveDomains();

                    if (ForwardCheck(cell, value) && AC3())
                    {
                        if (Backtrack())
                            return true;
                    }

                    RestoreDomains(savedDomains);
                    unassigned.Add(cell);
                }

                cell.Value = 0; // بازگرداندن مقدار
            }

            return false; // هیچ مقدار معتبری پیدا نشد
        }

        // انتخاب خانه‌ای با کمترین دامنه (MRV)
        private Model SelectMRVCell()
        {
            Model selected = null;
            int minDomainSize = int.MaxValue;

            foreach (var cell in unassigned)
            {
                if (cell.Domain.Count < minDomainSize)
                {
                    minDomainSize = cell.Domain.Count;
                    selected = cell;
                }
                else if (cell.Domain.Count == minDomainSize)
                {
                    // اولویت با سطر پایین‌تر، سپس ستون چپ‌تر
                    if (
                        (cell.Row < selected.Row)
                        || (cell.Row == selected.Row && cell.Col < selected.Col)
                    )
                    {
                        selected = cell;
                    }
                }
            }

            return selected;
        }

        // بررسی سازگاری مقدار تخصیص داده‌شده به خانه با گروه‌های مرتبط
        private bool IsConsistent(Model cell)
        {
            List<Entry> entries = cellEntries[cell];

            foreach (var entry in entries)
            {
                HashSet<int> seen = new HashSet<int>();
                int sum = 0;
                int assignedCount = 0;

                foreach (var c in entry.Cells)
                {
                    if (c.Value != 0)
                    {
                        if (seen.Contains(c.Value))
                            return false; // تکرار عدد ممنوع است
                        seen.Add(c.Value);
                        sum += c.Value;
                        assignedCount++;
                    }
                }

                int remaining = entry.Cells.Count - assignedCount;

                if (sum > entry.Key)
                    return false; // جمع بیشتر از مقدار موردنظر

                if (remaining > 0)
                {
                    List<int> available = new List<int>();
                    for (int d = 1; d <= 9; d++)
                    {
                        if (!seen.Contains(d))
                            available.Add(d);
                    }

                    if (
                        !CanAchieveSum(
                            available,
                            remaining,
                            entry.Key - sum,
                            out int minSum,
                            out int maxSum
                        )
                    )
                        return false;

                    if (!(minSum <= (entry.Key - sum) && (entry.Key - sum) <= maxSum))
                        return false;
                }
                else if (sum != entry.Key)
                {
                    return false; // جمع نهایی باید دقیقاً برابر مقدار موردنظر باشد
                }
            }

            return true;
        }

        // بررسی رو به جلو (Forward Checking)
        private bool ForwardCheck(Model cell, int value)
        {
            foreach (var entry in cellEntries[cell])
            {
                int sum = 0;
                int assigned = 0;

                foreach (var c in entry.Cells)
                {
                    if (c.Value != 0)
                    {
                        sum += c.Value;
                        assigned++;
                    }
                }

                if (sum > entry.Key)
                    return false;

                int remaining = entry.Cells.Count - assigned;
                if (remaining > 0)
                {
                    List<int> available = new List<int>();
                    for (int d = 1; d <= 9; d++)
                    {
                        if (d != value)
                            available.Add(d);
                    }

                    if (
                        !CanAchieveSum(
                            available,
                            remaining,
                            entry.Key - sum,
                            out int minSum,
                            out int maxSum
                        )
                    )
                        return false;
                }
            }

            return true;
        }

        // الگوریتم AC-3 برای کاهش دامنه‌ها با استفاده از قیود دوتایی
        private bool AC3()
        {
            Queue<Tuple<Model, Model>> queue = new Queue<Tuple<Model, Model>>();

            foreach (var entry in cellEntries.Values.SelectMany(x => x).Distinct())
            {
                for (int i = 0; i < entry.Cells.Count; i++)
                {
                    for (int j = 0; j < entry.Cells.Count; j++)
                    {
                        if (i != j)
                        {
                            queue.Enqueue(Tuple.Create(entry.Cells[i], entry.Cells[j]));
                        }
                    }
                }
            }

            while (queue.Count > 0)
            {
                var arc = queue.Dequeue();
                Model xi = arc.Item1;
                Model xj = arc.Item2;

                if (Revise(xi, xj))
                {
                    if (xi.Domain.Count == 0)
                        return false;

                    foreach (var entry in cellEntries[xi])
                    {
                        foreach (var xk in entry.Cells)
                        {
                            if (xk != xi && xk != xj)
                            {
                                queue.Enqueue(Tuple.Create(xk, xi));
                            }
                        }
                    }
                }
            }

            return true;
        }

        // حذف مقادیر ناسازگار از دامنه xi بر اساس دامنه xj
        private bool Revise(Model xi, Model xj)
        {
            bool revised = false;
            List<int> toRemove = new List<int>();

            foreach (int x in xi.Domain)
            {
                bool found = false;
                foreach (int y in xj.Domain)
                {
                    if (x != y)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    toRemove.Add(x);
                    revised = true;
                }
            }

            foreach (int val in toRemove)
            {
                xi.Domain.Remove(val);
            }

            return revised;
        }

        // ذخیره‌سازی دامنه‌های فعلی برای بازگشت در صورت نیاز
        private Dictionary<Model, List<int>> SaveDomains()
        {
            Dictionary<Model, List<int>> saved = new Dictionary<Model, List<int>>();
            foreach (var cell in unassigned)
            {
                saved[cell] = new List<int>(cell.Domain);
            }
            return saved;
        }

        // بازگردانی دامنه‌ها به حالت ذخیره شده قبلی
        private void RestoreDomains(Dictionary<Model, List<int>> saved)
        {
            foreach (var kvp in saved)
            {
                kvp.Key.Domain = new List<int>(kvp.Value);
            }
        }

        // بررسی اینکه آیا با n رقم از لیست digits می‌توان به عدد target رسید یا نه
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

            var sorted = digits.OrderBy(d => d).ToList(); // مرتب‌سازی لیست اعداد صعودی
            minSum = 0;
            maxSum = 0;

            for (int i = 0; i < count; i++)
                minSum += sorted[i]; // جمع کوچک‌ترین‌ها

            for (int i = digits.Count - 1; i >= digits.Count - count; i--)
                maxSum += sorted[i]; // جمع بزرگ‌ترین‌ها

            return target >= minSum && target <= maxSum; // آیا target در این بازه قرار دارد؟
        }
    }
}
