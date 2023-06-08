using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private Pathfinding _pathfinding;

    [SerializeField]
    private Tilemap _debugTilemap;

    [SerializeField]
    private Tile _debugTile;

    private Camera _mainCamera;

    private Vector3Int selectedtile;

    ShipUnit ship;

    PlayerShipUI playerShipUI;
    
    void Start()
    {
        _mainCamera = Camera.main;
        //playerShipUI = GameObject.FindGameObjectWithTag("UI Fleet");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            selectedtile = _debugTilemap.WorldToCell(mousePosition);
            if(ShipsPositions.instance.GetShip(selectedtile) != null)
            {
                ship = ShipsPositions.instance.GetShip(selectedtile);
                playerShipUI.ShipClicked(ship);
            }
            else
            {
                playerShipUI.NoShipClicked();
                Debug.Log("No Ship Found");
            }
            
            
        }

    }
}
