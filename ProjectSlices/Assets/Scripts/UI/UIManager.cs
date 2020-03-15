using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public int Score;
    public TextMeshProUGUI ScoreText;
    public Button Restart;
    public Button Start;
    public GameObject ClickPanel;
    public GameObject GameOver;

    public void ScoreIncreaseAndAssigned(int x)
    {
        Score += x;
        ScoreText.text = "Score : " + Score.ToString();
    }
}
