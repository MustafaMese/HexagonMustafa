using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum MoveType { ROTATING, SLIDING }
public enum HexType { NORMAL, BOMB }

public class Hexagon : MonoBehaviour
{
    private Vector2[] dirOdd_q = { new Vector2(0, -1), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1), new Vector2(-1, 1), new Vector2(-1, 0) };
    private Vector2[] dirEven_q = { new Vector2(0, -1), new Vector2(1, -1), new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0), new Vector2(-1, -1) };

    [SerializeField] SpriteRenderer spriteRenderer;
    public GameObject chosenIdentifier;
    public HexType hexType;

    private int id;
    private HexGrid hexGrid;
    private HexCoordinate hexCoordinate;

    public HexCoordinate HexCoordinate { get => hexCoordinate; set => hexCoordinate = value; }

    private void Start()
    {
        DOTween.Init();
        hexType = HexType.NORMAL;
    }

    private void Update() 
    {
        if(InputManager.Instance.entry == Entry.NONE) return;

        if (Input.touchCount > 0)
        {
            foreach (Touch t in Input.touches)
            {
                Vector3 touchPos = t.position;
                if (t.phase == TouchPhase.Began)
                {
                    Ray ray = Camera.main.ScreenPointToRay(touchPos);
                    RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
                    if (hit.collider != null && hit.transform == transform)
                    {
                        Group();
                        break;
                    }
                }
            }
        }
    }

    public void Initialize(int id, HexCoordinate hexCoordinate, Color color, HexGrid hexGrid)
    {
        this.id = id;
        this.hexGrid = hexGrid;
        this.HexCoordinate = hexCoordinate;
        transform.position = hexCoordinate.position;

        ChangeColor(color);
    }

    private void ChangeColor(Color color)
    {
        spriteRenderer.color = color;
    }

    private void Notify(MoveType moveType)
    {
        if(moveType == MoveType.ROTATING)
        {
            DOTween.KillAll();

            List<Hexagon> hexes = new List<Hexagon>(hexGrid.chosenHexes);
            for (var i = 0; i < hexes.Count; i++)
                hexes[i].transform.position = hexes[i].hexCoordinate.position;

            hexGrid.Notify();
        }
        else
            hexGrid.StartSliding();
    }

    #region MOVE METHODS

    public void Move(HexCoordinate hexCoordinate, MoveType moveType)
    {
        transform.DOKill();

        if(moveType == MoveType.ROTATING)
            transform.DOMove(hexCoordinate.position, 0.5f).SetEase(Ease.OutQuart).OnComplete(() => Notify(MoveType.ROTATING));
        else
            transform.DOMove(hexCoordinate.position, 0.11f).OnComplete(() => Notify(MoveType.SLIDING));
    }

    #endregion

    #region GROUPING METHODS

    private void Group()
    {
        DisableIdentifiers();

        Vector2 coordinate = HexCoordinate.coordinate;
        Vector2 targetCoordinate, targetCoordinate2;

        if(coordinate.x % 2 == 0)
            IdentifyCoordinate(coordinate, dirEven_q, out targetCoordinate, out targetCoordinate2);
        else
            IdentifyCoordinate(coordinate, dirOdd_q, out targetCoordinate, out targetCoordinate2);

        if(targetCoordinate.x > -1 && targetCoordinate.y > -1 && targetCoordinate2.x > -1 && targetCoordinate2.y > -1)
            EnableIdentifiers(targetCoordinate, targetCoordinate2);
    }

    private void DisableIdentifiers()
    {
        while (hexGrid.chosenHexes.Count > 0)
        {
            Hexagon hexagon = hexGrid.chosenHexes.Pop();
            if(hexagon != null)
                hexagon.chosenIdentifier.SetActive(false);
        }
    }

    private void EnableIdentifiers(Vector2 targetCoordinate, Vector2 targetCoordinate2)
    {
        Hexagon targetHexagon = hexGrid.GetHexagon(targetCoordinate);
        Hexagon targetHexagon2 = hexGrid.GetHexagon(targetCoordinate2);

        chosenIdentifier.SetActive(true);
        targetHexagon.chosenIdentifier.SetActive(true);
        targetHexagon2.chosenIdentifier.SetActive(true);

        hexGrid.chosenHexes.Push(this);
        hexGrid.chosenHexes.Push(targetHexagon);
        hexGrid.chosenHexes.Push(targetHexagon2);
    }

    private void IdentifyCoordinate(Vector2 coordinate, Vector2[] direction, out Vector2 targetCoordinate, out Vector2 targetCoordinate1)
    {
        int number;
        int[] randDirections;
        int counter = 0;

        targetCoordinate = targetCoordinate1 = new Vector2(-1, -1);

        while(counter < 5)
        {
            number = Random.Range(0, direction.Length);
            targetCoordinate = new Vector2(coordinate.x + direction[number].x, coordinate.y + direction[number].y);
            if(targetCoordinate.x < 0 || targetCoordinate.y < 0)
            {
                counter++;
                continue;
            }
            if (number == 0)
            {
                randDirections = new int[] { 1, 5 };
                number = Random.Range(0, randDirections.Length);
                targetCoordinate1 = new Vector2(coordinate.x + direction[randDirections[number]].x, coordinate.y + direction[randDirections[number]].y);
            }
            else if (number == 5)
            {
                randDirections = new int[] { 0, 4 };
                number = Random.Range(0, randDirections.Length);
                targetCoordinate1 = new Vector2(coordinate.x + direction[randDirections[number]].x, coordinate.y + direction[randDirections[number]].y);
            }
            else
            {
                randDirections = new int[] { number - 1, number + 1 };
                number = Random.Range(0, randDirections.Length);
                targetCoordinate1 = new Vector2(coordinate.x + direction[randDirections[number]].x, coordinate.y + direction[randDirections[number]].y);
            }
            if (targetCoordinate1.x < 0 || targetCoordinate1.y < 0)
                counter++;
            else if(hexGrid.GetHexCoordinateByCoordinate(targetCoordinate) != null && hexGrid.GetHexCoordinateByCoordinate(targetCoordinate1) != null)
                break;
        }
        
    }

    #endregion

    #region EXPLODE METHODS

    public bool TryExplode()
    {
        Vector2 coordinate = HexCoordinate.coordinate;
        Vector2 targetCoordinate, targetCoordinate1 = -1 * Vector2.one, targetCoordinate2 = -1 * Vector2.one;
        Hexagon targetHexagon, targetHexagon1 = null, targetHexagon2 = null;

        if(coordinate.x % 2 == 0)
        {
            for (var counter = 0; counter < dirEven_q.Length; counter++)
            {
                targetCoordinate = new Vector2(coordinate.x + dirEven_q[counter].x, coordinate.y + dirEven_q[counter].y);
                targetHexagon = hexGrid.GetHexagon(targetCoordinate);
                if (targetHexagon != null && targetHexagon.id == id)
                {
                    Assign(coordinate, dirEven_q, out targetCoordinate1, out targetCoordinate2, out targetHexagon1, out targetHexagon2, counter);

                    if (targetHexagon1 != null && targetHexagon1.id == targetHexagon.id)
                    {
                        Explode(targetHexagon, targetHexagon1);
                        return true;
                    }
                    else if (targetHexagon2 != null && targetHexagon2.id == targetHexagon.id)
                    {
                        Explode(targetHexagon, targetHexagon2);
                        return true;
                    }
                }
            }

            return false;
        }
        else 
        {
            for (var i = 0; i < dirOdd_q.Length; i++)
            {
                targetCoordinate = new Vector2(coordinate.x + dirOdd_q[i].x, coordinate.y + dirOdd_q[i].y);
                targetHexagon = hexGrid.GetHexagon(targetCoordinate);
                if (targetHexagon != null && targetHexagon.id == id)
                {
                    Assign(coordinate, dirOdd_q, out targetCoordinate1, out targetCoordinate2, out targetHexagon1, out targetHexagon2, i);

                    if (targetHexagon1 != null && targetHexagon1.id == targetHexagon.id)
                    {
                        Explode(targetHexagon, targetHexagon1);
                        return true;
                    }
                    else if (targetHexagon2 != null && targetHexagon2.id == targetHexagon.id)
                    {
                        Explode(targetHexagon, targetHexagon2);
                        return true;
                    }
                }
            }

            return false;
        }
    }

    private void Explode(Hexagon targetHexagon, Hexagon targetHexagon1)
    {
        if(hexType == HexType.BOMB)    
        {
            Bomb bomb = (Bomb)this;
            bomb.Unsubscribe();
            ScoreManager.Instance.IncreaseExplodedBombCount();
        }
        if (targetHexagon.hexType == HexType.BOMB)
        {
            Bomb bomb = (Bomb)targetHexagon;
            bomb.Unsubscribe();
            ScoreManager.Instance.IncreaseExplodedBombCount();
        }
        if (targetHexagon1.hexType == HexType.BOMB)
        {
            Bomb bomb = (Bomb)targetHexagon1;
            bomb.Unsubscribe();
            ScoreManager.Instance.IncreaseExplodedBombCount();
        }

        ScoreManager.Instance.IncreaseScore(15);
        ScoreManager.Instance.IncreaseExplodedHexCount();

        hexGrid.DeleteHexagon(transform.position);
        hexGrid.DeleteHexagon(targetHexagon.transform.position);
        hexGrid.DeleteHexagon(targetHexagon1.transform.position);
        Destroy(this.gameObject);
        Destroy(targetHexagon.gameObject);
        Destroy(targetHexagon1.gameObject);
    }

    private void Assign(Vector2 coordinate, Vector2[] direction, out Vector2 targetCoordinate1, out Vector2 targetCoordinate2, out Hexagon targetHexagon1, out Hexagon targetHexagon2, int counter)
    {
        if (counter == 0)
        {
            targetCoordinate1 = new Vector2(coordinate.x + direction[1].x, coordinate.y + direction[1].y);
            targetCoordinate2 = new Vector2(coordinate.x + direction[5].x, coordinate.y + direction[5].y);

            targetHexagon1 = hexGrid.GetHexagon(targetCoordinate1);
            targetHexagon2 = hexGrid.GetHexagon(targetCoordinate2);
        }
        else if (counter == 5)
        {
            targetCoordinate1 = new Vector2(coordinate.x + direction[0].x, coordinate.y + direction[0].y);
            targetCoordinate2 = new Vector2(coordinate.x + direction[4].x, coordinate.y + direction[4].y);

            targetHexagon1 = hexGrid.GetHexagon(targetCoordinate1);
            targetHexagon2 = hexGrid.GetHexagon(targetCoordinate2);
        }
        else
        {
            targetCoordinate1 = new Vector2(coordinate.x + direction[counter - 1].x, coordinate.y + direction[counter - 1].y);
            targetCoordinate2 = new Vector2(coordinate.x + direction[counter + 1].x, coordinate.y + direction[counter + 1].y);

            targetHexagon1 = hexGrid.GetHexagon(targetCoordinate1);
            targetHexagon2 = hexGrid.GetHexagon(targetCoordinate2);
        }
    }

    #endregion
    
}
