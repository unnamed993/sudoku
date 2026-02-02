using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    public Button TimerPause;
    public GameObject PausePanel;
    public GameObject PauseBack;
    public Button PauseRepeat;
    public Button ContinueButton;
    public Image TimerPauseImage;
    public Sprite PauseSprite;
    public Sprite PlaySprite;

    public Text PauseTimerText;
    public Text PauseErrorText;
    public Text PauseLevelText;

    public EnglishGame englishGame;

    void Start()
    {
        FindFirstObjectByType<EnglishGame>();
    }

    void Update()
    {
            PauseTimerText.text = englishGame.TimerText.text;
            PauseErrorText.text = englishGame.ErrorText.text;
            PauseLevelText.text = englishGame.LevelsText.text;
    }

    public void OnPauseClicked()
    {
        Time.timeScale = 0;
        TimerPauseImage.sprite = PlaySprite;

        PausePanel.SetActive(true);
        PauseBack.SetActive(true);
    }

    public void OnContinueClicked()
    {
        Time.timeScale = 1;
        TimerPauseImage.sprite = PauseSprite;

        PausePanel.SetActive(false);
        PauseBack.SetActive(false);
        AudioManager.Instance.Play("switchsound1");
    }

    public void OnPauseRepeatClicked()
    {
        ResetGameState();
        englishGame.LoadInitialState();
        englishGame.RestoreInitialSudoku();

        englishGame.TimerText.text = "00:00";
        englishGame.ErrorText.text = "0/3";
        PauseLevelText.text = englishGame.LevelsText.text;

        Time.timeScale = 1;
        TimerPauseImage.sprite = PauseSprite;

        PauseBack.SetActive(false);
        AudioManager.Instance.Play("switchsound1");
    }

    private void ResetGameState()
    {
        englishGame.ResetTimer();
        englishGame.ResetErrors();
    }
}
