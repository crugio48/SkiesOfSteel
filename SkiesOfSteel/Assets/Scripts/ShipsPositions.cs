using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipsPositions : MonoBehaviour
{
    // Make Singleton of this class
    public static ShipsPositions instance = null;

    private Dictionary<Vector3Int, ShipUnit> shipsPositions;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            shipsPositions = new Dictionary<Vector3Int, ShipUnit>();
        }
        else if (instance != this)
        {
            Debug.Log(this.name + ": There shouldn't be another instance of me");
            Destroy(gameObject);
        }
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

}
