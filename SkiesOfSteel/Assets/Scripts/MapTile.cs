using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu]
public class MapTile : Tile
{
    [Space]
    [Header("Map Tile settings:")]

    [SerializeField]
    private bool _isWalkable;


    public bool IsWalkable { get { return _isWalkable; } }
}
