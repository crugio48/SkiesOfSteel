using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "ScriptableObjects/Actions/SingleAttack")]
public class SingleAttack : Action
{
    public int power;
    public int range;

    [Range(1, 100)]
    public int accuracy;

    public override void Activate(ShipUnit thisShip)
    {
        base.Activate(thisShip);


        ShipUnit enemyShip = new ShipUnit() /*TODO select target of attack given the range value*/;

        if (AccuracyHit(accuracy))
        {
            //TODO show animation of attack
            enemyShip.TakeHit(thisShip, power);
            Debug.Log(thisShip + " hit " + enemyShip);
        }
        else
        {
            //TODO show animation of miss
            Debug.Log(thisShip + " missed " + enemyShip);
        }
    }
}
