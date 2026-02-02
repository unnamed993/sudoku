using System;

public class SimpleSudokuGenerator
{
    private static readonly int N = 9;

    public static bool SolveSudoku(int[,] board)
    {
        for (int row = 0; row < N; row++)
        {
            for (int col = 0; col < N; col++)
            {
                if (board[row, col] == 0)
                {
                    for (int num = 1; num <= 9; num++)
                    {
                        if (IsSafe(board, row, col, num))
                        {
                            board[row, col] = num;
                            if (SolveSudoku(board))
                                return true;
                            board[row, col] = 0;
                        }
                    }
                    return false;
                }
            }
        }
        return true;
    }

    private static bool IsSafe(int[,] board, int row, int col, int num)
    {
        return IsSafeInRow(board, row, num) && IsSafeInCol(board, col, num) && IsSafeInBox(board, row, col, num);
    }

    private static bool IsSafeInRow(int[,] board, int row, int num)
    {
        for (int col = 0; col < N; col++)
        {
            if (board[row, col] == num)
                return false;
        }
        return true;
    }

    private static bool IsSafeInCol(int[,] board, int col, int num)
    {
        for (int row = 0; row < N; row++)
        {
            if (board[row, col] == num)
                return false;
        }
        return true;
    }

    private static bool IsSafeInBox(int[,] board, int row, int col, int num)
    {
        int startRow = row - row % 3;
        int startCol = col - col % 3;
        for (int r = startRow; r < startRow + 3; r++)
        {
            for (int c = startCol; c < startCol + 3; c++)
            {
                if (board[r, c] == num)
                    return false;
            }
        }
        return true;
    }

    public static void RemoveNumbers(int[,] board, int count)
    {
        Random rand = new Random();
        while (count > 0)
        {
            int row = rand.Next(N);
            int col = rand.Next(N);
            if (board[row, col] != 0)
            {
                board[row, col] = 0;
                count--;
            }
        }
    }

    public static void PrintBoard(int[,] board)
    {
        for (int row = 0; row < N; row++)
        {
            for (int col = 0; col < N; col++)
            {
                Console.Write(board[row, col] + " ");
            }
            Console.WriteLine();
        }
    }

    public static void GenerateSudoku(out int[,] board, int emptyCells)
    {
        board = new int[N, N];
        SolveSudoku(board);
        RemoveNumbers(board, emptyCells);
    }
}
