using System;
using System.Collections.Generic;
using System.Linq;

namespace Kakuro
{
    class Solver_BT_FC_MRV
    {
        private Model[,] grid; // ماتریس اصلی بازی
        private List<Model> unassigned; // لیست خانه‌های سفید که مقدار ندارند
        private Dictionary<Model, List<Entry>> cellEntries; // نگاشت هر خانه سفید به لیستی از ورودی‌هایی که متعلق به آن هستند
        private int nodeCount; // شمارنده گره‌ها (برای تحلیل عملکرد)

        public int NodeCount => nodeCount; // ویژگی فقط خواندنی برای nodeCount

        public Solver_BT_FC_MRV(Model[,] grid)
        {
            this.grid = grid;
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            // استخراج خانه‌های سفید بدون مقدار برای شروع حل
            unassigned = new List<Model>();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (grid[i, j].Type == Model_Type.white && grid[i, j].Value == 0)
                    {
                        unassigned.Add(grid[i, j]);
                    }
                }
            }

            // پیش‌پردازش و ایجاد نگاشت خانه ← ورودی‌ها
            cellEntries = PreprocessEntries();
        }

        // ایجاد ورودی‌های افقی و عمودی و نگاشت آن‌ها به هر خانه سفید
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
                        // ورودی افقی (کلید سمت راست)
                        if (cell.RightKey > 0)
                        {
                            Entry entry = new Entry();
                            entry.Key = cell.RightKey;
                            int col = j + 1;
                            while (col < cols && grid[i, col].Type == Model_Type.white)
                            {
                                entry.Cells.Add(grid[i, col]);
                                col++;
                            }
                            if (entry.Cells.Count > 0)
                                allEntries.Add(entry);
                        }

                        // ورودی عمودی (کلید پایین)
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

            // نگاشت هر خانه سفید به ورودی‌هایی که در آن‌ها حضور دارد
            Dictionary<Model, List<Entry>> cellMap = new Dictionary<Model, List<Entry>>();
            foreach (var cell in unassigned)
            {
                cellMap[cell] = allEntries.Where(e => e.Cells.Contains(cell)).ToList();
            }

            return cellMap;
        }

        // شروع فرآیند حل
        public bool Solve()
        {
            nodeCount = 0;
            return Backtrack();
        }

        // الگوریتم بازگشت به عقب (Backtracking)
        private bool Backtrack()
        {
            nodeCount++;

            // شرط پایان: اگر همه خانه‌ها مقدار گرفته‌اند
            if (unassigned.Count == 0)
                return true;

            // انتخاب خانه‌ای با کمترین دامنه مقادیر ممکن (MRV)
            Model cell = SelectMRVCell();

            // مقادیر ممکن برای خانه انتخاب شده
            List<int> values = PossibleValues(cell);

            foreach (int value in values)
            {
                cell.Value = value;

                if (IsConsistent(cell)) // بررسی سازگاری با محدودیت‌ها
                {
                    unassigned.Remove(cell); // حذف از لیست خانه‌های حل نشده

                    if (Backtrack()) // ادامه بازگشتی
                        return true;

                    unassigned.Add(cell); // بازگرداندن به لیست در صورت بن‌بست
                }

                cell.Value = 0; // بازگرداندن مقدار به حالت حل نشده
            }

            return false; // بازگشت برای امتحان مقادیر دیگر
        }

        // انتخاب خانه با کمترین تعداد مقادیر ممکن (MRV)
        private Model SelectMRVCell()
        {
            Model selected = null;
            int minValues = int.MaxValue;

            foreach (var cell in unassigned)
            {
                List<int> values = PossibleValues(cell);
                if (values.Count < minValues)
                {
                    minValues = values.Count;
                    selected = cell;
                }
                else if (values.Count == minValues)
                {
                    // در صورت تساوی، انتخاب خانه‌ای که زودتر در جدول آمده (اول ردیف، سپس ستون)
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

        // بررسی مقادیر مجاز برای یک خانه با استفاده از Forward Checking
        private List<int> PossibleValues(Model cell)
        {
            List<int> values = new List<int>();
            List<Entry> entries = cellEntries[cell];

            HashSet<int> used = new HashSet<int>();
            foreach (var entry in entries)
            {
                foreach (var c in entry.Cells)
                {
                    if (c != cell && c.Value != 0)
                    {
                        used.Add(c.Value); // عددهای استفاده‌شده در این ورودی‌ها
                    }
                }
            }

            for (int v = 1; v <= 9; v++)
            {
                if (!used.Contains(v))
                {
                    cell.Value = v;
                    if (IsConsistent(cell)) // بررسی سازگاری مقدار فعلی
                    {
                        values.Add(v);
                    }
                }
            }

            cell.Value = 0; // ریست مقدار برای ادامه بررسی
            return values;
        }

        // بررسی اینکه مقدار فعلی خانه با قوانین سازگار است یا نه
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
                        if (seen.Contains(c.Value)) // تکراری بودن مقدار
                            return false;
                        seen.Add(c.Value);
                        sum += c.Value;
                        assignedCount++;
                    }
                }

                int remaining = entry.Cells.Count - assignedCount;

                if (sum > entry.Key) // اگر جمع فعلی بیشتر از مقدار هدف باشد
                    return false;

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

                    // بررسی اینکه جمع باقی‌مانده بین حداقل و حداکثر ممکن هست یا نه
                    if (!(minSum <= (entry.Key - sum) && (entry.Key - sum) <= maxSum))
                        return false;
                }
                else if (sum != entry.Key)
                {
                    return false;
                }
            }

            return true;
        }

        // بررسی اینکه آیا می‌توان با تعداد مشخصی از اعداد موجود، به مجموع هدف رسید یا نه
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

            var sorted = digits.OrderBy(d => d).ToList();
            minSum = 0;
            maxSum = 0;

            for (int i = 0; i < count; i++)
                minSum += sorted[i];

            for (int i = digits.Count - 1; i >= digits.Count - count; i--)
                maxSum += sorted[i];

            return target >= minSum && target <= maxSum;
        }
    }
}
