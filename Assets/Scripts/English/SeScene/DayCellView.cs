using UnityEngine;
using UnityEngine.UI;
using System;

public class DayCellView : MonoBehaviour
{
    [Header("Refs")]
    public Button button;
    public Text label;
    public GameObject selectionBG;
    public GameObject startedDot;
    public GameObject completedIcon;
    void Awake()
    {
        if (!button) button = GetComponent<Button>();
        if (!label) label = GetComponentInChildren<Text>(true);

        if (!selectionBG)
            selectionBG = transform.Find("SelectionBG")?.gameObject;

        if (!startedDot)
            startedDot = transform.Find("StartedDot")?.gameObject;

        if (!completedIcon)
            completedIcon = transform.Find("CompletedIcon")?.gameObject;
    }

    public void Bind(
        int dayNumber,
        bool isDay,
        bool isFuture,
        bool isSelected,
        bool started,
        bool completed,
        Color normalText,
        Color selectedText,
        Color futureText,
        Action onClick)
    {
        if (!isDay)
        {
            label.text = "";
            selectionBG.SetActive(false);
            startedDot.SetActive(false);
            completedIcon.SetActive(false);
            button.onClick.RemoveAllListeners();
            button.enabled = false;
            return;
        }

        if (label)
        {
            label.text = dayNumber.ToString();
            label.color = isFuture ? futureText : (isSelected ? selectedText : normalText);
        }

        if (selectionBG) selectionBG.SetActive(isSelected);
        if (startedDot) startedDot.SetActive(!isFuture && started && !completed);
        if (completedIcon) completedIcon.SetActive(!isFuture && completed);

        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.enabled = !isFuture;

            if (!isFuture && onClick != null)
                button.onClick.AddListener(() => onClick());
        }

    }
}

