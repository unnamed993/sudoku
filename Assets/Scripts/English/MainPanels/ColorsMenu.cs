using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[System.Serializable]
public struct ThemePalette
{
    // Фоны
    public Color MainBG;
    public Color CardBG;
    public Color ActionBarUnderlay;
    public Color ActionButtonBG;
    public Color ActionButtonActiveBG;

    // Поле
    public Color Grid;
    public Color BoardCellBG;
    public Color BoardDigit;
    public Color RowColHighlight;

    // Кнопки 1–9
    public Color NumBtnBG;
    public Color NumBtnText;

    // Тексты и иконки
    public Color StatusText;
    public Color TopIcons;

    public Color SelectedCell;
    // Прочее
    public Color SliderThumb;
}

public class ColorsMenu : MonoBehaviour
{
    // ---------- Палитры и выбор темы ----------
    [Header("Темы")]
    public ThemePalette[] themes;
    private const string PP_THEME = "ui_theme_index";

    [Header("Кнопки выбора темы (по порядку)")]
    public List<Button> themeButtons;
    public RectTransform sharedTick;

    // ---------- Окно выбора цветов ----------
    [Header("Окно выбора цветов")]
    public GameObject colorsPanel;
    public Button openColorsButton;
    public Text colorsLabelSmallA;
    public Text colorsLabelBigA;
    public Image sliderHandle;

    // ---------- Фоны/панели ----------
    [Header("Фоны/панели")]
    public Image mainPanel;
    public List<Image> cardPanels;
    public List<Image> actionBarUnderlays;

    // ---------- Верхние иконки ----------
    [Header("Иконки верхней панели")]
    public List<Image> topIcons;

    // ---------- Action-кнопки ----------
    [Header("Action-кнопки")]
    public List<Button> actionButtons;
    public List<Image> actionActiveOverlays;

    // ---------- Кнопки 1–9 ----------
    [Header("Кнопки 1–9")]
    public Button[] numberButtons;

    // ---------- Поле судоку ----------
    [Header("Сетка/клетки/подсветка")]
    public Image[] gridImages;              // 8 линий
    public List<Image> cellBackgrounds;     // фоны клеток
    public List<Text> boardDigits;          // тексты цифр "Value"
    public List<RowColHighlighter> rowColHighlights;

    // ---------- Тексты статуса (ТОЛЬКО главная панель) ----------
    [Header("Статусы главной панели (меняются по StatusText)")]
    public List<Text> statusTexts;          // Levels, Timer, Errors, подписи action-кнопок

    // ---------- Корни панелей для автопокраски ----------
    [Header("Корни панелей для автопокраски")]
    public List<Transform> autoColorRoots;     // корень панели "Настройки"

    // ---------- Масштаб шрифта ----------
    [Header("Размер текста (слайдер)")]
    public Slider textSizeSlider;
    private const string PP_TSZ = "ui_text_size_index";
    public int[] mainFieldSizes = { 60, 64, 68, 70 };
    public int[] noteSizes = { 20, 20, 26, 26 };
    public int[] inputPanelSizes = { 60, 64, 68, 70 };

    [Header("Группы текстов для масштабирования")]
    public Transform sudokuFieldParent;
    private readonly List<Text> valueTexts = new();
    private readonly List<Text> noteTexts = new();
    public List<Text> inputPanelTexts = new();

    // ---------- Общие ----------
    public static Color CurrentBoardDigit;
    public static Color CurrentSelectedCell;

    // ---------- Анимации ----------
    [SerializeField] private CanvasGroup colorsPanelGroup;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float themeTransition = 0.25f;
    private Coroutine fadeRoutine;
    private Coroutine transitionRoutine;

