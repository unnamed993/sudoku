using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class DailyBottomUIController : MonoBehaviour
{
    public CalendarGenerator calendar;

    [Header("Info Panel")]
    public Text dateText;
    public Text difficultyText;
    public Text timeText;

    [Header("Play Button")]
    public GameObject playButtonRoot;
    public Text playButtonText;

    private static readonly CultureInfo Ru = new CultureInfo("ru-RU");

    void OnEnable()
    {
        if (calendar != null)
            calendar.SelectedDayChanged += OnSelectedDayChanged;
    }

    void OnDisable()
    {
        if (calendar != null)
            calendar.SelectedDayChanged -= OnSelectedDayChanged;
    }

    private void OnSelectedDayChanged(DateTime date, CalendarGenerator.DayState state)
    {
        string key = date.ToString("yyyyMMdd");

        if (dateText != null)
            dateText.text = date.ToString("d MMMM yyyy 'г.'", Ru);

        int diff = PlayerPrefs.GetInt($"Daily_{key}_DailyDiff", PlayerPrefs.GetInt($"Daily_{key}_DailyDiff", -1));
        diff = PlayerPrefs.GetInt($"Daily_{key}_DailyDiff", PlayerPrefs.GetInt($"Daily_{key}_DailyDiff", -1));
 
        if (difficultyText != null)
            difficultyText.text = DiffToText(diff);

        float t = PlayerPrefs.GetFloat($"Daily_{key}_ElapsedTime", 0f);
        if (timeText != null)
            timeText.text = TimeSpan.FromSeconds(t).ToString(@"mm\:ss");

        if (state == CalendarGenerator.DayState.Completed)
        {
            if (playButtonRoot != null) playButtonRoot.SetActive(false);
        }
        else
        {
            if (playButtonRoot != null) playButtonRoot.SetActive(true);
            if (playButtonText != null)
                playButtonText.text = (state == CalendarGenerator.DayState.Started) ? "Продолжить" : "Начать игру";
        }
    }

    private string DiffToText(int diff)
    {
        // diff: 0/1/2 у вас (31/39/else) -> Beginner/Easy/Medium? вы сейчас маппите на 1..3
        // Для UI календаря достаточно простых названий:
        return diff switch
        {
            0 => "Лёгкий",
            1 => "Средний",
            2 => "Сложный",
            _ => "—"
        };
    }
}
