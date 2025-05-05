using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace IslandTourUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            openFileDialog1.FileName = "";
            txtSelectedFile.Text = openFileDialog1.FileName ;
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(openFileDialog1.FileName))
                {
                    MessageBox.Show("Сначала выберите файл!");
                    return;
                }

                string[] input = File.ReadAllLines(openFileDialog1.FileName);
                var grid = Solution.ParseInput(input);
                var path = Solution.GeneratePath(grid);

                Output.Text = "";
                foreach (var cell in path)
                {
                    Output.AppendText($"{cell.Item1 + 1} {cell.Item2 + 1}\n");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.DefaultExt = ".txt";
            openFileDialog1.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Обновляем интерфейс после выбора файла
                txtSelectedFile.Text = openFileDialog1.FileName;
            }
        }
    }

    static class Solution
    {
        static public char[][] ParseInput(string[] inputLines)
        {
            if (inputLines.Length < 2)
                throw new ArgumentException("Недостаточно строк во входном файле");

            int n = int.Parse(inputLines[0]);
            int m = int.Parse(inputLines[1]);

            if (inputLines.Length - 2 != n)
                throw new ArgumentException("Несоответствие размеров сетки");

            char[][] grid = new char[n][];
            for (int i = 0; i < n; i++)
            {
                grid[i] = inputLines[i + 2].PadRight(m, '.').ToCharArray();
            }
            return grid;
        }

        static public List<Tuple<int, int>> GeneratePath(char[][] grid)
        {
            var neighboringCells = new HashSet<Tuple<int, int>>();

            // 1. Находим все соседние клетки
            for (int i = 0; i < grid.Length; i++)
            {
                for (int j = 0; j < grid[i].Length; j++) // Исправлено на grid[i]
                {
                    if (grid[i][j] == '.' && IsNeighborToIsland(grid, i, j))
                    {
                        neighboringCells.Add(Tuple.Create(i, j));
                    }
                }
            }

            if (neighboringCells.Count == 0)
                return new List<Tuple<int, int>>();

            // 2. Выбираем стартовую клетку
            var startCell = neighboringCells
                .OrderBy(c => c.Item1)
                .ThenBy(c => c.Item2)
                .First();

            // 3. Строим путь
            return BuildPath(neighboringCells, startCell);
        }

        private static bool IsNeighborToIsland(char[][] grid, int x, int y)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = x + dx;
                    int ny = y + dy;

                    // Исправленная проверка границ
                    if (nx >= 0 && nx < grid.Length &&
                        ny >= 0 && ny < grid[nx].Length &&
                        grid[nx][ny] == '#')
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static List<Tuple<int, int>> BuildPath(
            HashSet<Tuple<int, int>> cells,
            Tuple<int, int> start)
        {
            var path = new List<Tuple<int, int>> { start };
            var visited = new HashSet<Tuple<int, int>> { start };

            // Направления: вправо, вниз, влево, вверх
            int[][] directions = {
                new[] { 0, 1 },  // right
                new[] { 1, 0 },  // down
                new[] { 0, -1 }, // left
                new[] { -1, 0 } // up
            };

            int dirIndex = 0;
            int retryCount = 0;

            while (visited.Count < cells.Count)
            {
                var last = path.Last();
                var newDir = directions[dirIndex];
                var nextCell = Tuple.Create(last.Item1 + newDir[0], last.Item2 + newDir[1]);

                if (cells.Contains(nextCell) && !visited.Contains(nextCell))
                {
                    path.Add(nextCell);
                    visited.Add(nextCell);
                    retryCount = 0;
                }
                else
                {
                    dirIndex = (dirIndex + 1) % 4;
                    retryCount++;

                    // Защита от бесконечного цикла
                    if (retryCount > 4) break;
                }
            }

            return path;
        }
    }
}
