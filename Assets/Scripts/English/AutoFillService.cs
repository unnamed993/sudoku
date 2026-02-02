using System.Collections.Generic;

public sealed class AutoFillService
{
    public int CountEmpty(int[,] board)
    {
        int k = 0;
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (board[r, c] == 0) k++;
        return k;
    }

    public bool HasActiveErrors(int[,] board, int[,] solution)
    {
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (board[r, c] != 0 && board[r, c] != solution[r, c])
                    return true;
        return false;
    }

    public bool IsAutoFillAvailable(int[,] board, int[,] solution, int threshold = 12)
    {
        if (board == null || solution == null) return false;
        return !HasActiveErrors(board, solution) && CountEmpty(board) == threshold;
    }

    public IEnumerable<(int r, int c, int v)> GetEmptyCellsRowOrder(int[,] board, int[,] solution)
    {
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (board[r, c] == 0)
                    yield return (r, c, solution[r, c]);
    }
}
