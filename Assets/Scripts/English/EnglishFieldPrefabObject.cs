using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class EnglishFieldPrefabObject
{
    private int _row;
    private int _column;
    private GameObject _instance;

    public GameObject Instance
    {
        get { return _instance; }
        private set { _instance = value; }
    }

    public EnglishFieldPrefabObject(GameObject instance, int row, int column)
    {
        _instance = instance;
        Row = row;
        _column = column;
    }
    public bool IsChangeAble = true;

    public bool TryGetTextByName(string name, out Text text)
    {
        text = null;
        Text[] texts = _instance.GetComponentsInChildren<Text>();
        foreach (var currentText in texts)
        {
            if (currentText.name.Equals(name))
            {
                text = currentText;
                return true;
            }
        }
        return false;
    }

    public int Number;
    public int Row { get => _row; set => _row = value; }
    public int Column { get => _column; set => _column = value; }

    public void ChangeColorToRed()
    {
        Instance.GetComponent<Image>().color = Color.red;
    }

    public void SetHoverMode()
    {
        {
            Instance.GetComponent<Image>().color = new Color(0.49f, 0.73f, 1f);
        }
    }

    public void UnsetHoverMode()
    {
        Instance.GetComponent<Image>().color = new Color(1f, 1f, 1f);
    }

    public void SetNumber(int number)
    {
        Number = number;
        if (TryGetTextByName("Value", out Text text))
        {
            if (number == 0)
            {
                text.text = "";
            }
            else
            {
                text.text = number.ToString();
            }

            for (int i = 1; i < 10; i++)
            {
                if (TryGetTextByName($"Number_{i}", out Text textNumber))
                {
                    textNumber.text = "";
                }
            }
        }
    }

    public void SetSmallNumber(int number)
    {
        if (TryGetTextByName($"Number_{number}", out Text text))
        {
            text.text = number.ToString();
            if (TryGetTextByName("Value", out Text textValue))
            {
                textValue.text = "";
            }
        }
    }
}