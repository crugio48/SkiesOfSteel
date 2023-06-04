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



    public override void Activate(ShipUnit thisShip)
    {
        base.Activate(thisShip);

        List<ShipUnit> enemyShips = new List<ShipUnit>()/*TODO select target of attack and rotation given the range value*/;


        foreach (ShipUnit enemy in enemyShips)
        {
            if (AccuracyHit(accuracy))
            {
                //TODO show animation of attack
                enemy.TakeHit(thisShip, power);
                Debug.Log(thisShip + " hit " + enemy);
            }
            else
            {
                //TODO show animation of miss
                Debug.Log(thisShip + " missed " + enemy);
            }
        }
    }
}


public enum Shape
{
    TRIANGLE
}
