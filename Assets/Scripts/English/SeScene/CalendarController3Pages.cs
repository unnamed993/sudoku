using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CalendarController3Pages : MonoBehaviour
{
    [Header("Month bar")]
    public Button nextButton;
    public Button backButton;
    public Text monthLabel;

    [Header("Pages (DaysGrid objects with CalendarPageView)")]
    public CalendarPageView pagePrev;
    public CalendarPageView pageCur;
    public CalendarPageView pageNext;

    [Header("Colors")]
    public Color normalTextColor = Color.black;
    public Color selectedTextColor = Color.white;
    public Color futureTextColor = new Color(0.75f, 0.75f, 0.75f, 1f);

    public enum DayState { NotStarted, Started, Completed }
    public event Action<DateTime, DayState> SelectedDayChanged;

    public int currentYear;
    public int currentMonth;

    readonly Dictionary<int, int> savedDayByMonth = new();
    int selectedDay = 1;

    void Start()
    {
        var today = DateTime.Today;
        currentYear = today.Year;
        currentMonth = today.Month;

        savedDayByMonth[MonthKey(currentYear, currentMonth)] = today.Day;

        if (nextButton) nextButton.onClick.AddListener(() => MoveMonth(+1));
        if (backButton) backButton.onClick.AddListener(() => MoveMonth(-1));

        RedrawAll();
    }
    public bool CanMove(int dir)
    {
        if (dir < 0) return true;

        int ny = currentYear, nm = currentMonth;
        AddMonths(ref ny, ref nm, +1);

        var now = DateTime.Today;
        return new DateTime(ny, nm, 1) <= new DateTime(now.Year, now.Month, 1);
    }
    public void MoveMonth(int delta)
    {
        if (!CanMove(delta)) return;

        AddMonths(ref currentYear, ref currentMonth, delta);
        RedrawAll();
    }

    public DateTime GetSelectedDate() => new DateTime(currentYear, currentMonth, Mathf.Clamp(selectedDay, 1, DateTime.DaysInMonth(currentYear, currentMonth)));

    int MonthKey(int year, int month) => year * 12 + month;

    static void AddMonths(ref int year, ref int month, int delta)
    {
        var d = new DateTime(year, month, 1).AddMonths(delta);
        year = d.Year; month = d.Month;
    }

    bool IsFutureMonth(int year, int month) => new DateTime(year, month, 1).Date > DateTime.Today;
    bool IsFutureDay(DateTime d) => d.Date > DateTime.Today;

    (bool started, bool completed) GetProgress(DateTime d)
    {
        string key = d.ToString("yyyyMMdd");
        bool completed = PlayerPrefs.GetInt($"DailyCompleted_{key}", 0) == 1;

        bool startedFlag = PlayerPrefs.GetInt($"DailyStarted_{key}", 0) == 1;
        bool hasSave = PlayerPrefs.HasKey($"Daily_{key}_Cell_0_0");
        bool started = (startedFlag || hasSave);

        return (started, completed);
    }
    DayState GetDayState(DateTime d)
    {
        string key = d.ToString("yyyyMMdd");
        if (PlayerPrefs.GetInt($"DailyCompleted_{key}", 0) == 1) return DayState.Completed;

        bool startedFlag = PlayerPrefs.GetInt($"DailyStarted_{key}", 0) == 1;
        bool hasSave = PlayerPrefs.HasKey($"Daily_{key}_Cell_0_0");
        return (startedFlag || hasSave) ? DayState.Started : DayState.NotStarted;
    }
    void RedrawAll()
    {
        var first = new DateTime(currentYear, currentMonth, 1);

        if (monthLabel)
        {
            string m = first.ToString("MMMM yyyy");
            monthLabel.text = char.ToUpper(m[0]) + m.Substring(1);
        }

        int ny = currentYear, nm = currentMonth;
        AddMonths(ref ny, ref nm, +1);
        if (nextButton) nextButton.gameObject.SetActive(!IsFutureMonth(ny, nm));

        int key = MonthKey(currentYear, currentMonth);
        if (!savedDayByMonth.TryGetValue(key, out selectedDay))
        {
            var today = DateTime.Today;
            selectedDay = (today.Year == currentYear && today.Month == currentMonth)
                ? today.Day
                : DateTime.DaysInMonth(currentYear, currentMonth);

            savedDayByMonth[key] = selectedDay;
        }

        if (IsFutureDay(new DateTime(currentYear, currentMonth, selectedDay)))
        {
            var today = DateTime.Today;
            selectedDay = (today.Year == currentYear && today.Month == currentMonth)
                ? today.Day
                : DateTime.DaysInMonth(currentYear, currentMonth);

            savedDayByMonth[key] = selectedDay;
        }

        int py = currentYear, pm = currentMonth; AddMonths(ref py, ref pm, -1);
        int cy = currentYear, cm = currentMonth;
        int ny2 = currentYear, nm2 = currentMonth; AddMonths(ref ny2, ref nm2, +1);

        if (pagePrev)
            pagePrev.Render(py, pm, -1, IsFutureDay, GetProgress, null, normalTextColor, selectedTextColor, futureTextColor, false);

        if (pageCur)
            pageCur.Render(cy, cm, selectedDay, IsFutureDay, GetProgress, OnDayClicked, normalTextColor, selectedTextColor, futureTextColor, true);

        if (pageNext)
            pageNext.Render(ny2, nm2, -1, IsFutureDay, GetProgress, null, normalTextColor, selectedTextColor, futureTextColor, false);

        NotifySelectedDayChanged();
    }
    void OnDayClicked(int day)
    {
        selectedDay = day;
        savedDayByMonth[MonthKey(currentYear, currentMonth)] = day;
        RedrawAll();
    }
    void NotifySelectedDayChanged()
    {
        var d = GetSelectedDate();
        SelectedDayChanged?.Invoke(d, GetDayState(d));
    }
}
