using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShipGraphics", menuName = "ScriptableObjects/ShipGraphics")]
public class ShipGraphics : ScriptableObject
{
    public Sprite[] _Engine = new Sprite[6];
    public Sprite[] _NoEngine = new Sprite[6];

    public Sprite GetSprite(int direction, bool engines)
    {
        return engines ? _Engine[(direction - 1)] : _NoEngine[(direction - 1)];
    }
}