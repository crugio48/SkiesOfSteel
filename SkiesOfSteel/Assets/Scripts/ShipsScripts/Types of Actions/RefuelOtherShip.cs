using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Actions/RefuelOtherShip")]
public class RefuelOtherShip : Action
{
    public int range;

    public override void Activate(ShipUnit thisShip, List<ShipUnit> targets, int customParam)
    {
        base.Activate(thisShip, targets, customParam);

        if (targets.Count > 1)
        {
            Debug.Log(thisShip.name + " Is trying to refuel too many ships with the action: " + this.name);
            return;
        }

        ShipUnit target = targets[0];

        int fuelAmount = customParam;

        //TODO show animation of fuel
        thisShip.RemoveFuel(fuelAmount);
        target.AddFuel(fuelAmount);
        Debug.Log(thisShip + " refueled " + target + " with " + fuelAmount + " fuel");
       
    }

    public override int GetMinAmountForCustomParam(ShipUnit thisShip, List<ShipUnit> targets)
    {
        return 0;
    
    }
    public override int GetMaxAmountForCustomParam(ShipUnit thisShip, List<ShipUnit> targets)
    {
        if (targets.Count > 1)
        {
            Debug.Log(thisShip.name + " Is trying to refuel too many ships with the action: " + this.name);
            return 0;
        }

        ShipUnit target = targets[0];

        return Mathf.Min(thisShip.GetCurrentFuel(), target.GetMaxFuel() - target.GetCurrentFuel());
    }

}
