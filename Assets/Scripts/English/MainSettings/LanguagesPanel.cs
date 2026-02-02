using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

public class LanguagesPanel : MonoBehaviour
{
    public GameObject LangPanel;
    public GameObject SettingsWindow;
    public Text systemLangText;

    void Start()
    {
        UpdateSystemLangText();
    }

    // Обновление названия системного языка на кнопке "Автоматически"
    public void UpdateSystemLangText()
    {
        var sysLang = Application.systemLanguage;
        systemLangText.text = GetLangInfo(sysLang).displayName;
    }

    // Выбор языка вручную по коду (например, "ru", "en" и т.д.)
    public void SetLanguage(string localeCode)
    {
        var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        if (locale != null)
            LocalizationSettings.SelectedLocale = locale;
    }

    public void SetSystemLanguage()
    {
        var info = GetLangInfo(Application.systemLanguage);
        var locale = LocalizationSettings.AvailableLocales.GetLocale(info.localeCode);
        if (locale != null)
            LocalizationSettings.SelectedLocale = locale;
        else
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("en");
    }

    // Универсальный метод: и название, и код локали
    (string displayName, string localeCode) GetLangInfo(SystemLanguage lang)
    {
        switch (lang)
        {
            case SystemLanguage.Arabic: return ("العربية (الإمارات)", "ar-AE");
            case SystemLanguage.ChineseSimplified: return ("中文 (简体)", "zh");
            case SystemLanguage.ChineseTraditional: return ("中文 (繁體)", "zh-Hant");
            case SystemLanguage.Czech: return ("Čeština (Česko)", "cs");
            case SystemLanguage.Danish: return ("Dansk (Danmark)", "da");
            case SystemLanguage.Dutch: return ("Nederlands (Nederland)", "nl-NL");
            case SystemLanguage.English: return ("English (United States)", "en");
            case SystemLanguage.Finnish: return ("Suomi (Suomi)", "fi");
            case SystemLanguage.French: return ("Français (France)", "fr");
            case SystemLanguage.German: return ("Deutsch (Deutschland)", "de");
            case SystemLanguage.Greek: return ("Ελληνικά (Ελλάδα)", "el");
            case SystemLanguage.Hebrew: return ("עברית (ישראל)", "he");
            case SystemLanguage.Hindi: return ("हिन्दी (भारत)", "hi");
            case SystemLanguage.Indonesian: return ("Bahasa Indonesia (Indonesia)", "id");
            case SystemLanguage.Italian: return ("Italiano (Italia)", "it");
            case SystemLanguage.Japanese: return ("日本語 (日本)", "ja");
            case SystemLanguage.Korean: return ("한국어 (대한민국)", "ko");
            case SystemLanguage.Norwegian: return ("Norsk (Norge)", "no");
            case SystemLanguage.Polish: return ("Polski (Polska)", "pl");
            case SystemLanguage.Portuguese: return ("Português (Portugal)", "pt");
            case SystemLanguage.Russian: return ("Русский (Россия)", "ru");
            case SystemLanguage.Spanish: return ("Español (España)", "es");
            case SystemLanguage.Swedish: return ("Svenska (Sverige)", "sv");
            case SystemLanguage.Thai: return ("ไทย (ไทย)", "th");
            case SystemLanguage.Turkish: return ("Türkçe (Türkiye)", "tr");
            case SystemLanguage.Vietnamese: return ("Tiếng Việt (Việt Nam)", "vi");
            default: return (lang.ToString(), "en");
        }
    }

    public void OnBackLangClicked()
    {
        LangPanel.SetActive(false);
        SettingsWindow.SetActive(true);
        AudioManager.Instance.Play("switchsound1");
    }
}
