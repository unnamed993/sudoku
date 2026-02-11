using System;
using UnityEngine;

public class CalendarPageView : MonoBehaviour
{
    [Header("Prefab")]
    public DayCellView dayCellPrefab;
    public int cellCount = 42;

    DayCellView[] _cells;

    void Awake()
    {
        BuildIfNeeded();
    }

    public void BuildIfNeeded()
    {
        if (_cells != null && _cells.Length == cellCount) return;

        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        _cells = new DayCellView[cellCount];

        for (int i = 0; i < cellCount; i++)
        {
            var cell = Instantiate(dayCellPrefab, transform);
            cell.name = $"DayCell_{i:00}";
            _cells[i] = cell;
        }
    }

    public void Render(
        int year, int month, int selectedDay,
        Func<DateTime, bool> isFutureDay,
        Func<DateTime, (bool started, bool completed)> getProgress,
        Action<int> onDayClicked,
        Color normalTextColor, Color selectedTextColor, Color futureTextColor,
        bool clickable)
    {
        BuildIfNeeded();

        DateTime firstDay = new DateTime(year, month, 1);
        int startOffset = ((int)firstDay.DayOfWeek + 6) % 7;
        int daysInMonth = DateTime.DaysInMonth(year, month);

        for (int i = 0; i < _cells.Length; i++)
        {
            int dayNumber = i - startOffset + 1;
            bool isDay = dayNumber >= 1 && dayNumber <= daysInMonth;

            if (!isDay)
            {
                _cells[i].Bind(0, false, false, false, false, false,
                    normalTextColor, selectedTextColor, futureTextColor, null);
                continue;
            }

            var date = new DateTime(year, month, dayNumber);
            bool isFuture = isFutureDay(date);
            bool isSelected = (dayNumber == selectedDay) && !isFuture;

            var (started, completed) = (!isFuture) ? getProgress(date) : (false, false);

            Action click = null;
            if (clickable && !isFuture && onDayClicked != null)
            {
                int capturedDay = dayNumber;
                click = () => onDayClicked(capturedDay);
            }

            _cells[i].Bind(dayNumber, true, isFuture, isSelected, started, completed,
                normalTextColor, selectedTextColor, futureTextColor, click);
        }
    }
}
