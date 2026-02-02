
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnglishGame : MonoBehaviour
{
    public GameObject MainPanel;
    public GameObject FieldPrefab;
    public GameObject SudokuFieldPanel;
    public GameObject LoseBack;
    public GameObject SettingsPanel;
    public RowColHighlighter rowColHighlighter;
    public VictoryOverlayController victoryOverlay;

    public Button InformationButton;
    public EnglishInform inform;
    public GameObject NoteBg;
    public Button BackButton;
    public Text TimerText;
    public Text ErrorText;
    public Text LevelsText;

    public List<Button> ControlButtons;
    public RemainingDigitsCounter digitsCounter;
    public Dictionary<Tuple<int, int>, EnglishFieldPrefabObject> EnglishFieldPrefabObjectDic => _englishFieldPrefabObjectDic;
    public bool timerEnabled = true;
    public event System.Action GameLoaded;
    public event System.Action BoardChanged;
    public int[,] Board => _gameObject?.Values;
    public int[,] Solution => _finalObject?.Values;

    private int _errorCount = 0;
    private int _dynamicMaxErrors;
    private const int MaxErrors = 3;

    private Stack<SudokuAction> actionHistory = new Stack<SudokuAction>();
    private English_SudokuObject _gameObject;
    private English_SudokuObject _finalObject;
    private EnglishFieldPrefabObject _currentHoveredFieldPrefab;
    private Dictionary<Tuple<int, int>, EnglishFieldPrefabObject> _englishFieldPrefabObjectDic =
        new Dictionary<Tuple<int, int>, EnglishFieldPrefabObject>();
    private Dictionary<Tuple<int, int>, Color> _cellTextColors = new Dictionary<Tuple<int, int>, Color>();
    private EnglishFieldPrefabObject _incorrectFieldPrefab = null;
    private float _timer = 0f;
    private bool IsInformationButtonActive = false;
    private bool _isTimerRunning = true;
    private bool _completed = false;

    private const string PP_MODE = "Game.Mode";
    private const string PP_DAILY_DATE = "Daily.SelectedDate";
    private const int STATS_DIFF_MIN = 0;
    private const int STATS_DIFF_MAX = 5;
    private static string StatsKey(int diff, string k) => $"Stats.{diff}.{k}";
    private bool _isDaily;
    private string K(string key) => _savePrefix + key;
    private string _savePrefix = "";

    void Start()
    {
        _completed = false;
        _isDaily = PlayerPrefs.GetString(PP_MODE, "normal") == "daily";
        if (_isDaily)
        {
            string date = PlayerPrefs.GetString(PP_DAILY_DATE, "");
            _savePrefix = "Daily_" + date + "_";
        }
        else
        {
            _savePrefix = "";
        }

        CreateFieldPrefabs();
        rowColHighlighter.Init(_englishFieldPrefabObjectDic);
        for (int i = 0; i < ControlButtons.Count; i++)
        {
            int digit = i + 1;
            ControlButtons[i].onClick.AddListener(() => OnControlButtonClicked(digit));
        }

        if (!_isDaily)
        {
            EnglishGameSettings.EasyMiddleHard_Number =
                PlayerPrefs.GetInt("SavedLevel", EnglishGameSettings.EasyMiddleHard_Number);
        }
        else
        {
            EnglishGameSettings.EasyMiddleHard_Number = 1;
        }

        if (PlayerPrefs.HasKey(K("Cell_0_0")))
            LoadGameState();
        else
        {
            if (_isDaily)
                CreateDailySudokuObject();
            else
                CreateSudokuObject();

            SaveInitialState();
            if (!_isDaily)
                Stats_AddWin(EnglishGameSettings.EasyMiddleHard_Number, _timer, _errorCount == 0);
        }
        AudioManager.Instance.BindButtons();
        UpdateErrorText();
        UpdateLevelText();

        if (!Settings.highlightSimilarNumbers)
            Settings.highlightSimilarNumbers = true;

        _errorCount = PlayerPrefs.GetInt(K("ErrorCount"), 0);
        _dynamicMaxErrors = Settings.errorsLimitEnabled ? MaxErrors : int.MaxValue;
        UpdateErrorText();
        UpdateRemainingDigits();

        GameLoaded?.Invoke();
        BoardChanged?.Invoke();
    }

    void Update()
    {
        if (_isTimerRunning)
        {
            _timer += Time.deltaTime;
            TimeSpan timeSpan = TimeSpan.FromSeconds(_timer);
            TimerText.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }
        TimerText.gameObject.SetActive(timerEnabled);
    }

    private class SudokuAction
    {
        public int Row, Column;
        public string PrevValue;
        public string[] PrevSmall;

        public SudokuAction(int row, int col, string val, string[] small)
        {
            Row = row; Column = col;
            PrevValue = val;
            PrevSmall = small;
        }
    }

    public EnglishFieldPrefabObject CurrentHoveredField => _currentHoveredFieldPrefab;

    public void SaveInitialState()
    {
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
            {
                PlayerPrefs.SetInt(K($"InitialCell_{r}_{c}"), _gameObject.Values[r, c]);
                PlayerPrefs.SetInt(K($"Solution_{r}_{c}"), _finalObject.Values[r, c]);
            }

        PlayerPrefs.SetInt(K("ErrorCount"), 0);
        PlayerPrefs.SetInt(K("SavedLevel"), EnglishGameSettings.EasyMiddleHard_Number);
        PlayerPrefs.SetFloat(K("ElapsedTime"), 0f);
        PlayerPrefs.Save();

        SaveGameState();
    }

    public void SaveGameState()
    {
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                PlayerPrefs.SetInt(K($"Cell_{r}_{c}"), _gameObject.Values[r, c]);

        PlayerPrefs.SetInt(K("ErrorCount"), _errorCount);
        PlayerPrefs.SetInt(K("SavedLevel"), EnglishGameSettings.EasyMiddleHard_Number);
        PlayerPrefs.SetFloat(K("ElapsedTime"), _timer);
        PlayerPrefs.Save();
    }

    public void LoadGameState()
    {
        _gameObject = new English_SudokuObject();
        _finalObject = new English_SudokuObject();

        _cellTextColors.Clear();
        if (_isDaily)
        {
            int dailyDiff = PlayerPrefs.GetInt(K("DailyDiff"), 0);
            EnglishGameSettings.EasyMiddleHard_Number = dailyDiff switch
            {
                0 => 1,
                1 => 2,
                2 => 3,
                _ => 1
            };
        }
        else
        {
            EnglishGameSettings.EasyMiddleHard_Number =
                PlayerPrefs.GetInt(K("SavedLevel"), EnglishGameSettings.EasyMiddleHard_Number);
        }
        UpdateLevelText();

        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                int val = PlayerPrefs.GetInt(K($"Cell_{r}_{c}"), 0);
                int sol = PlayerPrefs.GetInt(K($"Solution_{r}_{c}"), 0);
                int initialVal = PlayerPrefs.GetInt(K($"InitialCell_{r}_{c}"), 0);

                _gameObject.Values[r, c] = val;
                _finalObject.Values[r, c] = sol;

                var cell = _englishFieldPrefabObjectDic[Tuple.Create(r, c)];

                if (initialVal != 0)
                {
                    cell.SetNumber(initialVal);
                    cell.IsChangeAble = false;
                    cell.Instance.GetComponent<Image>().color = rowColHighlighter.baseCellColor;
                    SetTextColor(cell, ColorsMenu.CurrentBoardDigit);
                }
                else if (val != 0)
                {
                    cell.SetNumber(val);
                    cell.IsChangeAble = true;

                    var key = Tuple.Create(r, c);
                    bool correct = (val == sol);

                    if (correct)
                    {
                        var blueText = new Color(0.392f, 0.584f, 0.929f);
                        SetTextColor(cell, blueText);
                        _cellTextColors[key] = blueText;
                    }
                    else
                    {
                        var redText = new Color(0.941f, 0.502f, 0.502f);
                        SetTextColor(cell, redText);
                        _cellTextColors[key] = redText;
                    }
                }
                else
                {
                    cell.SetNumber(0);
                    cell.IsChangeAble = true;
                    cell.Instance.GetComponent<Image>().color = rowColHighlighter.baseCellColor;
                    SetTextColor(cell, ColorsMenu.CurrentBoardDigit);
                }
            }
        }

        _errorCount = PlayerPrefs.GetInt(K("ErrorCount"), 0);
        _timer = PlayerPrefs.GetFloat(K("ElapsedTime"), 0f);

        _dynamicMaxErrors = Settings.errorsLimitEnabled ? _errorCount + 1 : int.MaxValue;

        UpdateErrorText();
        TimerText.text = $"{TimeSpan.FromSeconds(_timer):mm\\:ss}";
        UpdateRemainingDigits();

        GameLoaded?.Invoke();
        BoardChanged?.Invoke();
    }

    public void LoadInitialState()
    {
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                int initialVal = PlayerPrefs.GetInt(K($"InitialCell_{r}_{c}"), 0);
                _gameObject.Values[r, c] = initialVal;

                var cell = _englishFieldPrefabObjectDic[Tuple.Create(r, c)];

                if (initialVal != 0)
                {
                    cell.SetNumber(initialVal);
                    cell.IsChangeAble = false;
                    cell.Instance.GetComponent<Image>().color = rowColHighlighter.baseCellColor;
                    SetTextColor(cell, ColorsMenu.CurrentBoardDigit);
                }
                else
                {
                    cell.SetNumber(0);
                    cell.IsChangeAble = true;
                    SetTextColor(cell, ColorsMenu.CurrentBoardDigit);
                    cell.Instance.GetComponent<Image>().color = rowColHighlighter.baseCellColor;
                }

                if (cell.IsChangeAble)
                {
                    cell.SetHoverMode();
                }
            }
        }
        UpdateRemainingDigits();
        GameLoaded?.Invoke();
        BoardChanged?.Invoke();
    }

    public void ResetTimer() => _timer = 0f;
    public void ResetErrors() => _errorCount = 0;

    public void RestoreInitialSudoku()
    {
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                int initialValue = PlayerPrefs.GetInt(K($"InitialCell_{r}_{c}"), 0);
                var field = _englishFieldPrefabObjectDic[Tuple.Create(r, c)];

                if (initialValue != 0)
                {
                    field.SetNumber(initialValue);
                    field.IsChangeAble = false;
                }
                else
                {
                    field.SetNumber(0);
                    field.IsChangeAble = true;
                }
                field.Instance.GetComponent<Image>().color = rowColHighlighter.baseCellColor;
            }
        }
        UpdateRemainingDigits();
        GameLoaded?.Invoke();
        BoardChanged?.Invoke();
    }

    public void OnPuzzleCompleted()
    {
        if (_completed) return;
        _completed = true;

        _isTimerRunning = false;
        bool isDaily = PlayerPrefs.GetString("Game.Mode", "normal") == "daily";

        if (isDaily)
        {
            string date = PlayerPrefs.GetString("Daily.SelectedDate", "");
            if (!string.IsNullOrEmpty(date))
            {
                PlayerPrefs.SetInt($"DailyCompleted_{date}", 1);
                PlayerPrefs.Save();
            }
        }
        if (!isDaily)
            Stats_AddWin(EnglishGameSettings.EasyMiddleHard_Number, _timer, _errorCount == 0);

        if (victoryOverlay != null)
            victoryOverlay.Show(isDaily);
    }


    public void UndoLastAction()
    {
        AudioManager.Instance.Play("switchsound4");
        if (actionHistory.Count == 0) return;

        var act = actionHistory.Pop();
        var key = Tuple.Create(act.Row, act.Column);
        var field = _englishFieldPrefabObjectDic[key];

        if (field.TryGetTextByName("Value", out Text tv))
            tv.text = act.PrevValue;
        int v = int.TryParse(act.PrevValue, out int tmp) ? tmp : 0;
        field.Number = v;
        _gameObject.Values[act.Row, act.Column] = v;

        for (int i = 1; i <= 9; i++)
            if (field.TryGetTextByName($"Number_{i}", out Text ts))
                ts.text = act.PrevSmall[i];

        UpdateRemainingDigits();
        BoardChanged?.Invoke();
    }

    public void OnErrorsLimitToggled(bool enabled)
    {
        if (enabled)
            _dynamicMaxErrors = (_errorCount < MaxErrors) ? MaxErrors : (_errorCount + 1);
        else
            _dynamicMaxErrors = int.MaxValue;

        UpdateErrorText();
    }

    public void ContinueAfterSecondChance()
    {
        if (Settings.errorsLimitEnabled)
        {
            _dynamicMaxErrors += 1;
            UpdateErrorText();
        }
    }

    public void OnSettingsButtonClicked()
    {
        SettingsPanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void ClickOn_BackButton()
    {
        SaveGameState();

        PlayerPrefs.SetString(PP_MODE, "normal");
        PlayerPrefs.Save();

        SceneManager.LoadScene("EnglishSelect");
    }

    public void GiveHint()
    {
        AudioManager.Instance.Play("switchsound6");
        string prevValue = "";
        _currentHoveredFieldPrefab.TryGetTextByName("Value", out Text tv);
        if (tv != null) prevValue = tv.text;

        string[] prevSmall = new string[10];
        for (int i = 1; i <= 9; i++)
            _currentHoveredFieldPrefab.TryGetTextByName($"Number_{i}", out Text ts);

        actionHistory.Push(new SudokuAction(
            _currentHoveredFieldPrefab.Row,
            _currentHoveredFieldPrefab.Column,
            prevValue,
            prevSmall
        ));

        int r = _currentHoveredFieldPrefab.Row;
        int c = _currentHoveredFieldPrefab.Column;
        int correct = _finalObject.Values[r, c];
        SetCell(r, c, correct);
    }

    public void ClearCell(int r, int c)
    {
        _gameObject.Values[r, c] = 0;

        var cell = _englishFieldPrefabObjectDic[Tuple.Create(r, c)];
        cell.SetNumber(0);
        SetTextColor(cell, ColorsMenu.CurrentBoardDigit);
        _cellTextColors.Remove(Tuple.Create(r, c));
        cell.Instance.GetComponent<Image>().color = rowColHighlighter.baseCellColor;

        UpdateRemainingDigits();
        SaveGameState();
        BoardChanged?.Invoke();
    }

    public void ClickOn_InformationButton()
    {
        IsInformationButtonActive = !IsInformationButtonActive;
        AudioManager.Instance.Play(IsInformationButtonActive ? "switchsound2" : "switchsound1");
        NoteBg.SetActive(IsInformationButtonActive);
    }

    public void ClickOn_EraseButton()
    {
        AudioManager.Instance.Play("switchsound5");
        if (_currentHoveredFieldPrefab == null) return;
        if (!_currentHoveredFieldPrefab.IsChangeAble) return;

        int r = _currentHoveredFieldPrefab.Row;
        int c = _currentHoveredFieldPrefab.Column;

        if (_currentHoveredFieldPrefab.Number != 0 && _currentHoveredFieldPrefab.Number != _finalObject.Values[r, c])
            _currentHoveredFieldPrefab.SetNumber(0);

        for (int i = 1; i <= 9; i++)
            if (_currentHoveredFieldPrefab.TryGetTextByName($"Number_{i}", out Text ts))
                ts.text = "";

        if (_currentHoveredFieldPrefab == _incorrectFieldPrefab)
            _incorrectFieldPrefab = null;

        ResetAllHighlights();
        UpdateRemainingDigits();
        BoardChanged?.Invoke();
    }

    public void ResetAllHighlights()
    {
        foreach (var cell in _englishFieldPrefabObjectDic.Values)
        {
            cell.Instance.GetComponent<Image>().color = rowColHighlighter.baseCellColor;

            var key = Tuple.Create(cell.Row, cell.Column);
            if (_cellTextColors.ContainsKey(key))
                SetTextColor(cell, _cellTextColors[key]);
            else
                SetTextColor(cell, ColorsMenu.CurrentBoardDigit);
        }
    }

    public void SetCell(int r, int c, int v)
    {
        _gameObject.Values[r, c] = v;

        var cell = _englishFieldPrefabObjectDic[Tuple.Create(r, c)];
        cell.SetNumber(v);

        var blueText = new Color(0.255f, 0.412f, 0.882f);
        SetTextColor(cell, blueText);
        _cellTextColors[Tuple.Create(r, c)] = blueText;


        if (Settings.autoDeleteNotesEnabled)
            inform.AutoDeleteNotes(v, r, c);

        UpdateRemainingDigits();
        SaveGameState();

        BoardChanged?.Invoke();
    }

    public void ReapplyThemeColors()
    {
        foreach (var kv in _englishFieldPrefabObjectDic)
        {
            var cell = kv.Value;
            cell.Instance.GetComponent<Image>().color = rowColHighlighter.baseCellColor;

            var key = Tuple.Create(cell.Row, cell.Column);

            if (_cellTextColors.TryGetValue(key, out var saved))
                SetTextColor(cell, saved);
            else
                SetTextColor(cell, ColorsMenu.CurrentBoardDigit);
        }

        rowColHighlighter.ClearHighlight();
    }
    public static void Stats_AddStarted(int diff)
    {
        diff = Mathf.Clamp(diff, STATS_DIFF_MIN, STATS_DIFF_MAX);

        int started = PlayerPrefs.GetInt(StatsKey(diff, "Started"), 0);
        PlayerPrefs.SetInt(StatsKey(diff, "Started"), started + 1);
        PlayerPrefs.Save();
    }

    public static void Stats_AddWin(int diff, float timeSec, bool noErrors)
    {
        diff = Mathf.Clamp(diff, STATS_DIFF_MIN, STATS_DIFF_MAX);

        int wins = PlayerPrefs.GetInt(StatsKey(diff, "Wins"), 0);
        PlayerPrefs.SetInt(StatsKey(diff, "Wins"), wins + 1);

        if (noErrors)
        {
            int wne = PlayerPrefs.GetInt(StatsKey(diff, "WinsNoErrors"), 0);
            PlayerPrefs.SetInt(StatsKey(diff, "WinsNoErrors"), wne + 1);
        }

        float best = PlayerPrefs.GetFloat(StatsKey(diff, "BestTimeSec"), 0f);
        if (best <= 0f || timeSec < best)
            PlayerPrefs.SetFloat(StatsKey(diff, "BestTimeSec"), timeSec);

        float total = PlayerPrefs.GetFloat(StatsKey(diff, "TotalWinTimeSec"), 0f);
        PlayerPrefs.SetFloat(StatsKey(diff, "TotalWinTimeSec"), total + timeSec);

        PlayerPrefs.Save();
    }
    private void CreateDailySudokuObject()
    {
        string date = PlayerPrefs.GetString(PP_DAILY_DATE, "");
        int seed = int.Parse(date);

        English_SudokuGenerator.CreateSudokuObjectSeeded(
            seed,
            out _finalObject,
            out _gameObject,
            out int emptyCount
        );

        PlayerPrefs.SetInt($"DailyStarted_{date}", 1);
        PlayerPrefs.Save();

        int dailyDiff = (emptyCount == 31) ? 0 : (emptyCount == 39) ? 1 : 2;
        PlayerPrefs.SetInt(K("DailyDiff"), dailyDiff);

        EnglishGameSettings.EasyMiddleHard_Number = dailyDiff switch
        {
            0 => 1,
            1 => 2,
            2 => 3,
            _ => 1
        };
        UpdateLevelText();

        for (int row = 0; row < 9; row++)
            for (int col = 0; col < 9; col++)
            {
                int v = _gameObject.Values[row, col];
                if (v != 0)
                {
                    var cell = _englishFieldPrefabObjectDic[new Tuple<int, int>(row, col)];
                    cell.SetNumber(v);
                    cell.IsChangeAble = false;
                }
            }

        UpdateRemainingDigits();
    }

    private void UpdateRemainingDigits()
    {
        if (digitsCounter != null)
            digitsCounter.UpdateCounts(_gameObject.Values);
    }

    private void SetTextColor(EnglishFieldPrefabObject fieldPrefab, Color color)
    {
        if (fieldPrefab.TryGetTextByName("Value", out Text text))
        {
            text.color = color;
        }
    }

    private void HighlightSimilarCells(int number, Color bgColor, EnglishFieldPrefabObject currentField = null)
    {
        if (!Settings.highlightSimilarNumbers) return;

        foreach (var field in _englishFieldPrefabObjectDic.Values)
        {
            if (field.Number == number && field != currentField && field.Number != 0)
            {
                field.Instance.GetComponent<Image>().color = bgColor;

                var key = Tuple.Create(field.Row, field.Column);
                if (_cellTextColors.TryGetValue(key, out Color origColor))
                    SetTextColor(field, origColor);
                else
                    SetTextColor(field, ColorsMenu.CurrentBoardDigit);
            }
        }
    }
    private void HighlightCellsInRowColumnAndBlock(int number, EnglishFieldPrefabObject currentField)
    {
        int row = currentField.Row;
        int column = currentField.Column;

        for (int c = 0; c < 9; c++)
        {
            var fieldPrefab = _englishFieldPrefabObjectDic[new Tuple<int, int>(row, c)];
            if (fieldPrefab.Number == number && fieldPrefab != currentField && fieldPrefab.Number != 0)
            {
                fieldPrefab.Instance.GetComponent<Image>().color = new Color(0.941f, 0.502f, 0.502f, 0.5f);
                SetTextColor(fieldPrefab, new Color(0.941f, 0.502f, 0.502f));
            }
        }

        for (int r = 0; r < 9; r++)
        {
            var fieldPrefab = _englishFieldPrefabObjectDic[new Tuple<int, int>(r, column)];
            if (fieldPrefab.Number == number && fieldPrefab != currentField && fieldPrefab.Number != 0)
            {
                fieldPrefab.Instance.GetComponent<Image>().color = new Color(0.941f, 0.502f, 0.502f, 0.5f);
                SetTextColor(fieldPrefab, new Color(0.941f, 0.502f, 0.502f));
            }
        }

        int startRow = (row / 3) * 3;
        int startColumn = (column / 3) * 3;

        for (int r = startRow; r < startRow + 3; r++)
        {
            for (int c = startColumn; c < startColumn + 3; c++)
            {
                var fieldPrefab = _englishFieldPrefabObjectDic[new Tuple<int, int>(r, c)];
                if (fieldPrefab.Number == number && fieldPrefab != currentField && fieldPrefab.Number != 0)
                {
                    fieldPrefab.Instance.GetComponent<Image>().color = new Color(0.941f, 0.502f, 0.502f, 0.5f);
                    SetTextColor(fieldPrefab, new Color(0.941f, 0.502f, 0.502f));
                }
            }
        }
    }

    private void UpdateErrorText()
    {
        if (Settings.errorsLimitEnabled)
            ErrorText.text = $"{_errorCount}/{_dynamicMaxErrors}";
        else
            ErrorText.text = $"{_errorCount}";
    }

    public void RemoveHighlight()
    {
        foreach (var fieldPrefab in _englishFieldPrefabObjectDic.Values)
        {
            fieldPrefab.Instance.GetComponent<Image>().color = rowColHighlighter.baseCellColor;

            var key = Tuple.Create(fieldPrefab.Row, fieldPrefab.Column);
            if (_cellTextColors.ContainsKey(key))
            {
                SetTextColor(fieldPrefab, _cellTextColors[new Tuple<int, int>(fieldPrefab.Row, fieldPrefab.Column)]);
            }
            else
            {
                SetTextColor(fieldPrefab, ColorsMenu.CurrentBoardDigit);
            }
        }
    }

    private void OnClick_FieldPrefab(EnglishFieldPrefabObject englishFieldPrefabObject)
    {
        RemoveHighlight();

        if (_currentHoveredFieldPrefab != null)
            _currentHoveredFieldPrefab.UnsetHoverMode();

        _currentHoveredFieldPrefab = englishFieldPrefabObject;

        if (rowColHighlighter.backlightEnabled)
            rowColHighlighter.HighlightRowColAndBlock(englishFieldPrefabObject.Row, englishFieldPrefabObject.Column);

        englishFieldPrefabObject.SetHoverMode();

        englishFieldPrefabObject.Instance.GetComponent<Image>().color = ColorsMenu.CurrentSelectedCell;

        if (_currentHoveredFieldPrefab.Number != 0)
            HighlightSimilarCells(_currentHoveredFieldPrefab.Number,
                                  new Color(0.392f, 0.584f, 0.929f),
                                  _currentHoveredFieldPrefab);

        _incorrectFieldPrefab = (_currentHoveredFieldPrefab == _incorrectFieldPrefab) ? _incorrectFieldPrefab : null;
    }


    public void OnControlButtonClicked(int number)
    {
        if (_currentHoveredFieldPrefab == null || !_currentHoveredFieldPrefab.IsChangeAble)
            return;

        string prev = _currentHoveredFieldPrefab.TryGetTextByName("Value", out Text tv) ? tv.text : "";
        string[] prevSmall = new string[10];
        for (int i = 1; i <= 9; i++)
            prevSmall[i] = _currentHoveredFieldPrefab.TryGetTextByName($"Number_{i}", out Text ts) ? ts.text : "";
        actionHistory.Push(new SudokuAction(
            _currentHoveredFieldPrefab.Row,
            _currentHoveredFieldPrefab.Column,
            prev,
            prevSmall
        ));

        RemoveHighlight();

        if (IsInformationButtonActive)
        {
            inform.HandleNoteInput(number, _currentHoveredFieldPrefab);
            return;
        }
        else
        {
            _currentHoveredFieldPrefab.SetNumber(number);
            int r = _currentHoveredFieldPrefab.Row;
            int c = _currentHoveredFieldPrefab.Column;
            _gameObject.Values[r, c] = number;

            bool isCorrect = CheckForCorrectness(_currentHoveredFieldPrefab, number);
            var key = Tuple.Create(r, c);

            if (Settings.autoDeleteNotesEnabled && isCorrect)
                inform.AutoDeleteNotes(number, r, c);

            if (isCorrect)
            {
                var blueText = new Color(0.255f, 0.412f, 0.882f);
                SetTextColor(_currentHoveredFieldPrefab, blueText);
                _cellTextColors[key] = blueText;
                _currentHoveredFieldPrefab.Instance.GetComponent<Image>().color = ColorsMenu.CurrentSelectedCell;


                if (IsPuzzleComplete())
                {
                    OnPuzzleCompleted();
                    return;
                }
            }
            else
            {
                var redText = new Color(0.941f, 0.502f, 0.502f);
                _errorCount++;
                UpdateErrorText();
                if (Settings.errorsLimitEnabled && _errorCount >= _dynamicMaxErrors)
                {
                    LoseBack.SetActive(true);
                    return;
                }
                SetTextColor(_currentHoveredFieldPrefab, redText);
                _cellTextColors[key] = redText;
                _currentHoveredFieldPrefab.Instance.GetComponent<Image>().color = ColorsMenu.CurrentSelectedCell;

            }

            HighlightSimilarCells(number, new Color(0.392f, 0.584f, 0.929f), _currentHoveredFieldPrefab);
            HighlightCellsInRowColumnAndBlock(number, _currentHoveredFieldPrefab);
            _incorrectFieldPrefab = _currentHoveredFieldPrefab;
        }

        UpdateErrorText();
        SaveGameState();
        UpdateRemainingDigits();
        BoardChanged?.Invoke();
    }

    private void CreateFieldPrefabs()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int column = 0; column < 9; column++)
            {
                GameObject instance = GameObject.Instantiate(FieldPrefab, SudokuFieldPanel.transform);

                EnglishFieldPrefabObject englishFieldPrefabObject = new EnglishFieldPrefabObject(instance, row, column);
                _englishFieldPrefabObjectDic.Add(new Tuple<int, int>(row, column), englishFieldPrefabObject);

                instance.GetComponent<Button>().onClick.AddListener(() => OnClick_FieldPrefab(englishFieldPrefabObject));
            }
        }
    }

    private void CreateSudokuObject()
    {
        English_SudokuGenerator.CreateSudokuObject(
            out English_SudokuObject finalObject, out English_SudokuObject gameObject);
        _gameObject = gameObject;
        _finalObject = finalObject;
        for (int row = 0; row < 9; row++)
        {
            for (int column = 0; column < 9; column++)
            {
                var currentValue = _gameObject.Values[row, column];
                if (currentValue != 0)
                {
                    EnglishFieldPrefabObject fieldObject = _englishFieldPrefabObjectDic[new Tuple<int, int>(row, column)];
                    fieldObject.SetNumber(currentValue);
                    fieldObject.IsChangeAble = false;
                }
            }
        }
        UpdateRemainingDigits();
    }

    private bool CheckForCorrectness(EnglishFieldPrefabObject fieldPrefab, int number)
    {
        return _finalObject.Values[fieldPrefab.Row, fieldPrefab.Column] == number;
    }

    private bool IsPuzzleComplete()
    {
        for (int row = 0; row < 9; row++)
            for (int col = 0; col < 9; col++)
                if (_gameObject.Values[row, col] != _finalObject.Values[row, col])
                    return false;

        return true;
    }

    void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }
    void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }
    void OnLocaleChanged(UnityEngine.Localization.Locale locale)
    {
        UpdateLevelText();
    }

    private async void UpdateLevelText()
    {
        string[] keys = { "lvl_beginner", "lvl_easy", "lvl_medium", "lvl_hard", "lvl_expert", "lvl_extreme" };
        int lvl = EnglishGameSettings.EasyMiddleHard_Number;
        string key = (lvl >= 0 && lvl < keys.Length) ? keys[lvl] : "lvl_beginner";
        string localized = await LocalizationSettings.StringDatabase.GetLocalizedStringAsync("UI Text", key).Task;
        LevelsText.text = localized;
    }
}