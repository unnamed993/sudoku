using UnityEngine;

public class SelectPanelsController : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject dailyPanel;
    public GameObject killerPanel;
    public GameObject statsPanel;
    public StatsPanelController statsController;
    public TabsScrollController tabsController;
    void Start()
    {
        if (PlayerPrefs.GetInt("UI.OpenDailyPanel", 0) == 1)
        {
            PlayerPrefs.DeleteKey("UI.OpenDailyPanel");
            ShowDaily();
        }
        else
        {
            ShowMain();
        }
    }
    private void HideAll()
    {
        if (mainPanel) mainPanel.SetActive(false);
        if (dailyPanel) dailyPanel.SetActive(false);
        if (killerPanel) killerPanel.SetActive(false);
        if (statsPanel) statsPanel.SetActive(false);
    }

    public void ShowMain()
    {
        HideAll();
        if (mainPanel) mainPanel.SetActive(true);
    }

    public void ShowDaily()
    {
        HideAll();
        if (dailyPanel) dailyPanel.SetActive(true);
    }

    public void ShowKiller()
    {
        HideAll();
        if (killerPanel) killerPanel.SetActive(true);
    }

    public void ShowStats()
    {
        HideAll();
        if (statsPanel) statsPanel.SetActive(true);

        if (tabsController) tabsController.enabled = false;
        if (tabsController) tabsController.enabled = true;

        if (statsController) statsController.enabled = false;
        if (statsController) statsController.enabled = true;
    }
}