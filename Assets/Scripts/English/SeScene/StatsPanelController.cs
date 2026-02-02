using UnityEngine;
using UnityEngine.UI;

public class StatsPanelController : MonoBehaviour
{
    public enum Difficulty { Beginner = 0, Easy = 1, Medium = 2, Hard = 3, Expert = 4, Extreme = 5 }

    [Header("Values")]
    [SerializeField] private Text totalGamesValue;
    [SerializeField] private Text winsValue;
    [SerializeField] private Text winPercentValue;
    [SerializeField] private Text winsNoErrorsValue;
    [SerializeField] private Text bestTimeValue;
    [SerializeField] private Text avgTimeValue;

    [Header("Actions")]
    [SerializeField] private Button resetButton;

    private const string PP_LAST_TAB = "UI.Stats.LastTab";
    private const int DIFF_MIN = 0;
    private const int DIFF_MAX = 5;

    private Difficulty _current = Difficulty.Beginner;

    private void OnEnable()
    {
        if (resetButton)
        {
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(ResetStats);
        }

        int saved = PlayerPrefs.GetInt(PP_LAST_TAB, 0);
        SelectByIndex(saved);
    }

    public void SelectByIndex(int index)
    {
        index = Mathf.Clamp(index, DIFF_MIN, DIFF_MAX);
        _current = (Difficulty)index;

        PlayerPrefs.SetInt(PP_LAST_TAB, index);
        PlayerPrefs.Save();

        int started = PlayerPrefs.GetInt(Key(index, "Started"), 0);
        int wins = PlayerPrefs.GetInt(Key(index, "Wins"), 0);
        int winsNoErr = PlayerPrefs.GetInt(Key(index, "WinsNoErrors"), 0);

        float bestSec = PlayerPrefs.GetFloat(Key(index, "BestTimeSec"), 0f);
        float totalWinSec = PlayerPrefs.GetFloat(Key(index, "TotalWinTimeSec"), 0f);

        if (totalGamesValue) totalGamesValue.text = started.ToString();
        if (winsValue) winsValue.text = wins.ToString();

        int percent = (started > 0) ? Mathf.RoundToInt((wins * 100f) / started) : 0;
        if (winPercentValue) winPercentValue.text = percent + "%";

        if (winsNoErrorsValue) winsNoErrorsValue.text = winsNoErr.ToString();
        if (bestTimeValue) bestTimeValue.text = (bestSec > 0f) ? FormatMMSS(bestSec) : "--:--";

        if (avgTimeValue)
            avgTimeValue.text = (wins > 0) ? FormatMMSS(totalWinSec / wins) : "--:--";
    }

    public void ResetStats()
    {
        for (int d = DIFF_MIN; d <= DIFF_MAX; d++)
        {
            PlayerPrefs.DeleteKey(Key(d, "Started"));
            PlayerPrefs.DeleteKey(Key(d, "Wins"));
            PlayerPrefs.DeleteKey(Key(d, "WinsNoErrors"));
            PlayerPrefs.DeleteKey(Key(d, "BestTimeSec"));
            PlayerPrefs.DeleteKey(Key(d, "TotalWinTimeSec"));
        }

        PlayerPrefs.Save();
        SelectByIndex((int)_current);
    }

    private static string Key(int d, string k) => $"Stats.{d}.{k}";

    private static string FormatMMSS(float sec)
    {
        int s = Mathf.Max(0, Mathf.RoundToInt(sec));
        return $"{s / 60:00}:{s % 60:00}";
    }
}