using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { START, READY, STOP, END }

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    public static GameManager Instance
    {
        get { return instance; }
        set { instance = value; }
    }

    [SerializeField] UIManager uiManagerPrefab;
    [SerializeField] LoadManager loadManagerPrefab;
    [SerializeField] InputManager inputManagerPrefab;

    private UIManager uiManager;
    private LoadManager loadManager;
    private InputManager inputManager;

    public GameState gameState = GameState.READY;

    private void Awake()
    {
        Instance = this;
        Initialize();
    }

    private void Initialize()
    {
        uiManager = Instantiate(uiManagerPrefab);
        loadManager = Instantiate(loadManagerPrefab);
        inputManager = Instantiate(inputManagerPrefab);
    }
}