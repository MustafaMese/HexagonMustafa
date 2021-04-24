using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static UIManager instance = null;

    public static UIManager Instance
    {
        get { return instance; }
        set { instance = value; }
    }

    [SerializeField] EndCanvas endCanvasPrefab;
    [SerializeField] ScoreManager scoreManagerPrefab;

    private EndCanvas endCanvas;
    private ScoreManager scoreManager;

    private void Awake() 
    {
        instance = this; 
        Initialize();   
    }

    private void Initialize()
    {
        endCanvas = Instantiate(endCanvasPrefab);
        scoreManager = Instantiate(scoreManagerPrefab);
    }

    public void OpenEndCanvas()
    {
        GameManager.Instance.gameState = GameState.END;
        scoreManager.Disable();
        endCanvas.Open();
    }
}
