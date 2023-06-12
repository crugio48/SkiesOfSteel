using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipsPositions : Singleton<ShipsPositions>
{
    private Dictionary<Vector3Int, ShipUnit> shipsPositions;


    private void Start()
    {
        shipsPositions = new Dictionary<Vector3Int, ShipUnit>();
    }


    public void Place(ShipUnit ship, Vector3Int position)
    {
        if (shipsPositions.ContainsKey(position))
        {
            Debug.LogError("Ship already present in position: " + position);
            return;
        }

        // We passed all the checks

        shipsPositions.Add(position, ship);
        Debug.Log("Placing ship in position");
        
    }


    public void Move(ShipUnit ship, Vector3Int from, Vector3Int to)
    {
        if (shipsPositions.ContainsKey(to))
        {
            Debug.LogError("Ship already present in position: " + to);
            return;
        }


        if (!shipsPositions.ContainsKey(from) || ship != shipsPositions[from])
        {
            Debug.LogError("Trying to move the wrong ship from position: " + from);
            return;
        }

        // We passed all the checks

        shipsPositions.Remove(from);
        shipsPositions.Add(to, ship);
    }



    public ShipUnit GetShip(Vector3Int position)
    {
        if (shipsPositions.ContainsKey(position))
        {
            return shipsPositions[position];
        }
        else
        {
            return null;
        }
    }


    public void RemoveShip(Vector3Int position)
    {
        shipsPositions.Remove(position);
    }

}
