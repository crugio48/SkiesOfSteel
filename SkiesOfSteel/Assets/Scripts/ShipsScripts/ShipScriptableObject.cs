using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="ShipScriptableObject", menuName="ScriptableObjects/Ship")]
public class ShipScriptableObject : ScriptableObject
{
    // TODO add name
    public bool isFlagship;

    public int maxHealth;
    public int maxFuel;
    public int speed;
    public int attack;
    public int defense;
    public int battleCost;

    public ShipGraphics graphics;


    public List<Action> actionList;
}
