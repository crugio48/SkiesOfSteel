using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : MonoBehaviour
{
    public static TileManager Instance { get; private set; }

    [SerializeField]
    private List<TileInfo> _tileInfos;


    private Dictionary<TileBase, TileInfo> _dataFromTiles;



    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }


        _dataFromTiles = new Dictionary<TileBase, TileInfo>();

        foreach(var tileInfo in _tileInfos)
        {
            foreach(var tile in tileInfo.tiles)
            {
                _dataFromTiles.Add(tile, tileInfo);
            }
        }
    }


    public TileInfo GetTileInfo(TileBase tile)
    {
        return _dataFromTiles[tile];
    }

}
