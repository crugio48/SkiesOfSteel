using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public override bool Activate(ShipUnit thisShip, List<ShipUnit> targets, List<Vector3Int> positions, List<Orientation> orientations, int customParam)
    {
        if (base.Activate(thisShip, targets, positions, orientations, customParam) == false) return false;


        foreach (ShipUnit target in targets)
        {
            if (AccuracyHit(accuracy))
            {
                if (modifyAttack)
                    target.ModifyAttack(attackModification, isAttackOneTurnTemporary);

                if (modifyDefense)
                    target.ModifyDefense(defenseModification, isDefenseOneTurnTemporary);
                

                target.PlayAnimationClientRpc(AnimationToShow.HIT_STATS_CHANGE, thisShip.GetCurrentPosition());
                Debug.Log(thisShip.name + " modified the stats of " + target.name);
            }
            else
            {
                target.PlayAnimationClientRpc(AnimationToShow.MISSED_STATS_CHANGE, thisShip.GetCurrentPosition());
                Debug.Log(thisShip.name + " missed the roll to modify the stats of " + target.name);
            }
        }

        return true;
    }
}
