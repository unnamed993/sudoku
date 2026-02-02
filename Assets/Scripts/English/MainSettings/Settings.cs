using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    public GameObject SettingsPanel;
    public Toggle selectNumToggle;
    public Toggle BackLight;
    public Toggle errorsLimitToggle; 
    public Toggle TimerTog;
    public Toggle SoundsEffectsToggle;
    public Toggle DelNotesToggle;
    public Toggle ShowInfToggle;
    public Toggle HowNumbersToggle;
    public GameObject LangPanel;
    public GameObject SettingsWindow;
    public static bool highlightSimilarNumbers = false;
    public static bool errorsLimitEnabled = true;
    public static bool autoDeleteNotesEnabled = false;

    void Start()
    {
        SoundsEffectsToggle.isOn = PlayerPrefs.GetInt("SoundsEffectsToggle", 1) == 1;
        AudioManager.Instance.SetSoundEnabled(SoundsEffectsToggle.isOn);
        SoundsEffectsToggle.onValueChanged.AddListener(OnSoundsEffectsChanged);

        selectNumToggle.isOn = PlayerPrefs.GetInt("selectNumToggle", 1) == 1;
        selectNumToggle.onValueChanged.AddListener(OnSelectNumToggleChanged);

        BackLight.isOn = PlayerPrefs.GetInt("BackLight", 1) == 1;
        BackLight.onValueChanged.AddListener(OnBackLightChanged);

        errorsLimitToggle.isOn = PlayerPrefs.GetInt("ErrorsLimitEnabled", 1) == 1;
        errorsLimitEnabled = errorsLimitToggle.isOn;
        errorsLimitToggle.onValueChanged.AddListener(OnErrorsLimitToggleChanged);

        TimerTog.isOn = PlayerPrefs.GetInt("TimerTog", 1) == 1;
        TimerTog.onValueChanged.AddListener(OnTimerTogChanged);

        DelNotesToggle.isOn = PlayerPrefs.GetInt("DelNotesToggle", 1) == 1;
        DelNotesToggle.onValueChanged.AddListener(OnDelNotesChanged);

        ShowInfToggle.isOn = PlayerPrefs.GetInt("ShowInfToggle", 1) == 1;
        ShowInfToggle.onValueChanged.AddListener(OnShowInfChanged);

        HowNumbersToggle.isOn = PlayerPrefs.GetInt("HowNumbersToggle", 1) == 1;
        HowNumbersToggle.onValueChanged.AddListener(OnHowNumbersChanged);
    }

    private void OnSoundsEffectsChanged(bool isOn)
    {
        if (isOn)
        {
            AudioManager.Instance.SetSoundEnabled(true);
            AudioManager.Instance.Play("switchsound2");
        }
        else
        {
            AudioManager.Instance.Play("switchsound1");
            AudioManager.Instance.SetSoundEnabled(false);
        }
        PlayerPrefs.SetInt("SoundsEffectsToggle", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnSelectNumToggleChanged(bool isOn)
    {
        highlightSimilarNumbers = isOn;
        PlayerPrefs.SetInt("selectNumToggle", isOn ? 1 : 0);
        PlayerPrefs.Save();
        AudioManager.Instance.Play(isOn ? "switchsound2" : "switchsound1");
    }

    private void OnBackLightChanged(bool isOn)
    {
        PlayerPrefs.SetInt("BackLight", isOn ? 1 : 0);
        PlayerPrefs.Save();
        AudioManager.Instance.Play(isOn ? "switchsound2" : "switchsound1");

        var highlighter = FindFirstObjectByType<RowColHighlighter>();
        if (highlighter != null)
            highlighter.backlightEnabled = isOn;
    }

    private void OnDelNotesChanged(bool isOn)
    {
        autoDeleteNotesEnabled = isOn;
        PlayerPrefs.SetInt("DelNotesToggle", isOn ? 1 : 0);
        PlayerPrefs.Save();
        AudioManager.Instance.Play(isOn ? "switchsound2" : "switchsound1");
    }

    private void OnShowInfChanged(bool isOn)
    {
        PlayerPrefs.SetInt("ShowInfToggle", isOn ? 1 : 0);
        PlayerPrefs.Save();
        AudioManager.Instance.Play(isOn ? "switchsound2" : "switchsound1");
    }

    private void OnErrorsLimitToggleChanged(bool isOn)
    {
        errorsLimitEnabled = isOn;
        PlayerPrefs.SetInt("ErrorsLimitEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();
        var game = FindFirstObjectByType<EnglishGame>();
        if (game != null)
            game.OnErrorsLimitToggled(isOn);
        AudioManager.Instance.Play(isOn ? "switchsound2" : "switchsound1");
    }

    private void OnTimerTogChanged(bool isOn)
    {
        PlayerPrefs.SetInt("TimerTog", isOn ? 1 : 0);
        PlayerPrefs.Save();
        AudioManager.Instance.Play(isOn ? "switchsound2" : "switchsound1");

        var game = FindFirstObjectByType<EnglishGame>();
        if (game != null)
            game.timerEnabled = isOn;
    }

    private void OnHowNumbersChanged(bool isOn)
    {
        PlayerPrefs.SetInt("HowNumbersToggle", isOn ? 1 : 0);
        PlayerPrefs.Save();
        AudioManager.Instance.Play(isOn ? "switchsound2" : "switchsound1");

        var digitsCounter = FindFirstObjectByType<RemainingDigitsCounter>();
        if (digitsCounter != null)
            digitsCounter.SetCountsVisible(isOn);
    }

    public void OnLanguageButtonClicked()
    {
        SettingsWindow.SetActive(false);
        LangPanel.SetActive(true);
        AudioManager.Instance.Play("switchsound2");
    }

    public void OnHowPlayClicked()
    {
        AudioManager.Instance.Play("switchsound1");
    }

    public void OnAdverClicked()
    {
        AudioManager.Instance.Play("switchsound1");
    }

    public void OnShareClicked()
    {
        AudioManager.Instance.Play("switchsound1");
    }

    public void OnReviewClicked()
    {
        AudioManager.Instance.Play("switchsound1");
    }

    public void OnRateClicked()
    {
        AudioManager.Instance.Play("switchsound1");
    }

    public void OnBackButtonClicked()
    {
        SettingsPanel.SetActive(false);
        Time.timeScale = 1;
        AudioManager.Instance.Play("switchsound1");

        var game = FindFirstObjectByType<EnglishGame>();
        if (game != null)
            game.RemoveHighlight();
    }
}