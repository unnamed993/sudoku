using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnglishMain : MonoBehaviour
{
    public Button BeginerButton;
    public Button EasyButton;
    public Button MiddleButton;
    public Button HardButton;
    public Button ExpertButton;
    public Button ExtrimalButton;

    void Start()
    {
        if (BeginerButton) BeginerButton.onClick.AddListener(() => AudioManager.Instance.Play("switchsound1"));
        if (EasyButton) EasyButton.onClick.AddListener(() => AudioManager.Instance.Play("switchsound1"));
        if (MiddleButton) MiddleButton.onClick.AddListener(() => AudioManager.Instance.Play("switchsound1"));
        if (HardButton) HardButton.onClick.AddListener(() => AudioManager.Instance.Play("switchsound1"));
        if (ExpertButton) ExpertButton.onClick.AddListener(() => AudioManager.Instance.Play("switchsound1"));
        if (ExtrimalButton) ExtrimalButton.onClick.AddListener(() => AudioManager.Instance.Play("switchsound1"));
    }

    public void ClickOn_Beginer() { LoadLevel(0); }
    public void ClickOn_Easy() { LoadLevel(1); }
    public void ClickOn_Middle() { LoadLevel(2); }
    public void ClickOn_Hard() { LoadLevel(3); }
    public void ClickOn_Expert() { LoadLevel(4); }
    public void ClickOn_Extrimal() { LoadLevel(5); }

    void LoadLevel(int difficulty)
    {
        EnglishGameSettings.EasyMiddleHard_Number = difficulty;
        PlayerPrefs.SetInt("SavedLevel", difficulty);

        ClearNormalSave();

        SceneManager.LoadScene("EnglishGame");
    }
    void ClearNormalSave()
    {
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
            {
                PlayerPrefs.DeleteKey($"Cell_{r}_{c}");
                PlayerPrefs.DeleteKey($"InitialCell_{r}_{c}");
                PlayerPrefs.DeleteKey($"Solution_{r}_{c}");
            }

        PlayerPrefs.DeleteKey("ErrorCount");
        PlayerPrefs.DeleteKey("ElapsedTime");
        PlayerPrefs.DeleteKey("StatsStarted");
        PlayerPrefs.DeleteKey("DailyDiff");

        PlayerPrefs.Save();
    }
}
