using System;
using System.Collections.Generic;
using System.Linq;

namespace Kakuro
{
    class Solver_BT_FC_MRV
    {
        private Model[,] grid;
        private List<Model> unassigned;
        private Dictionary<Model, List<Entry>> cellEntries;
        private int nodeCount;

        public int NodeCount => nodeCount;

        public Solver_BT_FC_MRV(Model[,] grid)
        {
            this.grid = grid;
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            // Collect unassigned white cells
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

            // Preprocess entries and build mapping
            cellEntries = PreprocessEntries();
        }

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
                        // Horizontal entry (Right Key)
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

                        // Vertical entry (Bottom Key)
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

            // Map each white cell to its entries
            Dictionary<Model, List<Entry>> cellMap = new Dictionary<Model, List<Entry>>();
            foreach (var cell in unassigned)
            {
                cellMap[cell] = allEntries.Where(e => e.Cells.Contains(cell)).ToList();
            }

            return cellMap;
        }

        public bool Solve()
        {
            nodeCount = 0;
            return Backtrack();
        }

        private bool Backtrack()
        {
            nodeCount++;

            if (unassigned.Count == 0)
                return true;

            // Select cell with minimum remaining values (MRV)
            Model cell = SelectMRVCell();

            List<int> values = PossibleValues(cell);

            foreach (int value in values)
            {
                cell.Value = value;

                if (IsConsistent(cell))
                {
                    // Remove cell from unassigned
                    unassigned.Remove(cell);

                    if (Backtrack())
                        return true;

                    // Restore cell if backtracking
                    unassigned.Add(cell);
                }

                cell.Value = 0; // Unassign
            }

            return false;
        }

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
                    // Tie-breaker: row-major order
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
                        used.Add(c.Value);
                    }
                }
            }

            for (int v = 1; v <= 9; v++)
            {
                if (!used.Contains(v))
                {
                    cell.Value = v;
                    if (IsConsistent(cell))
                    {
                        values.Add(v);
                    }
                }
            }

            cell.Value = 0;
            return values;
        }

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
                            return false;
                        seen.Add(c.Value);
                        sum += c.Value;
                        assignedCount++;
                    }
                }

                int remaining = entry.Cells.Count - assignedCount;

                if (sum > entry.Key)
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
