using System;
using System.Collections.Generic;
using System.Linq;

namespace Kakuro
{
    class Solver_BT_FC_AC3
    {
        private Model[,] grid;
        private List<Model> unassigned;
        private Dictionary<Model, List<Entry>> cellEntries;
        private int nodeCount;

        public int NodeCount => nodeCount;

        public Solver_BT_FC_AC3(Model[,] grid)
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
                    Model cell = grid[i, j];
                    if (cell.Type == Model_Type.white && cell.Value == 0)
                    {
                        cell.Domain = Enumerable.Range(1, 9).ToList(); // Reset domain
                        unassigned.Add(cell);
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

                        // Vertical entry (Bottom Key)
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

            // Initial AC-3 to reduce domains
            if (!AC3())
            {
                Console.WriteLine("❌ AC-3 failed during initialization.");
                return false;
            }

            return Backtrack();
        }

        private bool Backtrack()
        {
            nodeCount++;

            if (unassigned.Count == 0)
                return true;

            Model cell = unassigned[0]; // No MRV for base BT-FC-AC3

            List<int> values = new List<int>(cell.Domain); // Use current domain

            foreach (int value in values)
            {
                cell.Value = value;

                if (IsConsistent(cell))
                {
                    // Remove cell from unassigned
                    unassigned.Remove(cell);

                    // Copy domains before recursion
                    Dictionary<Model, List<int>> savedDomains = SaveDomains();

                    // Apply forward checking
                    if (ForwardCheck(cell, value))
                    {
                        if (Backtrack())
                            return true;
                    }

                    // Restore domains
                    RestoreDomains(savedDomains);
                    unassigned.Add(cell);
                }

                cell.Value = 0; // Unassign
            }

            return false;
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

        private bool AC3()
        {
            Queue<Tuple<Model, Model>> queue = new Queue<Tuple<Model, Model>>();

            // Build arcs (all pairs in the same entry)
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

        private Dictionary<Model, List<int>> SaveDomains()
        {
            Dictionary<Model, List<int>> saved = new Dictionary<Model, List<int>>();
            foreach (var cell in unassigned)
            {
                saved[cell] = new List<int>(cell.Domain);
            }
            return saved;
        }

        private void RestoreDomains(Dictionary<Model, List<int>> saved)
        {
            foreach (var kvp in saved)
            {
                kvp.Key.Domain = new List<int>(kvp.Value);
            }
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
