using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private Tilemap overlayMap;

    [SerializeField]
    private LayerMask layerMask;

    [SerializeField]
    private Tile overlayTile;

    [SerializeField]
    private PlayerShipUI playerShipUI;

    [SerializeField]
    private ActionInstructionCanvas actionInstructionCanvas;

    private Camera _mainCamera;

    private ShipUnit _selectedShip;

    private bool _receiveInput = false;
    
    private bool targetingShips = false;
    


    void Start()
    {
        _mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        GameManager.Instance.StartGameEvent += StartReceivingInput;
        ShipUnit.MovementCompleted += RefreshMovementTiles;

    }

    private void OnDisable()
    {
        GameManager.Instance.StartGameEvent -= StartReceivingInput;
        ShipUnit.MovementCompleted -= RefreshMovementTiles;

    }

    public void StartReceivingInput()
    {
        _receiveInput = true;
    }

    public void StopReceivingInput()
    {
        _receiveInput = false;
    }

    private void RefreshMovementTiles(ShipUnit shipModifiedMovementLeft)
    {
        if (shipModifiedMovementLeft == _selectedShip && shipModifiedMovementLeft.IsMyShip())
        {
            ResetOverlayMap();

            if (_selectedShip.GetMovementLeft() > 0)
            {
                List<Vector3Int> possibleDestinationTiles = Pathfinding.Instance.GetPossibleDestinations(_selectedShip.GetCurrentPosition(), _selectedShip.GetMovementLeft(), _selectedShip);
                DisplayMovementOverlayTiles(possibleDestinationTiles);
            }
        }
    }


    private void Update()
    {
        if (!_receiveInput) return;

        if (NetworkManager.Singleton.IsServer) return;

        if (!targetingShips)
        {
            if (Input.GetMouseButtonDown(0) && !IsUIPresent())
            {

                ResetOverlayMap();

                SelectShip();
            }

            if (Input.GetMouseButtonDown(1) && !IsUIPresent())
            {
                if (_selectedShip != null && _selectedShip.IsMyShip() && _selectedShip.GetMovementLeft() > 0) TryToMove();
            }
        }

    }

    private void SelectShip()
    {
        Vector2 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int selectedTile = overlayMap.WorldToCell(mousePosition);

        if (ShipsPositions.Instance.IsThereAShip(selectedTile))
        {
            _selectedShip = ShipsPositions.Instance.GetShip(selectedTile);

            playerShipUI.ShipClicked(_selectedShip);

            Debug.Log("Selected ship: " + _selectedShip.name);

            if (_selectedShip.IsMyShip() && _selectedShip.GetMovementLeft() > 0)
            {
                List<Vector3Int> possibleDestinationTiles = Pathfinding.Instance.GetPossibleDestinations(_selectedShip.GetCurrentPosition(), _selectedShip.GetMovementLeft(), _selectedShip);
                DisplayMovementOverlayTiles(possibleDestinationTiles);
            }
        }
        else
        {
            playerShipUI.NoShipClicked();
            Debug.Log("No Ship Found");
        }
    }
    private bool IsUIPresent()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    private void TryToMove()
    {
        Vector2 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int selectedTile = overlayMap.WorldToCell(mousePosition);

        List<Vector3Int> possibleDestinationTiles = Pathfinding.Instance.GetPossibleDestinations(_selectedShip.GetCurrentPosition(), _selectedShip.GetMovementLeft(), _selectedShip);

        if (possibleDestinationTiles.Contains(selectedTile))
        {
            Debug.Log("Calling MoveServerRpc");
            _selectedShip.MoveServerRpc(selectedTile);
            ResetOverlayMap();
        }
        else
        {
            Debug.Log("Can't reach that tile with the current movement left");
        }
    }


    private void DisplayMovementOverlayTiles(List<Vector3Int> possibleDestinationTiles)
    {
        Color color = Color.yellow;
        foreach (Vector3Int pos in possibleDestinationTiles)
        {
            overlayMap.SetTile(pos, overlayTile);
            overlayMap.SetTileFlags(pos, TileFlags.None);
            overlayMap.SetColor(pos, color);
        }
    }


    public void ResetOverlayMap()
    {
        overlayMap.ClearAllTiles();
    }
    
}

