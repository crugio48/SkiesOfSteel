using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShipGraphics", menuName = "ScriptableObjects/ShipGraphics")]
public class ShipGraphics : ScriptableObject
{
    public Sprite[] _Engine = new Sprite[6];
    public Sprite[] _NoEngine = new Sprite[6];

    public Sprite GetSprite(Orientation direction, bool engines)
    {
        return engines ? _Engine[(int)direction] : _NoEngine[(int)direction];
    }
}