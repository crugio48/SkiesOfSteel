using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GachaPull", menuName = "ScriptableObjects/GachaItem")]
public class GachaPull : ScriptableObject
{
    public int stars = 3;
    public Sprite image;
}
