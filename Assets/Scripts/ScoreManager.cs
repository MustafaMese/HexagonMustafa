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

    public int Score { get => score; }
    public int BombScore { get => bombScore; }
    public int MoveCount { get => moveCount; }
    public int ExplodedBomb { get => explodedBomb; }
    public int ExplodedHexagon { get => explodedHexagon;  }

    [SerializeField] Canvas canvas;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI moveText;

    private int score = 0;
    private int bombScore = 0;
    private int moveCount = 0;
    private int explodedBomb = 0;
    private int explodedHexagon = 0;

    public bool createBomb;

    public List<Bomb> bombs = new List<Bomb>();

    private void Awake() 
    {
        instance = this;
    }

    private void Start() 
    {
        canvas.enabled = false;
        SetScoreText();
        createBomb = false;
    }

    public void Enable()
    {
        canvas.enabled = true;
        Reset();
    }

    public void Disable()
    {
        canvas.enabled = false;
    }

    public void IncreaseScore(int point)
    {
        score += point;
        bombScore += point;

        if(bombScore > 990)
        {
            createBomb = true;
            bombScore = 0;
        }

        SetScoreText();
    }

    public void IncreaseMoveCount()
    {
        moveCount++;
        SetMoveText();
    }

    private void Reset()
    {
        score = 0;
        bombScore = 0;
        moveCount = 0;
        explodedBomb = 0;
        explodedHexagon = 0;
        SetScoreText();
        SetMoveText();
    }

    public void IncreaseExplodedHexCount()
    {
        explodedHexagon += 3;
    }

    public void IncreaseExplodedBombCount()
    {
        explodedBomb++;
    }

    private void SetScoreText()
    {
        scoreText.text = score.ToString();
    }

    private void SetMoveText()
    {
        moveText.text = "#" + moveCount.ToString();
    }

    public void SetBombs()
    {
        for (var i = 0; i < bombs.Count; i++)
        {
            bombs[i].DecreaseCounter();
        }
    }

    public void TryAgain()
    {
        LoadManager.Instance.Load();
    }
}
