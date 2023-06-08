using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovementDebugger : MonoBehaviour
{
    public Tilemap tilemap;
    public ShipUnit shipToMove;

    private void Start()
    {
        Vector3Int initialPosition = new Vector3Int(0,-2,0);

        shipToMove.SetInitialPosition(initialPosition);
        ShipsPositions.instance.Place(shipToMove, initialPosition);

    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            shipToMove.EnableShip();


            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int destinationTile = tilemap.WorldToCell(mousePosition);

            shipToMove.Move(destinationTile);
        }
    }
}
