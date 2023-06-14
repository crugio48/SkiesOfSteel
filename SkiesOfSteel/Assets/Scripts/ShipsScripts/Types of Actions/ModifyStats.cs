using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(menuName = "ScriptableObjects/Actions/ModifyStats")]
public class ModifyStats : Action
{
    [Space]
    public bool modifyAttack;
    public bool isAttackOneTurnTemporary;
    [Range(-6, 6)]
    public int attackModification;

    public bool modifyDefense;
    public bool isDefenseOneTurnTemporary;
    [Range(-6, 6)]
    public int defenseModification;

    [Range(1, 100)]
    public int accuracy;

    public override void Activate(ShipUnit thisShip, List<ShipUnit> targets, Vector3Int vec3, int customParam)
    {
        base.Activate(thisShip, targets, vec3, customParam);

        foreach (ShipUnit target in targets)
        {
            if (AccuracyHit(accuracy))
            {
                if (modifyAttack)
                    target.ModifyAttack(attackModification, isAttackOneTurnTemporary);

                if (modifyDefense)
                    target.ModifyDefense(defenseModification, isDefenseOneTurnTemporary);


                Debug.Log(thisShip.name + " modified the stats of " + target.name);
            }
            else
            {
                Debug.Log(thisShip.name + " missed the roll to modify the stats of " + target.name);
            }
        }
    }
}