    // ========================= LIFECYCLE =========================
    private void Start()
    {
        if (openColorsButton) openColorsButton.onClick.AddListener(OpenColorsPanel);

        for (int i = 0; i < themeButtons.Count; i++)
        {
            int idx = i;
            if (themeButtons[i]) themeButtons[i].onClick.AddListener(() => SelectTheme(idx));
        }

        // Слайдер размера
        if (textSizeSlider)
        {
            textSizeSlider.minValue = 0;
            textSizeSlider.maxValue = mainFieldSizes.Length - 1;
            textSizeSlider.wholeNumbers = true;
            textSizeSlider.onValueChanged.AddListener(ChangeAllTextSizes);
        }

        // Подготовка групп текстов и применение сохранённых размеров
        StartCoroutine(ApplySavedSizeNextFrame());
        IEnumerator ApplySavedSizeNextFrame()
        {
            yield return null; // дождаться инстансов поля
            CollectFieldTexts();
            AutoFillInputPanelTexts();

            int savedSize = Mathf.Clamp(PlayerPrefs.GetInt(PP_TSZ, 0), 0, mainFieldSizes.Length - 1);
            if (textSizeSlider) textSizeSlider.value = savedSize;
            ChangeAllTextSizes(savedSize);
        }

        // Применить сохранённую тему мгновенно (на старте — без анимации)
        int savedTheme = Mathf.Clamp(PlayerPrefs.GetInt(PP_THEME, 0), 0, Mathf.Max(0, (themes?.Length ?? 1) - 1));
        ApplyTheme(savedTheme);

        // Галочка активной темы
        if (sharedTick && savedTheme >= 0 && savedTheme < themeButtons.Count && themeButtons[savedTheme])
        {
            sharedTick.SetParent(themeButtons[savedTheme].transform, false);
            sharedTick.anchoredPosition = Vector2.zero;
            sharedTick.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (!colorsPanel || !colorsPanel.activeInHierarchy) return;

        Vector2 pos; bool down = false;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        { pos = Mouse.current.position.ReadValue(); down = true; }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        { pos = Touchscreen.current.primaryTouch.position.ReadValue(); down = true; }
        else pos = default;

        if (down && !IsPointerOverColorsPanel(pos)) CloseColorsPanel();

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            CloseColorsPanel();
    }

    // ========================= ПАНЕЛЬ ЦВЕТОВ =========================
    public void OpenColorsPanel()
    {
        if (!colorsPanel) return;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        colorsPanel.SetActive(true);
        if (colorsPanelGroup) fadeRoutine = StartCoroutine(FadeCanvasGroup(colorsPanelGroup, colorsPanelGroup.alpha, 1f));
    }

    public void CloseColorsPanel()
    {
        if (!colorsPanel) return;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        if (colorsPanelGroup) fadeRoutine = StartCoroutine(FadeOutAndDisable(colorsPanelGroup));
        else colorsPanel.SetActive(false);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to)
    {
        cg.alpha = from;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        cg.alpha = to;
    }

    private IEnumerator FadeOutAndDisable(CanvasGroup cg)
    {
        yield return FadeCanvasGroup(cg, cg.alpha, 0f);
        cg.interactable = false;
        cg.blocksRaycasts = false;
        if (colorsPanel) colorsPanel.SetActive(false);
    }

    // ========================= ВЫБОР ТЕМЫ =========================
    public void SelectTheme(int index)
    {
        index = Mathf.Clamp(index, 0, themes.Length - 1);

        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        transitionRoutine = StartCoroutine(TransitionToTheme(index));

        PlayerPrefs.SetInt(PP_THEME, index);

        if (sharedTick && index >= 0 && index < themeButtons.Count && themeButtons[index])
        {
            sharedTick.SetParent(themeButtons[index].transform, false);
            sharedTick.anchoredPosition = Vector2.zero;
            sharedTick.gameObject.SetActive(true);
        }
    }

    // ========================= ПРИМЕНЕНИЕ ТЕМЫ (быстро) =========================
    private void ApplyTheme(int i)
    {
        if (themes == null || themes.Length == 0) return;
        i = Mathf.Clamp(i, 0, themes.Length - 1);
        var t = themes[i];
        CurrentSelectedCell = t.SelectedCell;
        CurrentBoardDigit = t.BoardDigit;

        if (mainPanel) mainPanel.color = t.MainBG;
        foreach (var p in cardPanels) if (p) p.color = t.CardBG;
        foreach (var u in actionBarUnderlays) if (u) u.color = t.ActionBarUnderlay;
        foreach (var ico in topIcons) if (ico) ico.color = t.TopIcons;

        // Action-кнопки
        foreach (var btn in actionButtons)
        {
            if (!btn) continue;
            if (btn.targetGraphic) btn.targetGraphic.color = t.ActionButtonBG;

            var cb = btn.colors;
            cb.normalColor = t.ActionButtonBG;
            cb.highlightedColor = Mul(t.ActionButtonBG, 1.06f);
            cb.pressedColor = Mul(t.ActionButtonBG, 0.92f);
            cb.selectedColor = t.ActionButtonBG;
            cb.disabledColor = new Color(t.ActionButtonBG.r, t.ActionButtonBG.g, t.ActionButtonBG.b, 0.45f);
            btn.colors = cb;
        }
        for (int k = 0; k < actionActiveOverlays.Count; k++)
            if (actionActiveOverlays[k]) actionActiveOverlays[k].color = t.ActionButtonActiveBG;

        // Кнопки 1–9
        if (numberButtons != null)
        {
            for (int k = 0; k < numberButtons.Length; k++)
            {
                var b = numberButtons[k]; if (!b) continue;
                if (b.targetGraphic) b.targetGraphic.color = t.NumBtnBG;

                var st = b.colors;
                st.normalColor = t.NumBtnBG;
                st.highlightedColor = Mul(t.NumBtnBG, 1.06f);
                st.pressedColor = Mul(t.NumBtnBG, 0.92f);
                st.selectedColor = t.NumBtnBG;
                st.disabledColor = new Color(t.NumBtnBG.r, t.NumBtnBG.g, t.NumBtnBG.b, 0.45f);
                b.colors = st;

                var txt = b.GetComponentInChildren<Text>(true);
                if (txt) txt.color = t.NumBtnText;
            }
        }

        // Сетка, клетки, цифры поля
        if (gridImages != null)
            for (int g = 0; g < gridImages.Length; g++) if (gridImages[g]) gridImages[g].color = t.Grid;

        foreach (var img in cellBackgrounds) if (img) img.color = t.BoardCellBG;
        CollectBoardDigits();
        foreach (var txt in boardDigits) if (txt) txt.color = t.BoardDigit;

        // Подсветчик
        if (rowColHighlights == null || rowColHighlights.Count == 0)
            rowColHighlights = new List<RowColHighlighter>(Object.FindObjectsByType<RowColHighlighter>(FindObjectsSortMode.None));
        foreach (var rc in rowColHighlights)
        {
            if (!rc) continue;
            rc.baseCellColor = t.BoardCellBG;
            rc.highlightColor = t.RowColHighlight;
            rc.ClearHighlight();
        }

        // --- Тексты главной панели ---
        foreach (var txt in statusTexts)
            if (txt && !txt.GetComponent<KeepOwnColor>())
                txt.color = t.StatusText;

        // --- Автопокраска панелей (Настройки, Языки) ---
        foreach (var root in autoColorRoots)
            ColorizePanelTexts(root, t.StatusText);

        // Слайдер
        if (sliderHandle) sliderHandle.color = t.SliderThumb;

        // Синхронизация поля
        var eg = Object.FindAnyObjectByType<EnglishGame>();
        if (eg) eg.ReapplyThemeColors();
    }

    // ========================= ПЕРЕХОД ТЕМЫ (плавно) =========================
    private IEnumerator TransitionToTheme(int i)
    {
        if (themes == null || themes.Length == 0) yield break;
        var t = themes[i];
        CurrentSelectedCell = t.SelectedCell;
        CurrentBoardDigit = t.BoardDigit;

        // ---- МГНОВЕННЫЕ элементы ----
        foreach (var img in cellBackgrounds) if (img) img.color = t.BoardCellBG;

        CollectBoardDigits();
        foreach (var txt in boardDigits) if (txt) txt.color = t.BoardDigit;

        // Кнопки 1–9
        if (numberButtons != null)
        {
            for (int k = 0; k < numberButtons.Length; k++)
            {
                var b = numberButtons[k]; if (!b) continue;

                if (b.targetGraphic) b.targetGraphic.color = t.NumBtnBG;

                var st = b.colors;
                st.normalColor = t.NumBtnBG;
                st.highlightedColor = Mul(t.NumBtnBG, 1.06f);
                st.pressedColor = Mul(t.NumBtnBG, 0.92f);
                st.selectedColor = t.NumBtnBG;
                st.disabledColor = new Color(t.NumBtnBG.r, t.NumBtnBG.g, t.NumBtnBG.b, 0.45f);
                b.colors = st;

                var nTxt = b.GetComponentInChildren<Text>(true);
                if (nTxt) nTxt.color = t.NumBtnText;
            }
        }

        // Action-кнопки
        foreach (var btn in actionButtons)
        {
            if (!btn) continue;
            if (btn.targetGraphic) btn.targetGraphic.color = t.ActionButtonBG;

            var cb = btn.colors;
            cb.normalColor = t.ActionButtonBG;
            cb.highlightedColor = Mul(t.ActionButtonBG, 1.06f);
            cb.pressedColor = Mul(t.ActionButtonBG, 0.92f);
            cb.selectedColor = t.ActionButtonBG;
            cb.disabledColor = new Color(t.ActionButtonBG.r, t.ActionButtonBG.g, t.ActionButtonBG.b, 0.45f);
            btn.colors = cb;
        }
        for (int k = 0; k < actionActiveOverlays.Count; k++)
            if (actionActiveOverlays[k]) actionActiveOverlays[k].color = t.ActionButtonActiveBG;

        // Подсветчик
        if (rowColHighlights == null || rowColHighlights.Count == 0)
            rowColHighlights = new List<RowColHighlighter>(Object.FindObjectsByType<RowColHighlighter>(FindObjectsSortMode.None));
        foreach (var rc in rowColHighlights)
        {
            if (!rc) continue;
            rc.baseCellColor = t.BoardCellBG;
            rc.highlightColor = t.RowColHighlight;
            rc.ClearHighlight();
        }

        // Синхро поля
        var eg0 = Object.FindAnyObjectByType<EnglishGame>();
        if (eg0) eg0.ReapplyThemeColors();

        // ---- ПЛАВНЫЕ элементы оболочки ----
        var from_main = mainPanel ? mainPanel.color : Color.white;

        var from_cards = new Color[cardPanels.Count];
        for (int k = 0; k < cardPanels.Count; k++) from_cards[k] = cardPanels[k] ? cardPanels[k].color : Color.white;

        var from_under = new Color[actionBarUnderlays.Count];
        for (int k = 0; k < actionBarUnderlays.Count; k++) from_under[k] = actionBarUnderlays[k] ? actionBarUnderlays[k].color : Color.white;

        var from_icons = new Color[topIcons.Count];
        for (int k = 0; k < topIcons.Count; k++) from_icons[k] = topIcons[k] ? topIcons[k].color : Color.white;

        var from_grid = new Color[gridImages.Length];
        for (int k = 0; k < gridImages.Length; k++) from_grid[k] = gridImages[k] ? gridImages[k].color : Color.white;

        // Исходные цвета статусов главной панели
        var from_statusTexts = new Color[statusTexts.Count];
        for (int k = 0; k < statusTexts.Count; k++)
            from_statusTexts[k] = statusTexts[k] ? statusTexts[k].color : Color.white;

        var from_slider = sliderHandle ? sliderHandle.color : Color.white;

        // Мгновенно красим тексты панелей (они статичны)
        foreach (var root in autoColorRoots)
            ColorizePanelTexts(root, t.StatusText);

        float elapsed = 0f;
        while (elapsed < themeTransition)
        {
            elapsed += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(elapsed / themeTransition);

            if (mainPanel) mainPanel.color = Color.Lerp(from_main, t.MainBG, a);

            for (int k = 0; k < cardPanels.Count; k++)
                if (cardPanels[k]) cardPanels[k].color = Color.Lerp(from_cards[k], t.CardBG, a);

            for (int k = 0; k < actionBarUnderlays.Count; k++)
                if (actionBarUnderlays[k]) actionBarUnderlays[k].color = Color.Lerp(from_under[k], t.ActionBarUnderlay, a);

            for (int k = 0; k < topIcons.Count; k++)
                if (topIcons[k]) topIcons[k].color = Color.Lerp(from_icons[k], t.TopIcons, a);

            for (int k = 0; k < gridImages.Length; k++)
                if (gridImages[k]) gridImages[k].color = Color.Lerp(from_grid[k], t.Grid, a);

            // Плавно только статусы главной панели
            for (int k = 0; k < statusTexts.Count; k++)
            {
                var txt = statusTexts[k];
                if (txt && !txt.GetComponent<KeepOwnColor>())
                    txt.color = Color.Lerp(from_statusTexts[k], t.StatusText, a);
            }

            if (sliderHandle)
                sliderHandle.color = Color.Lerp(from_slider, t.SliderThumb, a);

            // (подсветчики — фиксация параметров, если нужно)
            foreach (var rc in rowColHighlights)
            {
                if (!rc) continue;
                rc.baseCellColor = t.BoardCellBG;
                rc.highlightColor = t.RowColHighlight;
            }

            yield return null;
        }

        // Финальная фиксация подсветки
        foreach (var rc in rowColHighlights)
        {
            if (!rc) continue;
            rc.baseCellColor = t.BoardCellBG;
            rc.highlightColor = t.RowColHighlight;
        }

        var eg = Object.FindAnyObjectByType<EnglishGame>();
        if (eg) eg.ReapplyThemeColors();
    }

    // ========================= МАСШТАБ ШРИФТА =========================
    private void CollectFieldTexts()
    {
        valueTexts.Clear();
        noteTexts.Clear();
        if (!sudokuFieldParent) return;

        foreach (Transform cell in sudokuFieldParent)
        {
            var texts = cell.GetComponentsInChildren<Text>(true);
            foreach (var t in texts)
            {
                if (!t) continue;
                if (t.transform.name.StartsWith("Number_")) noteTexts.Add(t);
                else valueTexts.Add(t); // основной Value
            }
        }
    }

    private void AutoFillInputPanelTexts()
    {
        inputPanelTexts.Clear();
        if (numberButtons == null) return;
        foreach (var b in numberButtons)
        {
            if (!b) continue;
            var txt = b.GetComponentInChildren<Text>(true);
            if (txt) inputPanelTexts.Add(txt);
        }
    }

    public void ChangeAllTextSizes(float value)
    {
        CollectFieldTexts();
        AutoFillInputPanelTexts();

        int idx = Mathf.Clamp((int)value, 0, mainFieldSizes.Length - 1);

        foreach (var t in valueTexts) if (t) t.fontSize = mainFieldSizes[idx];
        foreach (var t in noteTexts) if (t) t.fontSize = noteSizes[idx];
        foreach (var t in inputPanelTexts) if (t) t.fontSize = inputPanelSizes[idx];

        PlayerPrefs.SetInt(PP_TSZ, idx);
    }

    // ========================= УТИЛИТЫ =========================
    private static Color Mul(Color c, float k)
    {
        return new Color(
            Mathf.Clamp01(c.r * k),
            Mathf.Clamp01(c.g * k),
            Mathf.Clamp01(c.b * k),
            c.a
        );
    }

    private void CollectBoardDigits()
    {
        boardDigits.Clear();

        if (sudokuFieldParent)
        {
            foreach (var t in sudokuFieldParent.GetComponentsInChildren<Text>(true))
                if (t && t.gameObject.name == "Value") boardDigits.Add(t);
            if (boardDigits.Count > 0) return;
        }

        foreach (var t in Object.FindObjectsByType<Text>(FindObjectsSortMode.None))
            if (t && t.gameObject.name == "Value") boardDigits.Add(t);
    }

    private bool IsPointerOverColorsPanel(Vector2 screenPos)
    {
        var es = EventSystem.current;
        var raycaster = GetComponentInParent<GraphicRaycaster>();
        if (!es || !raycaster || !colorsPanel) return false;

        var ped = new PointerEventData(es) { position = screenPos };
        var results = new List<RaycastResult>();
        raycaster.Raycast(ped, results);

        foreach (var hit in results)
            if (hit.gameObject && hit.gameObject.transform.IsChildOf(colorsPanel.transform))
                return true;

        return false;
    }

    // ---- Автопокраска текстов под корнем панели ----
    private void ColorizePanelTexts(Transform root, Color color)
    {
        if (!root) return;
        var texts = root.GetComponentsInChildren<Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            var t = texts[i];
            if (!t) continue;
            if (t.GetComponent<KeepOwnColor>()) continue; // заголовки/исключения
            t.color = color;
        }
    }
}
