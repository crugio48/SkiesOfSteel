using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovementDebugger : MonoBehaviour
{
    public Tilemap tilemap;
    public ShipUnit shipToMove;

    private bool _debuggerStarted = false;

    private void Start()
    {
        StartCoroutine(MovementDebuggerStartCoroutine());
    }

    private IEnumerator MovementDebuggerStartCoroutine()
    {
        yield return new WaitForSeconds(1);

        Vector3Int initialPosition = new Vector3Int(0, -2, 0);

        shipToMove.SetInitialPosition(initialPosition);
        ShipsPositions.Instance.Place(shipToMove, initialPosition);

        _debuggerStarted = true;
    }


    private void Update()
    {
        if (!_debuggerStarted) return;

        if (Input.GetMouseButtonDown(1))
        {
            shipToMove.EnableShip();


            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int destinationTile = tilemap.WorldToCell(mousePosition);

            shipToMove.Move(destinationTile);
        }
    }
}
