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

    public override bool Activate(ShipUnit thisShip, List<ShipUnit> targets, List<Vector3Int> positions, List<Orientation> orientations, int customParam)
    {
        if (base.Activate(thisShip, targets, positions, orientations, customParam) == false) return false;

        // If target is an area then we need to calculate the targets given the shape and the positions and orientations
        if (isTargetAnArea)
        {
            targets.Clear();

            for (int i = 0; i < positions.Count; i++)
            {
                foreach (ShipUnit ship in ShapeLogic.Instance.GetShipsInThisShape(shape, orientations[i], positions[i]))
                {
                    if (!targets.Contains(ship)) targets.Add(ship);
                }
            }
        }

        foreach (ShipUnit target in targets)
        {
            if (AccuracyHit(accuracy))
            {
                target.TakeHit(thisShip, power);

                target.PlayAnimationClientRpc(AnimationToShow.HIT_ATTACK, thisShip.GetCurrentPosition());
                Debug.Log(thisShip.name + " hit " + target.name);
            }
            else
            {
                target.PlayAnimationClientRpc(AnimationToShow.MISSED_ATTACK, thisShip.GetCurrentPosition());
                Debug.Log(thisShip.name + " missed " + target.name);
            }
        }


        return true;
    }
}
