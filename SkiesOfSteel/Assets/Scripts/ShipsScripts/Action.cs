
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;


public class Action : ScriptableObject
{
    public new string name;
    public string description;
    public Sprite sprite;
    public int fuelCost;
    public int range;

    [Space]
    public bool needsTarget;
    public bool needsLineOfSight;
    public bool isSelfOnly;
    public bool canTargetSelf;
    public int amountOfTargets;

    [Space]
    public bool isTargetAnArea;
    public bool isAffectingOnlyEmptyPositions;
    public Shape shape;

    [Space]
    public bool needsCustomParameter;
    public string stringToDisplayWhenAskingForCustomParam;


    public virtual bool Activate(ShipUnit thisShip, List<ShipUnit> targets, List<Vector3Int> positions, List<Orientation> orientations, int customParam)
    {
        // Checking parameters existence
        if (thisShip == null || targets == null || positions == null || orientations == null)
        {
            Debug.LogError(thisShip.name + " is trying to use this action:" + this.name + " not in the intended way");
            return false;
        }

        // Server side checks on action activation
        if (!HasEnoughFuel(thisShip) || !IsRangeRespected(thisShip, targets, positions, orientations) || !IsCustomParamRangeRespected(thisShip, targets, positions, orientations, customParam))
        {
            Debug.LogError(thisShip.name + " is trying to use this action:" + this.name + " not in the intended way");
            return false;
        }

        return true;
    }

    public virtual int GetMinAmountForCustomParam(ShipUnit thisShip, List<ShipUnit> targets, List<Vector3Int> vec3List, List<Orientation> orientations) { return 0; }
    public virtual int GetMaxAmountForCustomParam(ShipUnit thisShip, List<ShipUnit> targets, List<Vector3Int> vec3List, List<Orientation> orientations) { return 0; }


    protected bool AccuracyHit(int accuracy)
    {
        int roll = UnityEngine.Random.Range(0, 100);     // creates a number between 0 and 99

        if (roll < accuracy)
            return true;
        else
            return false;
    }

    //---------------------------------------------------------------- Public checks


    public bool HasEnoughFuel(ShipUnit thisShip)
    {
        return thisShip.GetCurrentFuel() >= fuelCost;
    }


    private bool IsRangeRespected(ShipUnit thisShip, List<ShipUnit> targets, List<Vector3Int> vec3List, List<Orientation> orientations)
    {
        if (!needsTarget) return true;

        if (isTargetAnArea)
        {
            return TargetAreaCheck(thisShip, vec3List, orientations);
        }
        else
        {
            return NonTargetAreaCheck(thisShip, targets);
        }
    }



    public bool IsSingleRangeRespected(ShipUnit thisShip, Vector3Int pos)
    {
        return Node.HexManhattanDistance(thisShip.GetCurrentPosition(), pos) <= range;
    }


    public bool HasLineOfSight(ShipUnit thisShip, Vector3Int pos)
    {
        return Pathfinding.Instance.IsThereLineOfSight(thisShip.GetCurrentPosition(), pos);
    }


    public bool IsCustomParamRangeRespected(ShipUnit thisShip, List<ShipUnit> targets, List<Vector3Int> vec3List, List<Orientation> orientations, int customParam)
    {
        if (!needsCustomParameter) return true;

        if (customParam > GetMaxAmountForCustomParam(thisShip, targets, vec3List, orientations)) return false;

        if (customParam < GetMinAmountForCustomParam(thisShip, targets, vec3List, orientations)) return false;

        return true;
    }


    //---------------------------------------------------------------- Private only checks
    private bool TargetAreaCheck(ShipUnit thisShip, List<Vector3Int> targetPositions, List<Orientation> orientations)
    {
        if (targetPositions.Count > amountOfTargets) return false;

        if (targetPositions.Count != orientations.Count) return false;

        for (int i = 0; i < targetPositions.Count; i++)
        {
            if(!IsSingleRangeRespected(thisShip, targetPositions[i])) return false;

            if (needsLineOfSight && !HasLineOfSight(thisShip, targetPositions[i])) return false;

            if (!canTargetSelf && targetPositions[i] == thisShip.GetCurrentPosition()) return false;

            if (isSelfOnly && targetPositions[i] != thisShip.GetCurrentPosition()) return false;

            if (!IsOrientationValueAdmissible((int) orientations[i])) return false;
        }


        return AreTargetsOfAreaCorrect(targetPositions, orientations);
    }

    private bool AreTargetsOfAreaCorrect(List<Vector3Int> targetPositions, List<Orientation> orientations)
    {
        if (!isAffectingOnlyEmptyPositions) return true;

        // If the action is used to target only empty positions in the grid then we have to check that the target ship list is empty in those positions

        for (int i = 0; i < targetPositions.Count; i++)
        {
            foreach (Vector3Int pos in ShapeLogic.Instance.GetPositionsInThisShape(shape, orientations[i], targetPositions[i]))
            {
                if (ShipsPositions.Instance.IsThereAShip(pos)) return false;
            }
        }

        return true;
    
    }



    private bool NonTargetAreaCheck(ShipUnit thisShip, List<ShipUnit> targets)
    {
        if (targets.Count > amountOfTargets) return false;

        foreach (ShipUnit target in targets)
        {
            if (!IsSingleRangeRespected(thisShip, target.GetCurrentPosition())) return false;

            if (needsLineOfSight && !HasLineOfSight(thisShip, target.GetCurrentPosition())) return false;

            if (!canTargetSelf && target == thisShip) return false;

            if (isSelfOnly && target != thisShip) return false;
        }

        return true;
    }


    private bool IsOrientationValueAdmissible(int value)
    {
        if (value < Enum.GetNames(typeof(Orientation)).Length && value >= 0) return true;

        return false;
    }
}