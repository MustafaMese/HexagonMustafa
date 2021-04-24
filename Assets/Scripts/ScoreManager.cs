using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    private static ScoreManager instance = null;

    public static ScoreManager Instance
    {
        get { return instance; }
        set { instance = value; }
    }

    [SerializeField] Canvas canvas;
    [SerializeField] TextMeshProUGUI scoreText;

    private int score = 0;
    public int Score { get => score; set => score = value; }

    public List<Bomb> bombs = new List<Bomb>();

    private void Awake() 
    {
        instance = this;
    }

    private void Start() 
    {
        SetText();
    }

    public void Disable()
    {
        canvas.enabled = false;
    }

    public void IncreaseScore(int point)
    {
        Score += point;
        SetText();
    }

    private void SetText()
    {
        scoreText.text = Score.ToString();
    }

    public void SetBombs()
    {
        for (var i = 0; i < bombs.Count; i++)
        {
            bombs[i].DecreaseCounter();
        }
    }
}
