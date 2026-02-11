using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DaysGridSwipe : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public CalendarGenerator calendar;
    public float minSwipePixels = 80f;

    private Vector2 _downPos;
    private bool _swiped;

    public void OnPointerDown(PointerEventData eventData)
    {
        _downPos = eventData.position;
        _swiped = false;
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (_swiped || calendar == null) return;

        float dx = eventData.position.x - _downPos.x;
        if (Mathf.Abs(dx) < minSwipePixels) return;

        _swiped = true;

        if (dx < 0f) calendar.NextMonth();
        else calendar.PreviousMonth();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_swiped) return;

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        for (int i = 0; i < results.Count; i++)
        {
            var go = results[i].gameObject;

            if (go == this.gameObject) continue;

            var btn = go.GetComponentInParent<Button>();
            if (btn != null && btn.interactable)
            {
                ExecuteEvents.Execute(btn.gameObject, eventData, ExecuteEvents.pointerClickHandler);
                return;
            }
        }
    }
}
