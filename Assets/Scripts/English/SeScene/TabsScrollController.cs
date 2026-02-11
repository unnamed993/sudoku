using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TabsScrollController : MonoBehaviour
{
    [Serializable]
    public class Tab
    {
        public Button button;
        public GameObject underline;
        public int difficultyIndex;
    }

    [Header("Tabs")]
    [SerializeField] private Tab[] tabs;

    [Header("Events")]
    public UnityEvent<int> onTabSelected;

    private int _current = 0;
    private const string PP_LAST_TAB = "UI.Stats.LastTab";

    private void OnEnable()
    {
        BindButtons();

        int saved = PlayerPrefs.GetInt(PP_LAST_TAB, 0);
        SelectByIndex(saved, invokeEvent: true);
    }

    private void BindButtons()
    {
        if (tabs == null) return;

        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i].button == null) continue;

            int diff = tabs[i].difficultyIndex;

            tabs[i].button.onClick.RemoveAllListeners();
            tabs[i].button.onClick.AddListener(() =>
            {
                SelectByIndex(diff, invokeEvent: true);
            });
        }
    }

    public void SelectByDifficulty(int difficultyIndex, bool invokeEvent)
    {
        SelectByIndex(difficultyIndex, invokeEvent);
    }

    private void SelectByIndex(int difficultyIndex, bool invokeEvent)
    {
        _current = difficultyIndex;

        if (tabs != null)
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                if (tabs[i].underline)
                    tabs[i].underline.SetActive(tabs[i].difficultyIndex == _current);
            }
        }

        PlayerPrefs.SetInt(PP_LAST_TAB, _current);
        PlayerPrefs.Save();

        if (invokeEvent)
            onTabSelected?.Invoke(_current);
    }
}