using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Bomb : Hexagon
{
    [SerializeField] TextMeshPro counterText;

    private int counter;

    private void Start() 
    {
        counter = Random.Range(4, 8);    
        hexType = HexType.BOMB;
        SetText();
    }

    private void SetText()
    {
        counterText.text = counter.ToString();
    }

    public void DecreaseCounter()
    {
        counter--;
        if(counter <= 0)
            UIManager.Instance.OpenEndCanvas();
        else
            SetText();
    }

    public void Subcribe()
    {
        ScoreManager.Instance.bombs.Add(this);
    }

    public void Unsubscribe()
    {
        ScoreManager.Instance.bombs.Remove(this);
    }
}

