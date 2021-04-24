using System;
using System.Collections.Generic;
using UnityEngine;

public class HexCoordinate : IComparable
{
    public Vector2 coordinate;
    public Vector2 position;

    public HexCoordinate(Vector2 coordinate, Vector2 position)
    {
        this.coordinate = coordinate;
        this.position = position;
    }

    public int CompareTo(object obj)
    {
        HexCoordinate other = obj as HexCoordinate;

        Vector2 cOther = other.coordinate;
        if (coordinate.x == cOther.x)
            return 0;
        else if(cOther.y == coordinate.y)
        {
            if(coordinate.x % 2 == 0)
                return 1;
            else
                return -1;
        }
        else if(coordinate.y > cOther.y)
            return -1;
        else
            return 1;
    
    }
}
