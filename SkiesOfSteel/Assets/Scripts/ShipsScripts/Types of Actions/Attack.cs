using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Actions/Attack")]
public class Attack : Action
{
    [Space]
    public int power;

    [Range(1, 100)]
    public int accuracy;

    public override void Activate(ShipUnit thisShip, List<ShipUnit> targets, Vector3Int vec3, int customParam)
    {
        base.Activate(thisShip, targets, vec3, customParam);

        foreach (ShipUnit target in targets)
        {
            if (AccuracyHit(accuracy))
            {
                //TODO show animation of attack
                target.TakeHit(thisShip, power);
                Debug.Log(thisShip.name + " hit " + target.name);
            }
            else
            {
                //TODO show animation of miss
                Debug.Log(thisShip.name + " missed " + target.name);
            }
        }
    }
}
