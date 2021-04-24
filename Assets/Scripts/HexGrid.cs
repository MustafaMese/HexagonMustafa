using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    [SerializeField] Vector2 matrix = new Vector2(8, 9);
    [SerializeField] Hexagon hexPrefab;
    [SerializeField] Bomb bombPrefab;
    [Header("Hexagon Customizing")]
    [Space]
    [SerializeField] Color[] colors = { Color.yellow, Color.cyan, Color.blue, Color.red, Color.green};

    private ProcessHandler sliding;
    private bool isSliding = false;
    private int rotateCount = 3;
    private Vector2 offset = new Vector2(0.7f, 0.4f);
    private Dictionary<HexCoordinate, Hexagon> hexagons = new Dictionary<HexCoordinate, Hexagon>();
    private Entry entry;

    public Stack<Hexagon> chosenHexes = new Stack<Hexagon>();
    public Stack<HexCoordinate> emptyGrids = new Stack<HexCoordinate>();

    private bool bombCreated;

    private void Start() 
    {
        sliding = new ProcessHandler();
        InitializeGrids();
        
    }

    private IEnumerator MakeReady()
    {
        yield return new WaitForSeconds(0.2f);
        bool readyToStart = false;
        while (!readyToStart)
        {
            readyToStart = ExplodeAll();
            yield return new WaitForEndOfFrame();
        }

        GameManager.Instance.gameState = GameState.READY;
    }

    private void Update() 
    {
        // if(GameManager.Instance.gameState == GameState.START) 
        // {
        //     StartCoroutine(MakeReady());
        //     return;
        // }

        if (GameManager.Instance.gameState == GameState.READY && (InputManager.Instance.entry == Entry.SWIPE_LEFT || InputManager.Instance.entry == Entry.SWIPE_RIGHT))
            StartRotation(InputManager.Instance.entry);

        if (isSliding)
        {
            //sliding.Status();
            if (emptyGrids.Count > 0)
            {
                if (sliding.start)
                {
                    HexCoordinate[] arr = emptyGrids.ToArray();
                    Array.Sort(arr);
                    emptyGrids = new Stack<HexCoordinate>(arr);
                    sliding.Continue();
                }
                else if (sliding.continuing)
                    Slide();
            }
            else
            {
                if (sliding.end)
                {
                    bool explosion = ExplodeAll();
                    if (explosion)
                        sliding.Reset();
                }
                else
                {
                    bombCreated = false;
                    isSliding = false;
                    sliding.Reset();
                    DisableIdentifiers();
                    GameManager.Instance.gameState = GameState.READY;
                }

            }
        }
    }

    private bool ExplodeAll()
    {
        foreach (var hex in hexagons.Values)
        {
            bool explosion = hex.TryExplode();
            if (explosion)
            {
                sliding.Start();
                return false;
            }
        }
        return true;
    }

    private void Slide()
    {
        var hexCoordinate = emptyGrids.Pop();
        Vector2 coordinate = hexCoordinate.coordinate;
        Vector2 desiredCoordinate = new Vector2(coordinate.x, coordinate.y - 1);
        if (desiredCoordinate.y >= 0)
        {
            var targetCoordinate = GetHexCoordinateByCoordinate(desiredCoordinate);
            if (targetCoordinate != null && !emptyGrids.Contains(targetCoordinate))
            {
                Hexagon hexagon = hexagons[targetCoordinate];
                if (hexagon != null)
                {
                    hexagons[targetCoordinate] = null;
                    hexagons[hexCoordinate] = hexagon;
                    hexagon.HexCoordinate = hexCoordinate;
                    emptyGrids.Push(targetCoordinate);
                    hexagon.transform.position = hexCoordinate.position;
                    // hexagon.Move(hexCoordinate, MoveType.SLIDING);
                    // yield return new WaitForSeconds(0.09f);
                }
            }
        }
        else
        {
            Hexagon hexagon = CreateHexagon(hexCoordinate);
            hexagons[hexCoordinate] = hexagon;
        }

        if (emptyGrids.Count == 0)
            sliding.End();
    }

    public void StartSliding()
    {
        isSliding = true;
        sliding.start = true;
        GameManager.Instance.gameState = GameState.STOP;
    }

    // TODO Chosenhexesi list yap.

    public void Notify()
    {
        List<Hexagon> hexes = new List<Hexagon>(chosenHexes);
        for (var i = 0; i < hexes.Count; i++)
        {
            bool explosion = hexes[i].TryExplode();
            if(explosion)
            {
                ScoreManager.Instance.SetBombs();
                StartSliding();
                return;
            }
        }

        Rotate();
    }

    private void StartRotation(Entry entry)
    {
        this.entry = entry;
        Rotate();
    }

    private void Rotate()
    {
        GameManager.Instance.gameState = GameState.STOP;
        if (chosenHexes.Count > 0 && rotateCount > 0)
        {
            rotateCount--;
            var hexes = Sort(new List<Hexagon>(chosenHexes));
            ChangePositions(entry, hexes);
        }
        else
            EndRotation();

    }

    private void EndRotation()
    {
        entry = Entry.NONE;
        rotateCount = 3;
        GameManager.Instance.gameState = GameState.READY;
    }

    #region ROTATE METHODS

    private void ChangePositions(Entry entry, List<Hexagon> hexes)
    {
        Hexagon top = hexes[0];
        Hexagon right = hexes[1];
        Hexagon left = hexes[2];

        var topCoordinate = top.HexCoordinate;
        var rightCoordinate = right.HexCoordinate;
        var leftCoordinate = left.HexCoordinate;

        if (entry == Entry.SWIPE_RIGHT)
        {
            hexagons[topCoordinate] = left;
            hexagons[rightCoordinate] = top;
            hexagons[leftCoordinate] = right;
        }
        else if (entry == Entry.SWIPE_LEFT)
        {
            hexagons[topCoordinate] = right;
            hexagons[rightCoordinate] = left;
            hexagons[leftCoordinate] = top;
        }

        hexagons[topCoordinate].HexCoordinate = topCoordinate;
        hexagons[rightCoordinate].HexCoordinate = rightCoordinate;
        hexagons[leftCoordinate].HexCoordinate = leftCoordinate;

        hexagons[topCoordinate].Move(topCoordinate, MoveType.ROTATING);
        hexagons[rightCoordinate].Move(rightCoordinate, MoveType.ROTATING);
        hexagons[leftCoordinate].Move(leftCoordinate, MoveType.ROTATING);
    }

    private List<Hexagon> Sort(List<Hexagon> hexes)
    {
        // SORTING RULE --> 0 - TOP, 1 - RIGHT, 2 - LEFT
        List<Hexagon> list = new List<Hexagon>();
        list.Add(null);
        list.Add(null);
        list.Add(null);
        
        Hexagon top;
        top = hexes[0];
        for (var i = 1; i < hexes.Count; i++)
        {
            Vector2 topCoordinate = top.HexCoordinate.coordinate;
            Vector2 coordinate = hexes[i].HexCoordinate.coordinate;

            if (topCoordinate.y > coordinate.y)
                top = hexes[i];
            else if (topCoordinate.y == coordinate.y)
            {
                if (coordinate.x % 2 == 0)
                    top = hexes[i];
            }
        }
        list[0] = top;
        for (var i = 0; i < hexes.Count; i++)
        {
            if (list[0] == hexes[i])
                hexes.Remove(hexes[i]);
        }

        Vector2 coordinate1 = hexes[0].HexCoordinate.coordinate;
        Vector2 coordinate2 = hexes[1].HexCoordinate.coordinate;

        if (coordinate1.x > coordinate2.x)
        {
            list[1] = hexes[0];
            list[2] = hexes[1];
        }
        else
        {
            list[1] = hexes[1];
            list[2] = hexes[0];
        }

        return list;
    }
    #endregion

    #region UTILITIES

    private void DisableIdentifiers()
    {
        while (chosenHexes.Count > 0)
        {
            Hexagon hexagon = chosenHexes.Pop();
            if (hexagon != null)
                hexagon.chosenIdentifier.SetActive(false);
        }

        rotateCount = 3;
    }

    private void InitializeGrids()
    {
        float positionX;
        float positionY;
        Vector2 coordinate;
        Vector2 position;

        for (var x = 0; x < matrix.x; x++)
        {
            positionX = x * 0.7f;
            for (var y = 0; y < matrix.y; y++)
            {
                if (x % 2 == 0)
                    positionY = -0.8f * y;
                else
                    positionY = -0.4f + (-0.8f * y);

                coordinate = new Vector2(x, y);
                position = new Vector2(positionX, positionY);
                var hexCoordinate = new HexCoordinate(coordinate, position);

                Hexagon hexagon = CreateHexagon(hexCoordinate);
                hexagons.Add(hexCoordinate, hexagon);
                
            }
        }
        
    }

    private Hexagon CreateHexagon(HexCoordinate hexCoordinate)
    {
        if(ScoreManager.Instance.Score != 0 && ScoreManager.Instance.Score % 1000 == 0 && !bombCreated)
        {
            Bomb bomb = Instantiate(bombPrefab, transform);
            bomb.Subcribe();
            int number = UnityEngine.Random.Range(0, colors.Length);
            bomb.Initialize(number, hexCoordinate, colors[number], this);
            bombCreated = true;
            return bomb;
        }
        else
        {
            Hexagon hexagon = Instantiate(hexPrefab, transform);
            int number = UnityEngine.Random.Range(0, colors.Length);
            hexagon.Initialize(number, hexCoordinate, colors[number], this);
            return hexagon;
        }
    }

    public HexCoordinate GetHexCoordinateByCoordinate(Vector2 coordinate)
    {
        List<HexCoordinate> coordinates = new List<HexCoordinate>(hexagons.Keys);
        foreach (var hexagon in coordinates)
        {
            if(hexagon.coordinate == coordinate)
                return hexagon;
        }

        return null;
    }

    public Hexagon GetHexagon(Vector2 coordinate)
    {
        Hexagon hexagon;

        var hexCoordinate = GetHexCoordinateByCoordinate(coordinate);
        if (hexCoordinate != null && hexagons.TryGetValue(hexCoordinate, out hexagon))
            return hexagon;
        return null;
    }

    public void DeleteHexagon(Vector2 position)
    {
        List<HexCoordinate> coordinates = new List<HexCoordinate>(hexagons.Keys);
        for (var i = 0; i < hexagons.Count; i++)
        {
            if (coordinates[i].position == position)
            {
                hexagons[coordinates[i]] = null;
                emptyGrids.Push(coordinates[i]);
                return;
            }
        }

        throw new Exception("Silme işleminde hexagon yok!");
    }
    #endregion

}