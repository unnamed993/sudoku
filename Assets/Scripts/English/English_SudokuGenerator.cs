using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class English_SudokuGenerator
{
    private const int N = 9;

    // =========================
    //  Обычный режим (как было)
    // =========================
    public static void CreateSudokuObject(
        out English_SudokuObject finalObject,
        out English_SudokuObject gameObject)
    {
        int[,] solution = new int[N, N];
        SolveFull(solution);

        finalObject = new English_SudokuObject
        {
            Values = (int[,])solution.Clone()
        };

        English_SudokuObject puzzle;
        do
        {
            puzzle = RemoveNumbersAndBalance(solution);
        }
        while (SolveBySingles((int[,])puzzle.Values.Clone()));

        gameObject = puzzle;
    }

    private static bool SolveFull(int[,] board)
    {
        for (int r = 0; r < N; r++)
        {
            for (int c = 0; c < N; c++)
            {
                if (board[r, c] != 0) continue;

                foreach (var num in Enumerable.Range(1, N).OrderBy(_ => UnityEngine.Random.value))
                {
                    if (!IsSafe(board, r, c, num)) continue;

                    board[r, c] = num;
                    if (SolveFull(board)) return true;
                    board[r, c] = 0;
                }
                return false;
            }
        }
        return true;
    }

    private static English_SudokuObject RemoveNumbersAndBalance(int[,] solution)
    {
        var puzzle = new English_SudokuObject
        {
            Values = (int[,])solution.Clone()
        };

        int keepCount = EnglishGameSettings.EasyMiddleHard_Number switch
        {
            0 => 55,
            1 => 50,
            2 => 38,
            3 => 42,
            4 => 28,
            5 => 17,
            _ => 50
        };

        int baseKeep = keepCount / 9;
        int rem = keepCount % 9;
        var rand = new System.Random();

        for (int bi = 0; bi < N; bi++)
        {
            int br = (bi / 3) * 3, bc = (bi % 3) * 3;
            int blockKeep = baseKeep + (bi < rem ? 1 : 0);

            var cells = new List<(int r, int c)>();
            for (int dr = 0; dr < 3; dr++)
                for (int dc = 0; dc < 3; dc++)
                    cells.Add((br + dr, bc + dc));

            for (int i = 0; i < cells.Count; i++)
            {
                int j = rand.Next(i, cells.Count);
                var tmp = cells[i]; cells[i] = cells[j]; cells[j] = tmp;
            }
            for (int i = blockKeep; i < cells.Count; i++)
                puzzle.Values[cells[i].r, cells[i].c] = 0;
        }

        BalanceRowsColumns(solution, puzzle, minGivens: 4, maxGivens: 6);

        int current = puzzle.Values.Cast<int>().Count(v => v != 0);
        int delta = current - keepCount;

        if (delta > 0)
        {
            var filled = new List<(int r, int c)>();
            for (int r = 0; r < N; r++)
                for (int c = 0; c < N; c++)
                    if (puzzle.Values[r, c] != 0)
                        filled.Add((r, c));

            for (int i = 0; i < delta; i++)
            {
                var cell = filled[rand.Next(filled.Count)];
                puzzle.Values[cell.r, cell.c] = 0;
                filled.Remove(cell);
            }
        }
        else if (delta < 0)
        {
            var empty = new List<(int r, int c)>();
            for (int r = 0; r < N; r++)
                for (int c = 0; c < N; c++)
                    if (puzzle.Values[r, c] == 0)
                        empty.Add((r, c));

            for (int i = 0; i < -delta; i++)
            {
                var cell = empty[rand.Next(empty.Count)];
                puzzle.Values[cell.r, cell.c] = solution[cell.r, cell.c];
                empty.Remove(cell);
            }
        }
        return puzzle;
    }

    private static void BalanceRowsColumns(
        int[,] solution,
        English_SudokuObject puzzle,
        int minGivens,
        int maxGivens)
    {
        var rand = new System.Random();

        for (int r = 0; r < N; r++)
        {
            var filled = Enumerable.Range(0, N).Where(c => puzzle.Values[r, c] != 0).ToList();
            if (filled.Count > maxGivens)
            {
                int excess = filled.Count - maxGivens;
                for (int i = 0; i < excess; i++)
                {
                    int c = filled[rand.Next(filled.Count)];
                    puzzle.Values[r, c] = 0;
                    filled.Remove(c);
                }
            }
            else if (filled.Count < minGivens)
            {
                var empty = Enumerable.Range(0, N).Where(c => puzzle.Values[r, c] == 0).ToList();
                int needed = minGivens - filled.Count;
                for (int i = 0; i < needed; i++)
                {
                    int c = empty[rand.Next(empty.Count)];
                    puzzle.Values[r, c] = solution[r, c];
                    empty.Remove(c);
                }
            }
        }

        for (int c = 0; c < N; c++)
        {
            var filled = Enumerable.Range(0, N).Where(r => puzzle.Values[r, c] != 0).ToList();
            if (filled.Count > maxGivens)
            {
                int excess = filled.Count - maxGivens;
                for (int i = 0; i < excess; i++)
                {
                    int r = filled[rand.Next(filled.Count)];
                    puzzle.Values[r, c] = 0;
                    filled.Remove(r);
                }
            }
            else if (filled.Count < minGivens)
            {
                var empty = Enumerable.Range(0, N).Where(r => puzzle.Values[r, c] == 0).ToList();
                int needed = minGivens - filled.Count;
                for (int i = 0; i < needed; i++)
                {
                    int r = empty[rand.Next(empty.Count)];
                    puzzle.Values[r, c] = solution[r, c];
                    empty.Remove(r);
                }
            }
        }
    }

    // =========================
    //  Daily режим (детерминированно от даты)
    // =========================
    // Веса 5/3/2 по вашим тестам:
    // 31 пустая -> keep 50 (50%)
    // 39 пустых -> keep 42 (30%)
    // 43 пустых -> keep 38 (20%)
    public static void CreateSudokuObjectSeeded(
        int seedYYYYMMDD,
        out English_SudokuObject finalObject,
        out English_SudokuObject gameObject,
        out int emptyCount)
    {
        var rng = new System.Random(seedYYYYMMDD);

        int keepCount = PickDailyKeepCount(rng);
        emptyCount = 81 - keepCount;

        int[,] solution = new int[N, N];
        SolveFullSeeded(solution, rng);

        finalObject = new English_SudokuObject
        {
            Values = (int[,])solution.Clone()
        };

        English_SudokuObject puzzle;
        do
        {
            puzzle = RemoveNumbersAndBalanceSeeded(solution, keepCount, rng);
        }
        while (SolveBySingles((int[,])puzzle.Values.Clone()));

        gameObject = puzzle;
    }

    private static int PickDailyKeepCount(System.Random rng)
    {
        int roll = rng.Next(10); // 0..9
        if (roll < 5) return 50; // 31 пустая (50%)
        if (roll < 8) return 42; // 39 пустых (30%)
        return 38;               // 43 пустых (20%)
    }

    private static bool SolveFullSeeded(int[,] board, System.Random rng)
    {
        for (int r = 0; r < N; r++)
        {
            for (int c = 0; c < N; c++)
            {
                if (board[r, c] != 0) continue;

                int[] nums = Enumerable.Range(1, N).ToArray();
                Shuffle(nums, rng);

                for (int i = 0; i < nums.Length; i++)
                {
                    int num = nums[i];
                    if (!IsSafe(board, r, c, num)) continue;

                    board[r, c] = num;
                    if (SolveFullSeeded(board, rng)) return true;
                    board[r, c] = 0;
                }
                return false;
            }
        }
        return true;
    }

    private static void Shuffle<T>(T[] arr, System.Random rng)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            int j = rng.Next(i, arr.Length);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
    }

    private static English_SudokuObject RemoveNumbersAndBalanceSeeded(
        int[,] solution,
        int keepCount,
        System.Random rng)
    {
        var puzzle = new English_SudokuObject
        {
            Values = (int[,])solution.Clone()
        };

        int baseKeep = keepCount / 9;
        int rem = keepCount % 9;

        for (int bi = 0; bi < N; bi++)
        {
            int br = (bi / 3) * 3, bc = (bi % 3) * 3;
            int blockKeep = baseKeep + (bi < rem ? 1 : 0);

            var cells = new List<(int r, int c)>();
            for (int dr = 0; dr < 3; dr++)
                for (int dc = 0; dc < 3; dc++)
                    cells.Add((br + dr, bc + dc));

            for (int i = 0; i < cells.Count; i++)
            {
                int j = rng.Next(i, cells.Count);
                var tmp = cells[i]; cells[i] = cells[j]; cells[j] = tmp;
            }

            for (int i = blockKeep; i < cells.Count; i++)
                puzzle.Values[cells[i].r, cells[i].c] = 0;
        }

        BalanceRowsColumnsSeeded(solution, puzzle, minGivens: 4, maxGivens: 6, rng);

        int current = puzzle.Values.Cast<int>().Count(v => v != 0);
        int delta = current - keepCount;

        if (delta > 0)
        {
            var filled = new List<(int r, int c)>();
            for (int r = 0; r < N; r++)
                for (int c = 0; c < N; c++)
                    if (puzzle.Values[r, c] != 0)
                        filled.Add((r, c));

            for (int i = 0; i < delta; i++)
            {
                var cell = filled[rng.Next(filled.Count)];
                puzzle.Values[cell.r, cell.c] = 0;
                filled.Remove(cell);
            }
        }
        else if (delta < 0)
        {
            var empty = new List<(int r, int c)>();
            for (int r = 0; r < N; r++)
                for (int c = 0; c < N; c++)
                    if (puzzle.Values[r, c] == 0)
                        empty.Add((r, c));

            for (int i = 0; i < -delta; i++)
            {
                var cell = empty[rng.Next(empty.Count)];
                puzzle.Values[cell.r, cell.c] = solution[cell.r, cell.c];
                empty.Remove(cell);
            }
        }

        return puzzle;
    }

    private static void BalanceRowsColumnsSeeded(
        int[,] solution,
        English_SudokuObject puzzle,
        int minGivens,
        int maxGivens,
        System.Random rng)
    {
        for (int r = 0; r < N; r++)
        {
            var filled = Enumerable.Range(0, N).Where(c => puzzle.Values[r, c] != 0).ToList();
            if (filled.Count > maxGivens)
            {
                int excess = filled.Count - maxGivens;
                for (int i = 0; i < excess; i++)
                {
                    int c = filled[rng.Next(filled.Count)];
                    puzzle.Values[r, c] = 0;
                    filled.Remove(c);
                }
            }
            else if (filled.Count < minGivens)
            {
                var empty = Enumerable.Range(0, N).Where(c => puzzle.Values[r, c] == 0).ToList();
                int needed = minGivens - filled.Count;
                for (int i = 0; i < needed; i++)
                {
                    int c = empty[rng.Next(empty.Count)];
                    puzzle.Values[r, c] = solution[r, c];
                    empty.Remove(c);
                }
            }
        }

        for (int c = 0; c < N; c++)
        {
            var filled = Enumerable.Range(0, N).Where(r => puzzle.Values[r, c] != 0).ToList();
            if (filled.Count > maxGivens)
            {
                int excess = filled.Count - maxGivens;
                for (int i = 0; i < excess; i++)
                {
                    int r = filled[rng.Next(filled.Count)];
                    puzzle.Values[r, c] = 0;
                    filled.Remove(r);
                }
            }
            else if (filled.Count < minGivens)
            {
                var empty = Enumerable.Range(0, N).Where(r => puzzle.Values[r, c] == 0).ToList();
                int needed = minGivens - filled.Count;
                for (int i = 0; i < needed; i++)
                {
                    int r = empty[rng.Next(empty.Count)];
                    puzzle.Values[r, c] = solution[r, c];
                    empty.Remove(r);
                }
            }
        }
    }

    // =========================
    //  Общие утилиты (используются и там, и там)
    // =========================
    private static bool IsSafe(int[,] board, int row, int col, int num)
    {
        for (int i = 0; i < N; i++)
            if (board[row, i] == num || board[i, col] == num)
                return false;

        int br = (row / 3) * 3, bc = (col / 3) * 3;
        for (int r = br; r < br + 3; r++)
            for (int c = bc; c < bc + 3; c++)
                if (board[r, c] == num)
                    return false;

        return true;
    }

    private static bool SolveBySingles(int[,] board)
    {
        bool progress;
        do
        {
            progress = false;
            for (int r = 0; r < N; r++)
            {
                for (int c = 0; c < N; c++)
                {
                    if (board[r, c] != 0) continue;

                    var candidates = Enumerable.Range(1, N)
                        .Where(n => IsSafe(board, r, c, n))
                        .ToList();

                    if (candidates.Count == 1)
                    {
                        board[r, c] = candidates[0];
                        progress = true;
                    }
                }
            }
        } while (progress);

        return !board.Cast<int>().Any(v => v == 0);
    }
}
