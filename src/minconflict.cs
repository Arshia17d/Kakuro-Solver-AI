using System;
using System.Collections.Generic;
using System.Linq;

namespace Kakuro
{
    class Solver_MinConflicts
    {
        private Model[,] grid;
        private List<Model> whiteCells;
        private Dictionary<Model, List<Entry>> cellEntries;
        private int maxSteps;
        private int stepCount;
        private Random rand = new Random();

        public int StepCount => stepCount;

        public Solver_MinConflicts(Model[,] grid, int maxSteps = 10000)
        {
            this.grid = grid;
            this.maxSteps = maxSteps;
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            // Collect white cells
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

            // Preprocess entries
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
                        // Horizontal
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

                        // Vertical
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

            // Map cells to entries
            Dictionary<Model, List<Entry>> cellMap = new Dictionary<Model, List<Entry>>();
            foreach (var cell in whiteCells)
            {
                cellMap[cell] = allEntries.Where(e => e.Cells.Contains(cell)).ToList();
            }

            return cellMap;
        }

        public bool Solve()
        {
            Initialize();
            stepCount = 0;

            while (stepCount < maxSteps)
            {
                stepCount++;

                Model conflictedCell = GetConflictedCell();
                if (conflictedCell == null)
                {
                    Console.WriteLine("✅ Solution found in " + stepCount + " steps.");
                    return true;
                }

                int bestValue = GetBestValue(conflictedCell);
                if (bestValue != conflictedCell.Value)
                {
                    conflictedCell.Value = bestValue;
                }
            }

            Console.WriteLine("❌ Max steps reached. No solution found.");
            return false;
        }

        private void Initialize()
        {
            foreach (var cell in whiteCells)
            {
                cell.Value = rand.Next(1, 10);
            }
        }

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

        private int CountConflicts(Model cell)
        {
            int totalConflicts = 0;
            foreach (var entry in cellEntries[cell])
            {
                // Duplicate check
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

                // Sum check
                int remainingCells = entry.Cells.Count - assignedCount;
                if (remainingCells > 0)
                {
                    // Minimum possible sum for remaining cells (1+2+...+remainingCells)
                    int minPossible = remainingCells * (remainingCells + 1) / 2;
                    // Maximum possible sum for remaining cells (9+8+...+(9-remainingCells+1))
                    int maxPossible =
                        (9 * remainingCells) - (remainingCells * (remainingCells - 1)) / 2;

                    if (sum + minPossible > entry.Key || sum + maxPossible < entry.Key)
                    {
                        totalConflicts++;
                    }
                }
                else if (sum != entry.Key)
                {
                    totalConflicts++;
                }
            }

            return totalConflicts;
        }

        private int GetBestValue(Model cell)
        {
            int currentConflicts = CountConflicts(cell);
            int bestValue = cell.Value;
            int minConflicts = currentConflicts;

            for (int val = 1; val <= 9; val++)
            {
                if (val == cell.Value)
                    continue;

                cell.Value = val;
                int newConflicts = CountConflicts(cell);
                cell.Value = bestValue;

                if (newConflicts < minConflicts)
                {
                    minConflicts = newConflicts;
                    bestValue = val;
                }
                else if (newConflicts == minConflicts && val < bestValue)
                {
                    // Prefer smaller values to maximize future options
                    bestValue = val;
                }
            }

            return bestValue;
        }
    }
}
