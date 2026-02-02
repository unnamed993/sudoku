using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AutoFillController : MonoBehaviour
{
    public EnglishGame game;
    public Button autoFillButton;

    private int threshold = 12;
    private float stepDelay = 0.06f;
    private bool triggerWin = true;

    [SerializeField] private float slideOffset = 420f;
    [SerializeField] private float slideDuration = 0.28f;
    private RectTransform _rt;
    private Vector2 _posVisible, _posHidden;
    private bool _inited;
    private Coroutine _slideCo;

    private readonly AutoFillService _svc = new AutoFillService();
    private bool _playing;

    void Awake()
    {
        if (autoFillButton != null)
        {
            autoFillButton.onClick.AddListener(() => { if (!_playing) StartCoroutine(AutoFillSeq()); });
            _rt = autoFillButton.GetComponent<RectTransform>();
        }
    }

    void OnEnable()
    {
        if (game != null)
        {
            game.GameLoaded += Reevaluate;
            game.BoardChanged += Reevaluate;
        }
        Reevaluate();
    }

    void OnDisable()
    {
        if (game != null)
        {
            game.GameLoaded -= Reevaluate;
            game.BoardChanged -= Reevaluate;
        }
    }
    private void EnsureInit()
    {
        if (_inited || _rt == null) return;
        _posVisible = _rt.anchoredPosition;
        _posHidden = _posVisible + new Vector2(slideOffset, 0f);
        _inited = true;
    }

    private void Reevaluate()
    {
        if (game == null || autoFillButton == null || game.Board == null || game.Solution == null)
        {
            if (autoFillButton) autoFillButton.gameObject.SetActive(false);
            return;
        }

        bool can = _svc.IsAutoFillAvailable(game.Board, game.Solution, threshold);

        if (can)
        {
            if (!autoFillButton.gameObject.activeSelf)
            {
                autoFillButton.gameObject.SetActive(true);
                EnsureInit();
                if (_rt != null)
                {
                    if (_slideCo != null) StopCoroutine(_slideCo);
                    _slideCo = StartCoroutine(SlideIn());
                }
            }
        }
        else
        {
            if (autoFillButton.gameObject.activeSelf)
                autoFillButton.gameObject.SetActive(false);
        }

        autoFillButton.interactable = can && !_playing;
    }

    private IEnumerator SlideIn()
    {
        _rt.anchoredPosition = _posHidden;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, slideDuration);
            float k = t * t * (3f - 2f * t);
            _rt.anchoredPosition = Vector2.LerpUnclamped(_posHidden, _posVisible, k);
            yield return null;
        }
        _rt.anchoredPosition = _posVisible;
        _slideCo = null;
    }

    private IEnumerator AutoFillSeq()
    {
        _playing = true;
        if (autoFillButton) autoFillButton.gameObject.SetActive(false);
        SetInputLock(true);

        foreach (var (r, c, v) in _svc.GetEmptyCellsRowOrder(game.Board, game.Solution))
        {
            game.SetCell(r, c, v);

            var cell = game.EnglishFieldPrefabObjectDic[new System.Tuple<int, int>(r, c)];
            var t = cell.Instance.transform;
            t.localScale = Vector3.one * 0.85f;
            float tLerp = 0f;
            while (tLerp < 1f)
            {
                tLerp += Time.deltaTime * 10f;
                t.localScale = Vector3.Lerp(t.localScale, Vector3.one, tLerp);
                yield return null;
            }

            yield return new WaitForSeconds(stepDelay);
        }

        SetInputLock(false);
        _playing = false;

        if (triggerWin && _svc.CountEmpty(game.Board) == 0)
        {
            game.OnPuzzleCompleted();
        }
        else
        {
            Reevaluate();
        }
    }

    private void SetInputLock(bool locked)
    {
        if (game != null && game.ControlButtons != null)
            foreach (var b in game.ControlButtons) if (b) b.interactable = !locked;
        if (autoFillButton) autoFillButton.interactable = !locked;
    }
}