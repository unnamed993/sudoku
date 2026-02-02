using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TabsScrollController : MonoBehaviour
{
    [Serializable]
    public class Tab
    {
        public RectTransform tabRect;
        public Button button;
        public GameObject underline;
        public int difficultyIndex;
    }

    [Header("Refs")]
    [SerializeField] private ScrollRect scroll;
    [SerializeField] private RectTransform content;

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
            int idx = i;
            if (tabs[idx].button == null) continue;

            tabs[idx].button.onClick.RemoveAllListeners();
            tabs[idx].button.onClick.AddListener(() =>
            {
                SelectByDifficulty(tabs[idx].difficultyIndex, invokeEvent: true);
            });
        }
    }
    public void SelectByDifficulty(int difficultyIndex, bool invokeEvent)
    {
        SelectByIndex(difficultyIndex, invokeEvent);
    }

    private void SelectByIndex(int difficultyIndex, bool invokeEvent)
    {
        _current = Mathf.Clamp(difficultyIndex, 0, 999);

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

        ScrollToCurrent();

        if (invokeEvent)
            onTabSelected?.Invoke(_current);
    }
    private void ScrollToCurrent()
    {
        if (!scroll || !content || !scroll.viewport) return;

        RectTransform target = null;
        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i].difficultyIndex == _current)
            {
                target = tabs[i].tabRect;
                break;
            }
        }
        if (!target) return;

        Canvas.ForceUpdateCanvases();

        var viewport = (RectTransform)scroll.viewport;

        float contentWidth = content.rect.width;
        float viewportWidth = viewport.rect.width;
        if (contentWidth <= viewportWidth) return;

        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Vector3 worldCenter = (corners[0] + corners[2]) * 0.5f;

        Vector3 localCenter = content.InverseTransformPoint(worldCenter);
        float tabCenterX = localCenter.x + contentWidth * 0.5f;

        float desired = tabCenterX - viewportWidth * 0.5f;
        float max = contentWidth - viewportWidth;

        scroll.horizontalNormalizedPosition = Mathf.Clamp01(desired / max);
    }
}