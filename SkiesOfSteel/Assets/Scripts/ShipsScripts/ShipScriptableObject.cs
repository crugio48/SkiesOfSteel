using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="ShipScriptableObject", menuName="ScriptableObjects/Ship1")]
public class ShipScriptableObject : ScriptableObject
{
    public int health;
    public int speed;
    //sprites
    //current tile position

    public int attackDamage;
    public int attackRange;

    public float maxFuel;

}
