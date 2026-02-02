using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryOverlayController : MonoBehaviour
{
    [Header("Roots")]
    public GameObject normalRoot;
    public GameObject dailyRoot;

    [Header("Optional")]
    public GameObject levelSelectPanel;

    private bool _isDaily;

    void Awake()
    {
        if (normalRoot) normalRoot.SetActive(false);
        if (dailyRoot) dailyRoot.SetActive(false);
    }

    public void Show(bool isDaily)
    {
        _isDaily = isDaily;

        gameObject.SetActive(true);

        if (normalRoot) normalRoot.SetActive(!isDaily);
        if (dailyRoot) dailyRoot.SetActive(isDaily);

        Time.timeScale = 0f; // заморозка (можно убрать, если не нужно)
    }

    public void HideAll()
    {
        if (normalRoot) normalRoot.SetActive(false);
        if (dailyRoot) dailyRoot.SetActive(false);
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnNewGame()
    {
        HideAll();
        if (levelSelectPanel) levelSelectPanel.SetActive(true);
    }

    public void OnHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("EnglishSelect");
    }

    public void OnContinue()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt("UI.OpenDailyPanel", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("EnglishSelect");
    }
}