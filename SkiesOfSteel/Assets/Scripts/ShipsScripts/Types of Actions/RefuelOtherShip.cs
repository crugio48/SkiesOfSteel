using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Actions/RefuelOtherShip")]
public class RefuelOtherShip : Action
{
    public int range;

    public override void Activate(ShipUnit thisShip)
    {
        base.Activate(thisShip);

        ShipUnit target = new ShipUnit() /*TODO select target of refuel given the range value*/;


        int fuelAmount = 0 /*TODO select amount of fuel to give to target ship*/;


        //TODO show animation of fuel
        thisShip.RemoveFuel(fuelAmount);
        target.AddFuel(fuelAmount);
        Debug.Log(thisShip + " refueled " + target + " with " + fuelAmount + " fuel");
       
    }
}
