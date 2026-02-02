using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public class SceneSelectController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject MainMenuPanel;
    public GameObject LevelSelectPanel;
    public GameObject DimBackground;

    [Header("Popup (Шторка)")]
    public RectTransform PopupBackground;

    [Header("Animation")]
    [SerializeField] float snapDuration = 0.15f;
    [SerializeField] float closeThreshold = 120f;

    float openY, closedY;
    bool inited;
    Vector2 startPointer;
    float startY;
    Coroutine anim;

    void Start()
    {
        if (MainMenuPanel) MainMenuPanel.SetActive(true);
        if (DimBackground) DimBackground.SetActive(false);
        if (LevelSelectPanel) LevelSelectPanel.SetActive(false);
        AudioManager.Instance.BindButtons();
        InitMetrics();
    }

    void InitMetrics()
    {
        if (!PopupBackground) return;
        if (!inited) { openY = PopupBackground.anchoredPosition.y; inited = true; }
        closedY = openY - PopupBackground.rect.height;
    }

    public void OnContinue() => SceneManager.LoadScene("EnglishGame");
    public void OnNewGame() => Open();
    public void OnBackdropClick() => Close();

    public void ET_BeginDrag(BaseEventData data)
    {
        var e = (PointerEventData)data;
        StopAnim();
        startPointer = e.position;
        startY = PopupBackground.anchoredPosition.y;
    }

    public void ET_Drag(BaseEventData data)
    {
        var e = (PointerEventData)data;
        float dy = e.position.y - startPointer.y;
        float y = Mathf.Clamp(startY + dy, closedY, openY);
        SetY(y);
    }

    public void ET_EndDrag(BaseEventData data)
    {
        var e = (PointerEventData)data;
        float dy = e.position.y - startPointer.y;
        float y = PopupBackground.anchoredPosition.y;

        bool pass = dy < -closeThreshold;
        bool belowMid = y < (openY + closedY) * 0.5f;

        if (pass || belowMid) Close();
        else SnapTo(openY, false);
    }

    public void Open()
    {
        StopAnim();
        InitMetrics();

        if (LevelSelectPanel && !LevelSelectPanel.activeSelf) LevelSelectPanel.SetActive(true);
        if (DimBackground && !DimBackground.activeSelf) DimBackground.SetActive(true);

        SetY(closedY);
        SnapTo(openY, deactivate: false);
    }

    public void Close()
    {
        StopAnim();
        SnapTo(closedY, deactivate: true);
    }

    void SetY(float y)
    {
        var p = PopupBackground.anchoredPosition;
        p.y = y;
        PopupBackground.anchoredPosition = p;
    }

    void SnapTo(float targetY, bool deactivate)
    {
        anim = StartCoroutine(Snap(targetY, deactivate));
    }

    IEnumerator Snap(float targetY, bool deactivate)
    {
        Vector2 from = PopupBackground.anchoredPosition;
        Vector2 to = new Vector2(from.x, targetY);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / snapDuration;
            float k = Mathf.SmoothStep(0f, 1f, t);
            PopupBackground.anchoredPosition = Vector2.LerpUnclamped(from, to, k);
            yield return null;
        }
        SetY(targetY);

        if (deactivate)
        {
            if (DimBackground) DimBackground.SetActive(false);
            if (LevelSelectPanel) LevelSelectPanel.SetActive(false);
        }
        anim = null;
    }

    void StopAnim()
    {
        if (anim != null) { StopCoroutine(anim); anim = null; }
    }
}
