using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EndCanvas : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI moveText;
    [SerializeField] TextMeshProUGUI hexText;
    [SerializeField] TextMeshProUGUI bombText;

    public void Open()
    {
        panel.SetActive(true);
    }

    // Button method
    public void TryAgain()
    {
        LoadManager.Instance.Load();
    }

    public void SetValues(int score, int move, int hex, int bomb)
    {
        scoreText.text = "Score: " + score.ToString();
        moveText.text = "#" + move.ToString();
        hexText.text = "Hexagons: " + hex.ToString();
        bombText.text = "Bomb: " + bomb.ToString();
    }
}
