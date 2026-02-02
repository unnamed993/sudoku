using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections.Generic;

public class EnglishInform : MonoBehaviour
{
    public EnglishGame englishGame;

    public void HandleNoteInput(int number, EnglishFieldPrefabObject field)
    {
        if (field.TryGetTextByName($"Number_{number}", out Text ts))
            ts.text = (ts.text == number.ToString()) ? "" : number.ToString();
    }

    public void AutoDeleteNotes(int number, int row, int col)
    {
        var dict = englishGame.EnglishFieldPrefabObjectDic;

        for (int c = 0; c < 9; c++)
            if (dict[new Tuple<int, int>(row, c)].TryGetTextByName($"Number_{number}", out Text ts))
                ts.text = "";

        for (int r = 0; r < 9; r++)
            if (dict[new Tuple<int, int>(r, col)].TryGetTextByName($"Number_{number}", out Text ts))
                ts.text = "";

        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;
        for (int r = startRow; r < startRow + 3; r++)
            for (int c = startCol; c < startCol + 3; c++)
                if (dict[new Tuple<int, int>(r, c)].TryGetTextByName($"Number_{number}", out Text ts))
                    ts.text = "";
    }
}
