using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="ShipScriptableObject", menuName="ScriptableObjects/Ship1")]
public class ShipScriptableObject : ScriptableObject
{
    public int maxHealth;
    public int maxFuel;
    public int speed;
    public int attack;
    public int defense;
    public int battleCost;

    // Add more sprites for the different angles and add logic to switch between them in ShipUnit TODO
    public Sprite sprite;

    //Actions TODO
    public List<Attack> actions;
}
