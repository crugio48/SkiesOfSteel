using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Actions/ModifyStats")]
public class ModifyStats : Action
{
    public bool selfOnly;
    public int range;

    public bool modifyAttack;
    public int attackModification;

    public bool modifyDefense;
    public int defenseModification;

    [Range(1, 100)]
    public int accuracy;

    public override void Activate(ShipUnit thisShip)
    {
        base.Activate(thisShip);

        ShipUnit target = thisShip;
        if (!selfOnly)
        {
            /*TODO select target of attack given the range value*/
        }


        if (AccuracyHit(accuracy))
        {
            if (modifyAttack)
                target.ModifyAttack(attackModification);

            if (modifyDefense)
                target.ModifyDefense(defenseModification);
        }

    }
}
