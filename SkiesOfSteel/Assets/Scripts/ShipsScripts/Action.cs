using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action : ScriptableObject
{
    public new string name;
    public Sprite sprite;
    public int fuelCost;

    public virtual void Activate(ShipUnit thisShip, List<ShipUnit> targets, int customParam)
    {
        if (thisShip.GetCurrentFuel() < fuelCost)
        {
            Debug.Log("Trying to use an action that cost more fuel than current amount " + thisShip.name);
            return;
        }
        else
        {
            thisShip.RemoveFuel(fuelCost);
        }

    }


    public bool AccuracyHit(int accuracy)
    {
        int roll = Random.Range(0, 100);     // creates a number between 0 and 99

        if (roll < accuracy)
            return true;
        else
            return false;
    }

}
