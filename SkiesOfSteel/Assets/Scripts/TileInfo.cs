using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class TileInfo : ScriptableObject
{
    public TileBase[] tiles;


    public bool IsWalkable;

}
