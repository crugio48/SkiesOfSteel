using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private Tilemap _debugTilemap;

    [SerializeField]
    private Tile _debugTile;

    private Camera _mainCamera;

    private Vector3Int selectedtile;

    ShipUnit ship, casterShip;

    int indexAction;

    List<Vector3Int> possibleTiles;

    //TODO FIND playerShipUI & ActionInstructionCanvas
    PlayerShipUI playerShipUI;
    ActionInstructionCanvas actionInstructionCanvas;
    int cyclingTargeting;
    int targetAmount;
    private bool targetingShips;
    List<ShipUnit> targetList;

    void Start()
    {
        _mainCamera = Camera.main;
        //playerShipUI = GameObject.FindGameObjectWithTag("UI Fleet");
        targetingShips = false;
        cyclingTargeting = 0;
    }

    private void Update()
    {
        if (targetingShips == true)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                targetingShips = false;
                actionInstructionCanvas.DisableCanvas();
            }
            if (Input.GetMouseButtonDown(0))
            {
                
                Vector2 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                selectedtile = _debugTilemap.WorldToCell(mousePosition);
                if (ShipsPositions.Instance.GetShip(selectedtile) != null)
                {
                    targetList.Add(ShipsPositions.Instance.GetShip(selectedtile));
                    cyclingTargeting = +1;
                    if (cyclingTargeting == targetAmount)
                    {
                        actionInstructionCanvas.ChangeTextDescription("Select " + (targetAmount-cyclingTargeting) + " More Target");
                        playerShipUI.ReceiveTargets(indexAction, casterShip,targetList);
                        targetingShips = false;
                    }

                }
                else
                {
                    playerShipUI.NoShipClicked();
                    Debug.Log("No Ship Found to Target");
                }


            }
        }
        else {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            selectedtile = _debugTilemap.WorldToCell(mousePosition);
            if(ShipsPositions.Instance.GetShip(selectedtile) != null)
            {
                ship = ShipsPositions.Instance.GetShip(selectedtile);
                playerShipUI.ShipClicked(ship);
            }
            else
            {
                playerShipUI.NoShipClicked();
                Debug.Log("No Ship Found");
            }
            
            
        }
            if (Input.GetMouseButtonDown(1))
            {
                Vector2 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                selectedtile = _debugTilemap.WorldToCell(mousePosition);
                //possibleTiles = _pathfinding.GetPossibleDestinations(ship.GetCurrentPosition(), ship.GetMovementLeft());
                if (ship != null & possibleTiles.Contains(selectedtile))
                {
                    ship.MoveServerRpc(selectedtile);
                }
                else
                {
                    Debug.Log("No Ship Selected or Not Enough Movements Left");
                }


            }
        }

    }
    public void startLookingForTarget(ShipUnit _casterShip, int _indexAction, int _targetAmount)
    {
        targetingShips = true;
        targetAmount = _targetAmount;
        cyclingTargeting = 0;
        indexAction = _indexAction;
        casterShip = _casterShip;
    }
}
