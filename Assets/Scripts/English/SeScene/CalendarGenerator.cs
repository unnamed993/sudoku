using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class CalendarGenerator : MonoBehaviour
{
    public GameObject[] dayButtons;
    public Button nextButton;
    public Button backButton;
    public Text monthLabel;

    public int currentMonth;
    public int currentYear;

    public Color normalTextColor = Color.black;
    public Color selectedTextColor = Color.white;
    public Color futureTextColor = new Color(0.75f, 0.75f, 0.75f, 1f);

    private readonly Dictionary<int, int> savedDayByMonth = new Dictionary<int, int>();
    private int lastViewedMonthKey = -1;
    private int selectedDay = -1;

    void Start()
    {
        DateTime today = DateTime.Today;

        currentYear = today.Year;
        currentMonth = today.Month;

        int key = MonthKey(currentYear, currentMonth);
        savedDayByMonth[key] = today.Day;
        lastViewedMonthKey = key;

        if (nextButton != null) nextButton.onClick.AddListener(NextMonth);
        if (backButton != null) backButton.onClick.AddListener(PreviousMonth);

        GenerateCalendar();
    }

    public void NextMonth()
    {
        lastViewedMonthKey = MonthKey(currentYear, currentMonth);

        if (currentMonth == 12) { currentMonth = 1; currentYear++; }
        else currentMonth++;

        GenerateCalendar();
    }

    public void PreviousMonth()
    {
        lastViewedMonthKey = MonthKey(currentYear, currentMonth);

        if (currentMonth == 1) { currentMonth = 12; currentYear--; }
        else currentMonth--;

        GenerateCalendar();
    }

    int MonthKey(int year, int month) => year * 12 + month;

    bool IsFutureMonth(int year, int month)
    {
        var first = new DateTime(year, month, 1);
        return first.Date > DateTime.Today;
    }

    bool IsFutureDay(int year, int month, int day)
    {
        var d = new DateTime(year, month, day);
        return d.Date > DateTime.Today;
    }
    void PruneSavedDays(int currentKey, int neighborKey)
    {
        var keysToRemove = new List<int>();

        foreach (var k in savedDayByMonth.Keys)
            if (k != currentKey && k != neighborKey)
                keysToRemove.Add(k);

        for (int i = 0; i < keysToRemove.Count; i++)
            savedDayByMonth.Remove(keysToRemove[i]);
    }
    void GenerateCalendar()
    {
        DateTime firstDay = new DateTime(currentYear, currentMonth, 1);
        int startOffset = ((int)firstDay.DayOfWeek + 6) % 7;
        int daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);

        if (monthLabel != null)
        {
            string m = firstDay.ToString("MMMM yyyy");
            monthLabel.text = char.ToUpper(m[0]) + m.Substring(1);
        }

        int ny = currentYear;
        int nm = currentMonth + 1;
        if (nm == 13) { nm = 1; ny++; }

        if (nextButton != null)
        {
            bool canGoNext = !IsFutureMonth(ny, nm);
            nextButton.gameObject.SetActive(canGoNext);
        }

        int currentKey = MonthKey(currentYear, currentMonth);

        if (lastViewedMonthKey != -1 && Mathf.Abs(currentKey - lastViewedMonthKey) > 1)
        {
            savedDayByMonth.Clear();
        }
        else if (lastViewedMonthKey != -1)
        {
            PruneSavedDays(currentKey, lastViewedMonthKey);
        }

        if (!savedDayByMonth.TryGetValue(currentKey, out selectedDay))
        {
            DateTime today = DateTime.Today;
            if (today.Year == currentYear && today.Month == currentMonth)
                selectedDay = today.Day;
            else
                selectedDay = daysInMonth;
        }

        if (selectedDay > daysInMonth) selectedDay = daysInMonth;

        if (IsFutureDay(currentYear, currentMonth, selectedDay))
        {
            DateTime today = DateTime.Today;
            if (today.Year == currentYear && today.Month == currentMonth)
                selectedDay = today.Day;
            else
                selectedDay = daysInMonth;
        }

        for (int i = 0; i < dayButtons.Length; i++)
        {
            GameObject go = dayButtons[i];
            if (go == null) continue;

            go.SetActive(true);

            var btn = go.GetComponent<Button>();
            var txt = go.GetComponentInChildren<Text>(true);

            Transform bgT = go.transform.Find("SelectionBG");
            Image bg = bgT ? bgT.GetComponent<Image>() : null;

            Transform startedT = go.transform.Find("StartedDot");
            GameObject startedDot = startedT ? startedT.gameObject : null;

            Transform completedT = go.transform.Find("CompletedIcon");
            GameObject completedIcon = completedT ? completedT.gameObject : null;

            int dayNumber = i - startOffset + 1;
            bool isDay = dayNumber >= 1 && dayNumber <= daysInMonth;

            if (!isDay)
            {
                if (txt != null) txt.text = "";
                if (bgT != null) bgT.gameObject.SetActive(false);
                if (startedDot != null) startedDot.SetActive(false);
                if (completedIcon != null) completedIcon.SetActive(false);

                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.interactable = true;
                    btn.enabled = false;
                }
                continue;
            }

            bool isFuture = IsFutureDay(currentYear, currentMonth, dayNumber);
            bool isSelected = (dayNumber == selectedDay) && !isFuture;

            if (txt != null)
            {
                txt.text = dayNumber.ToString();
                txt.color = isFuture ? futureTextColor : (isSelected ? selectedTextColor : normalTextColor);
            }

            if (bg != null) bgT.gameObject.SetActive(isSelected);

            // маркеры Started / Completed
            if (!isFuture)
            {
                string dateKey = new DateTime(currentYear, currentMonth, dayNumber).ToString("yyyyMMdd");

                bool started = PlayerPrefs.GetInt($"DailyStarted_{dateKey}", 0) == 1;
                bool completed = PlayerPrefs.GetInt($"DailyCompleted_{dateKey}", 0) == 1;

                if (startedDot != null) startedDot.SetActive(started && !completed);
                if (completedIcon != null) completedIcon.SetActive(completed);
            }
            else
            {
                if (startedDot != null) startedDot.SetActive(false);
                if (completedIcon != null) completedIcon.SetActive(false);
            }

            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();

                if (!isFuture)
                {
                    int capturedDay = dayNumber;
                    int capturedKey = currentKey;

                    btn.onClick.AddListener(() =>
                    {
                        selectedDay = capturedDay;
                        savedDayByMonth[capturedKey] = capturedDay;
                        RefreshSelectionVisual();
                    });
                }

                btn.interactable = true;
                btn.enabled = !isFuture;
            }
        }

        RefreshSelectionVisual();
    }

    void RefreshSelectionVisual()
    {
        DateTime firstDay = new DateTime(currentYear, currentMonth, 1);
        int startOffset = ((int)firstDay.DayOfWeek + 6) % 7;
        int daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);

        for (int i = 0; i < dayButtons.Length; i++)
        {
            GameObject go = dayButtons[i];
            if (go == null) continue;

            var txt = go.GetComponentInChildren<Text>(true);

            Transform bgT = go.transform.Find("SelectionBG");
            Image bg = bgT ? bgT.GetComponent<Image>() : null;

            int dayNumber = i - startOffset + 1;
            bool isDay = dayNumber >= 1 && dayNumber <= daysInMonth;

            if (!isDay)
            {
                if (bgT != null) bgT.gameObject.SetActive(false);
                continue;
            }

            bool isFuture = IsFutureDay(currentYear, currentMonth, dayNumber);
            bool isSelected = (dayNumber == selectedDay) && !isFuture;

            if (bg != null) bgT.gameObject.SetActive(isSelected);

            if (txt != null)
                txt.color = isFuture ? futureTextColor : (isSelected ? selectedTextColor : normalTextColor);
        }
    }

    public DateTime GetSelectedDate()
    {
        return new DateTime(currentYear, currentMonth, selectedDay);
    }
}