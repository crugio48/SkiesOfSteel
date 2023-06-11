using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Actions/AreaAttack")]
public class AreaAttack : Action
{
    public int power;
    public int range;
    public Shape shape;

    [Range(1, 100)]
    public int accuracy;



    public override void Activate(ShipUnit thisShip, List<ShipUnit> targets, int customParam)
    {
        base.Activate(thisShip, targets, customParam);

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


public enum Shape
{
    TRIANGLE
}
