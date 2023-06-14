using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Actions/RefuelOtherShip")]
public class RefuelOtherShip : Action
{

    public override void Activate(ShipUnit thisShip, List<ShipUnit> targets, List<Vector3Int> positions, List<Orientation> orientations, int customParam)
    {
        base.Activate(thisShip, targets, positions, orientations, customParam);

        if (targets.Count != 1)
        {
            Debug.LogError(thisShip.name + " is trying to use the refuel a wrong amount of targets with the action: " + this.name);
            return;
        }

        ShipUnit target = targets[0];

        int fuelAmount = customParam;

        //TODO show animation of fuel
        thisShip.RemoveFuel(fuelAmount);
        target.AddFuel(fuelAmount);
        Debug.Log(thisShip + " refueled " + target + " with " + fuelAmount + " fuel");
       
    }

    public override int GetMinAmountForCustomParam(ShipUnit thisShip, List<ShipUnit> targets, List<Vector3Int> vec3List, List<Orientation> orientations)
    {
        return 1;
    }

    public override int GetMaxAmountForCustomParam(ShipUnit thisShip, List<ShipUnit> targets, List<Vector3Int> vec3List, List<Orientation> orientations)
    {
        if (targets.Count > 1)
        {
            Debug.LogError(thisShip.name + " is trying to use the refuel a wrong amount of targets with the action: " + this.name);
            return 0;
        }

        ShipUnit target = targets[0];

        return Mathf.Min(thisShip.GetCurrentFuel(), target.GetMaxFuel() - target.GetCurrentFuel());
    }

}
