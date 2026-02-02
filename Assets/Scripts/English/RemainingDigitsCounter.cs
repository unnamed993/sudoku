using UnityEngine;
using UnityEngine.UI;

public class RemainingDigitsCounter : MonoBehaviour
{
    public Text[] countTexts;

    public void SetCountsVisible(bool visible)
    {
        foreach (var text in countTexts)
            text.gameObject.SetActive(visible);
    }

    public void UpdateCounts(int[,] grid)
    {
        int[] used = new int[9];
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
            {
                int val = grid[r, c];
                if (val >= 1 && val <= 9)
                    used[val - 1]++;
            }

        for (int i = 0; i < 9; i++)
        {
            int remain = 9 - used[i];
            countTexts[i].text = remain > 0 ? remain.ToString() : "";
        }
    }
}
