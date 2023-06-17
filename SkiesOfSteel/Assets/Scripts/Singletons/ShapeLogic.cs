using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public enum Shape
{
    NONE,
    TRIANGLE
}

public enum Orientation
{
    TOP = 0,
    TOP_RIGHT = 1,
    BOTTOM_RIGHT = 2,
    BOTTOM = 3,
    BOTTOM_LEFT = 4,
    TOP_LEFT = 5,
}



public class ShapeLogic : Singleton<ShapeLogic>
{
    public List<Vector3Int> GetPositionsInThisShape(Shape shape, Orientation shapeOrientation, Vector3Int position)
    {
        return shape switch
        {
            Shape.TRIANGLE => TrianglePositions(shapeOrientation, position),


            Shape.NONE => new List<Vector3Int>(),

            _ => new List<Vector3Int>(),
        };
    }


    public List<ShipUnit> GetShipsInThisShape(Shape shape, Orientation shapeOrientation, Vector3Int position)
    {
        List<ShipUnit> ships = new List<ShipUnit>();

        foreach (Vector3Int pos in GetPositionsInThisShape(shape, shapeOrientation, position))
        {
            if (ShipsPositions.Instance.IsThereAShip(pos)) ships.Add(ShipsPositions.Instance.GetShip(pos));
        }

        return ships;
    }

    public Orientation GetNextClockwiseOrientation(Orientation orientation)
    {
        return (Orientation)(((int)orientation + 1) % 6);
    }

    public Orientation ComputeDirection(Vector3Int from, Vector3Int to)
    {
        Vector3Int diff = to - from;

        if (diff.y == 0) return diff.x > 0 ? Orientation.TOP : Orientation.BOTTOM;

        else if (diff.y < 0) return diff.x < (from.y & 1) ? Orientation.BOTTOM_LEFT : Orientation.TOP_LEFT;

        else return diff.x < (from.y & 1) ? Orientation.BOTTOM_RIGHT : Orientation.TOP_RIGHT;
    }


    // Get list of positions under this triangle shape
    private List<Vector3Int> TrianglePositions(Orientation shapeOrientation, Vector3Int position)
    {
        Orientation otherDirection = GetNextClockwiseOrientation(shapeOrientation);

        return new List<Vector3Int>
        {
            position,
            ShapeHelper.GetAdjacentGridPositionInDirection(position, shapeOrientation),
            ShapeHelper.GetAdjacentGridPositionInDirection(position, otherDirection)
        };
    }
}



public class ShapeHelper
{

    public static Vector3Int GetAdjacentGridPositionInDirection(Vector3Int pos, Orientation direction)
    {
        int parity = pos.y & 1; // parity = 0 means we are on an even column, parity = 1 means we are on an odd column

        if (parity == 0)
        {
            return direction switch
            {
                Orientation.TOP => pos + new Vector3Int(1, 0, 0),
                Orientation.TOP_RIGHT => pos + new Vector3Int(0, 1, 0),
                Orientation.BOTTOM_RIGHT => pos + new Vector3Int(-1, 1, 0),
                Orientation.BOTTOM => pos + new Vector3Int(-1, 0, 0),
                Orientation.BOTTOM_LEFT => pos + new Vector3Int(-1, -1, 0),
                Orientation.TOP_LEFT => pos + new Vector3Int(0, -1, 0),

                _ => pos, // Should never return this
            };
        }

        else
        {
            return direction switch
            {
                Orientation.TOP => pos + new Vector3Int(1, 0, 0),
                Orientation.TOP_RIGHT => pos + new Vector3Int(1, 1, 0),
                Orientation.BOTTOM_RIGHT => pos + new Vector3Int(0, 1, 0),
                Orientation.BOTTOM => pos + new Vector3Int(-1, 0, 0),
                Orientation.BOTTOM_LEFT => pos + new Vector3Int(0, -1, 0),
                Orientation.TOP_LEFT => pos + new Vector3Int(1, -1, 0),

                _ => pos, // Should never return this
            };
        }
    }
}



