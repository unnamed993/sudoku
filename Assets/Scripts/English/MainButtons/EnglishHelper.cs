using System;
using UnityEngine;
using UnityEngine.UI;

public class EnglishHelper : MonoBehaviour
{
    public EnglishGame englishGame;
    public Button helperButton;

    private int helperUses = 0;

    void Start()
    {
        helperButton.onClick.AddListener(OnHelperClick);
    }

    private void OnHelperClick()
    {
        var cell = englishGame.CurrentHoveredField;
        if (cell == null) return;

        if (helperUses == 0)
        {
            Debug.Log("Free hint");
            englishGame.GiveHint();
            helperUses++;
        }
        else
        {
            Debug.Log("Showing adЕ");
            ShowAd(success =>
            {
                Debug.Log($"Ad finished: {success}");
                if (success)
                {
                    englishGame.GiveHint();
                    helperUses++;
                }
                else
                {
                    Debug.Log("–еклама не показана, подсказка недоступна");
                }
            });
        }
    }

    // заглушка дл€ рекламы
    private void ShowAd(Action<bool> onComplete)
    {
        // TODO: здесь будет вызов SDK
        onComplete?.Invoke(true);
    }
}
