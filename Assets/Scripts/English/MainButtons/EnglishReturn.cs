using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class EnglishReturn : MonoBehaviour
{
    public EnglishGame englishGame;
    public Button returnButton;

    public void OnReturnClick()
    {
        if (englishGame != null)
        {
            englishGame.UndoLastAction();
        }
        else
        {

        }
    }
}
