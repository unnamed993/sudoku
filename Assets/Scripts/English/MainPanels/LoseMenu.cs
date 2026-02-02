using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class LoseMenu : MonoBehaviour
{
    public GameObject levelSelectPanel;
    public GameObject LoseBack;
    public Button secondChanceButton;
    public Button RepeatButton;
    public Button newGameButton;
    public EventSystem eventSystem;
    public Text TimerText;
    public Text ErrorText;

    private EnglishGame englishGameScript;

    void Start()
    {
        englishGameScript = eventSystem.GetComponent<EnglishGame>();

        newGameButton.onClick.AddListener(OnNewGameClicked);
        secondChanceButton.onClick.AddListener(OnSecondChanceClicked);
        RepeatButton.onClick.AddListener(OnRepeatClicked);
    }

    void OnSecondChanceClicked()
    {
        Debug.Log("¬ы посмотрели рекламу и получили второй шанс!");
        // ћожно добавить логику дл€ активации второго шанса, если необходимо
        LoseBack.SetActive(false);
        englishGameScript.ContinueAfterSecondChance();
    }

    public void OnRepeatClicked()
    {
        ResetGameState();
        englishGameScript.LoadInitialState();
        englishGameScript.RestoreInitialSudoku();

        TimerText.text = "00:00";
        ErrorText.text = "0/3";

        LoseBack.SetActive(false);
    }
    private void ResetGameState()
    {
        englishGameScript.ResetTimer();
        englishGameScript.ResetErrors();
    }

    void OnNewGameClicked()
    {
        levelSelectPanel.SetActive(true);
    }
}
