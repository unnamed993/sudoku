using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SwipePager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Refs")]
    public RectTransform viewport;
    public RectTransform content;
    public RectTransform pageCurrent;
    public CalendarController3Pages calendar;

    [Header("Rules")]
    [Range(0.1f, 0.9f)] public float switchPercent = 0.50f;
    public float dragStartThresholdPx = 25f;
    public float snapSpeed = 12f;

    float W;
    float dragStartTime;
    Vector2 contentStart;
    Vector2 pointerStart;

    [Range(0.05f, 0.5f)]
    public float rubber = 0.2f;
    public float flickVelocity = 1800f;

    bool dragging;
    bool swipeActive;
    Coroutine anim;
    void Start()
    {
        Recalc();
        CenterImmediate();
    }
    void Recalc()
    {
        W = pageCurrent.rect.width;
    }
    void CenterImmediate()
    {
        content.anchoredPosition = new Vector2(-W, 0f);
    }
    public void OnBeginDrag(PointerEventData e)
    {
        Recalc();

        dragging = true;
        swipeActive = false;

        contentStart = content.anchoredPosition;
        pointerStart = e.position;
        dragStartTime = Time.unscaledTime;

        if (anim != null) StopCoroutine(anim);
        anim = null;
    }
    public void OnDrag(PointerEventData e)
    {
        if (!dragging) return;

        float dx = e.position.x - pointerStart.x;

        if (!swipeActive)
        {
            if (Mathf.Abs(dx) < dragStartThresholdPx) return;
            swipeActive = true;
        }

        float newX = contentStart.x + dx;

        if (calendar != null)
        {
            bool blockNext = (dx < 0f && !calendar.CanMove(+1));
            bool blockPrev = (dx > 0f && !calendar.CanMove(-1));

            if (blockNext || blockPrev)
            {
                newX = contentStart.x + dx * rubber;
            }
            content.anchoredPosition = new Vector2(newX, 0f);
        }
    }
    public void OnEndDrag(PointerEventData e)
    {
        dragging = false;

        if (!swipeActive)
        {
            CenterImmediate();
            return;
        }

        float moved = content.anchoredPosition.x - contentStart.x;
        float threshold = W * switchPercent;
        float dragTime = Time.unscaledTime - dragStartTime;
        float distance = e.position.x - pointerStart.x;

        float velocity = Mathf.Abs(distance) / Mathf.Max(dragTime, 0.001f);

        int dir = 0;
        if (Mathf.Abs(moved) >= W * switchPercent)
        {
            dir = (moved < 0f) ? +1 : -1;
        }
        else if (velocity > flickVelocity)
        {
            dir = (distance < 0f) ? +1 : -1;
        }

        if (dir != 0 && calendar != null && !calendar.CanMove(dir))
            dir = 0;

        float targetX = (dir == 0) ? -W : (dir == +1 ? -2f * W : 0f);

        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(SnapAndMaybeMove(targetX, dir));
    }
    IEnumerator SnapAndMaybeMove(float targetX, int dir)
    {
        Vector2 from = content.anchoredPosition;
        Vector2 to = new Vector2(targetX, 0f);

        float dur = 0.18f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dur;
            content.anchoredPosition = Vector2.Lerp(from, to, Mathf.Clamp01(t));
            yield return null;
        }

        content.anchoredPosition = to;

        if (dir != 0 && calendar != null)
            calendar.MoveMonth(dir);

        CenterImmediate();
        anim = null;
    }

}
