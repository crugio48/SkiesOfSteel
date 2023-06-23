
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;


public class Treasure : SingletonNetwork<Treasure>
{
    [SerializeField] private Tilemap tilemap;

    private ShipUnit _carryingShip = null;

    private Vector3Int _curGridPosition;

    public void SetInitialPosition(Vector3Int startGridPosition)
    {
        _curGridPosition = startGridPosition;
        transform.position = tilemap.GetCellCenterWorld(startGridPosition);
    }

    public void SetCurGridPosition(Vector3Int curGridPosition)
    {
        _curGridPosition = curGridPosition;
    }

    public Vector3Int GetCurGridPosition()
    {
        return _curGridPosition;
    }

    public bool IsBeingCarried()
    {
        if (_carryingShip == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }


    // This will only be called by the server script of shipunit
    public void SetCarryingShip(ShipUnit ship)
    {
        _carryingShip = ship;
    }

    public void RemoveCarryingShip()
    {
        _carryingShip = null;
    }

    public ShipUnit GetCarryingShip()
    {
        return _carryingShip;
    }


    public void LateUpdate()
    {
        if (!IsServer) return;


        if (_carryingShip != null && _carryingShip.transform.position != transform.position)
        {
            transform.position = _carryingShip.transform.position;
        }
    }
}
