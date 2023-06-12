using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "ScriptableObjects/Actions/SingleAttack")]
public class SingleAttack : Action
{
    [Space]
    public int power;

    [Range(1, 100)]
    public int accuracy;

    public override void Activate(ShipUnit thisShip, List<ShipUnit> targets, int customParam)
    {
        base.Activate(thisShip, targets, customParam);

        if (targets.Count > 1)
        {
            Debug.Log(thisShip.name + " Is trying to attack too many ships with the action: " + this.name);
            return;
        }

        ShipUnit target = targets[0];


        if (AccuracyHit(accuracy))
        {
            //TODO show animation of attack
            target.TakeHit(thisShip, power);

            Debug.Log(thisShip + " hit " + target);
        }
        else
        {
            //TODO show animation of miss
            Debug.Log(thisShip + " missed " + target);
        }
    }
}
