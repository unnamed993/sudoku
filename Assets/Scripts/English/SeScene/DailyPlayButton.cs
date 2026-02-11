using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DailyPlayButton : MonoBehaviour
{
    private const string PP_MODE = "Game.Mode";
    private const string PP_DAILY_DATE = "Daily.SelectedDate";

    public CalendarController3Pages calendar;

    public void OnPlayClicked()
    {
        DateTime date = calendar.GetSelectedDate();
        string yyyymmdd = date.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

        PlayerPrefs.SetString(PP_MODE, "daily");
        PlayerPrefs.SetString(PP_DAILY_DATE, yyyymmdd);
        PlayerPrefs.Save();

        SceneManager.LoadScene("EnglishGame");
    }
}
