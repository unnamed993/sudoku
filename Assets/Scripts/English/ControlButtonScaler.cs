using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ControlButtonScaler : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private string soundKey = "switchsound1";
    private Vector3 _origScale;

    void Awake()
    {
        _origScale = transform.localScale;
        var btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(() => AudioManager.Instance.Play(soundKey));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.localScale = _origScale * 0.8f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = _origScale;
    }
}