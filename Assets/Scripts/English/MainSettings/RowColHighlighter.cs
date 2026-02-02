using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RowColHighlighter : MonoBehaviour
{
    [SerializeField] public Color highlightColor = new Color(0.89f, 0.94f, 0.98f, 1f);
    [SerializeField] public Color baseCellColor = Color.white;
    public bool backlightEnabled = true;
    private Image[,] cellImages;

    public void Init(Dictionary<Tuple<int, int>, EnglishFieldPrefabObject> cells)
    {
        cellImages = new Image[9, 9];
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                cellImages[r, c] = cells[new Tuple<int, int>(r, c)].Instance.GetComponent<Image>();
    }

    public void Highlight(int row, int col)
    {
        if (!backlightEnabled || cellImages == null) return;

        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                cellImages[r, c].color = baseCellColor;

        for (int i = 0; i < 9; i++)
        {
            cellImages[row, i].color = highlightColor;
            cellImages[i, col].color = highlightColor;
        }
    }

    public void HighlightRowColAndBlock(int row, int col)
    {
        if (!backlightEnabled || cellImages == null) return;

        ClearHighlight();

        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                cellImages[r, c].color = baseCellColor;

        // Строка/столбец: НЕ красим (row,col)
        for (int i = 0; i < 9; i++)
        {
            if (i != col) cellImages[row, i].color = highlightColor;
            if (i != row) cellImages[i, col].color = highlightColor;
        }

        // Блок 3×3: тоже НЕ красим центр
        int sr = (row / 3) * 3, sc = (col / 3) * 3;
        for (int r = sr; r < sr + 3; r++)
            for (int c = sc; c < sc + 3; c++)
                if (!(r == row && c == col))
                    cellImages[r, c].color = highlightColor;
    }

    public void ClearHighlight()
    {
        if (cellImages == null) return;
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                cellImages[r, c].color = baseCellColor;
    }
}
