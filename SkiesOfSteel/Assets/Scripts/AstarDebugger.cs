using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AstarDebugger : MonoBehaviour
{
    public static AstarDebugger Instance { get; private set; }

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
    }

    [SerializeField]
    private Astar _astar;

    [SerializeField]
    private Tilemap _debugTilemap;

    [SerializeField]
    private Tile _debugTile;

    [SerializeField]
    private Color _visitedColor, _frontierColor, _pathColor, _startColor, _goalColor;


    private Camera _mainCamera;


    private Vector3Int _start, _goal;


    private void Start()
    {
        _mainCamera = Camera.main;
        ColorTile(_start, _startColor);
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            _goal = _debugTilemap.WorldToCell(mousePosition);


            ColorTile(_goal, _goalColor);
                
            Node path = _astar.Search(_start, _goal);
            
            CreateTiles(path);
            
        }


        if (Input.GetMouseButtonDown(1))
        {

            Vector2 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            _start = _debugTilemap.WorldToCell(mousePosition);
            ColorTile(_start, _startColor);

        }


        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetTilemap();
        }
    }


    public void CreateTiles(Node path)
    {
        for (Node step = path; step != null; step = step.Parent)
        {
            ColorTile(step.Position, _pathColor);
        }
        ColorTile(_start, _startColor);
        ColorTile(_goal, _goalColor);
    }


    public void ColorTile(Vector3Int position, Color color)
    {
        _debugTilemap.SetTile(position, _debugTile);
        _debugTilemap.SetTileFlags(position, TileFlags.None);
        _debugTilemap.SetColor(position, color);
    }


    private void ResetTilemap()
    {
        _debugTilemap.ClearAllTiles();
    }
}
