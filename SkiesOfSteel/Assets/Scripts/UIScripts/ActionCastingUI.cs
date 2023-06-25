using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class ActionCastingUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _myActionDescription;

    [SerializeField] private TextMeshProUGUI _selectedShipActionDescription;

    [SerializeField] private TextMeshProUGUI _inputRequiredText;

    [SerializeField] private Button targetsSelectedButton;

    [SerializeField] private GameObject customParameterUI;

    [SerializeField] private TMP_InputField customParameterInput;

    [SerializeField] private Button customParameterSelectedButton;

    [SerializeField] private TextMeshProUGUI _errorText;

    [SerializeField] private Tilemap overlayMap;

    [SerializeField] private Tile overlayTile;

    [SerializeField] private InputManager inputManager;

    private Canvas _canvas;
    private Camera _mainCamera;


    private ShipUnit _selectedShip = null;
    private int _selectedActionIndex;
    private Action _selectedAction = null;

    private bool _isSelectingTargets;
    private List<ShipUnit> _targets;

    private Vector3Int _currentTileUnderMouse;
    private Orientation _curRotation;
    private List<Vector3Int> _positions;
    private List<Orientation> _orientations;

    private List<Vector3Int> _rangeTiles;

    private bool _isCastingAction = false;

    private void Start()
    {
        _canvas = GetComponent<Canvas>();
        _mainCamera = Camera.main;
        _targets = new List<ShipUnit>();
        _positions = new List<Vector3Int>();
        _orientations = new List<Orientation>();
    }


    private void Update()
    {
        if (_selectedShip == null || _selectedAction == null) return;

        if (_isSelectingTargets)
        {
            if (Input.GetMouseButtonDown(0) && !IsUIPresent())
            {
                Debug.Log("Clicked");
                TryAddClick();
            }


            if (_selectedAction.isTargetAnArea)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    _curRotation = ShapeLogic.Instance.GetNextClockwiseOrientation(_curRotation);

                    UpdateAreaOverlayIfNeeded();
                }

                Vector3Int newTileUnderMouse = GetTileUnderMouse();

                if (_currentTileUnderMouse != newTileUnderMouse)
                {
                    _currentTileUnderMouse = newTileUnderMouse;

                    UpdateAreaOverlayIfNeeded();
                }
            }
        }


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DisableCanvas();
        }

    }

    private void EnableCanvas()
    {
        _isCastingAction = true;
        _canvas.enabled = true;
        _targets.Clear();
        _positions.Clear();
        _orientations.Clear();
        _curRotation = Orientation.TOP;
        _currentTileUnderMouse = GetTileUnderMouse();

        _rangeTiles = Pathfinding.Instance.GetRangeTiles(_selectedShip.GetCurrentPosition(), _selectedAction.range);

        ResetOverlayMap();

        inputManager.StopReceivingInput();
    }


    public void DisableCanvas()
    {
        _isCastingAction = false;
        _selectedShipActionDescription.enabled = true;
        _canvas.enabled = false;
        _selectedAction = null;
        _selectedShip = null;
        EmptyOverlayTilemap();
        inputManager.StartReceivingInput();
    }


    public void ActionOfShipSelected(ShipUnit shipUnit, int actionIndex)
    {
        _myActionDescription.text = _selectedShipActionDescription.text;

        _selectedShipActionDescription.enabled = false;

        _errorText.text = string.Empty;

        Action action = shipUnit.GetActions()[actionIndex];

        if (action.needsTarget)
        {
            if (!action.isTargetAnArea)
            {
                _inputRequiredText.text = "Select " + action.amountOfTargets + " target ship" + (action.amountOfTargets > 1 ? "s" : "");
            }
            else
            {
                _inputRequiredText.text = "Select " + action.amountOfTargets + " target area" + (action.amountOfTargets > 1 ? "s" : "") +
                                           ", Press R to rotate area";
            }
            
            _isSelectingTargets = true;
            targetsSelectedButton.gameObject.SetActive(true);
            customParameterUI.SetActive(false);
        }
        else if (action.needsCustomParameter)
        {
            _inputRequiredText.text = action.stringToDisplayWhenAskingForCustomParam;

            _isSelectingTargets = false;
            targetsSelectedButton.gameObject.SetActive(false);
            customParameterInput.text = string.Empty;
            customParameterUI.SetActive(true);
        }
        else
        {
            _inputRequiredText.text = "When ready click confirm";

            _isSelectingTargets = false;
            targetsSelectedButton.gameObject.SetActive(false);
            customParameterUI.SetActive(false);
        }


        _selectedShip = shipUnit;
        _selectedActionIndex = actionIndex;
        _selectedAction = action;
        EnableCanvas();
    }



    public void ClickedTargetsConfirmButton()
    {
        _errorText.text = string.Empty;

        if (_targets.Count == 0 && !_selectedAction.isTargetAnArea)
        {
            _errorText.text = "You have to select at least one target ship";
            return;
        }
        else if (_positions.Count == 0 && _selectedAction.isTargetAnArea)
        {
            _errorText.text = "You have to select at least one target area";
            return;
        }

        _isSelectingTargets = false;

        if (_selectedAction.needsCustomParameter)
        {
            _inputRequiredText.text = _selectedAction.stringToDisplayWhenAskingForCustomParam;
            customParameterInput.text = string.Empty;
            customParameterUI.SetActive(true);
        }
        else
        {
            List<NetworkBehaviourReference> shipReferences = ConvertToReferences(_targets);
            _selectedShip.ActivateActionServerRpc(_selectedActionIndex, shipReferences.ToArray(), _positions.ToArray(), _orientations.ToArray(), 0);
            DisableCanvas();
        }

        targetsSelectedButton.gameObject.SetActive(false);
    }



    public void ClickedSelectedCustomParameterButton()
    {
        _errorText.text = string.Empty;

        if (string.IsNullOrEmpty(customParameterInput.text))
        {
            _errorText.text = "You must insert a value";

            return;
        }

        int value;

        if (!int.TryParse(customParameterInput.text, out value))
        {
            _errorText.text = "The value must be an integer";

            return;
        }

        if (_selectedAction.GetMaxAmountForCustomParam(_selectedShip, _targets, _positions, _orientations) < 
            _selectedAction.GetMinAmountForCustomParam(_selectedShip, _targets, _positions, _orientations))
        {
            _errorText.text = "You cannot use this action on those targets now";

            return;
        }


        if (!_selectedAction.IsCustomParamRangeRespected(_selectedShip, _targets, _positions, _orientations, value))
        {
            _errorText.text = "The value must be greater than " + _selectedAction.GetMinAmountForCustomParam(_selectedShip, _targets, _positions, _orientations) +
                               " and lower than " + _selectedAction.GetMaxAmountForCustomParam(_selectedShip, _targets, _positions, _orientations);
            return;
        }

        List<NetworkBehaviourReference> shipReferences = ConvertToReferences(_targets);

        _selectedShip.ActivateActionServerRpc(_selectedActionIndex, shipReferences.ToArray(), _positions.ToArray(), _orientations.ToArray(), value);
        DisableCanvas();

        customParameterUI.SetActive(false);
    }



    private List<NetworkBehaviourReference> ConvertToReferences(List<ShipUnit> shipUnits)
    {
        List<NetworkBehaviourReference> shipReferences = new List<NetworkBehaviourReference>();

        foreach (ShipUnit shipUnit in shipUnits)
        {
            shipReferences.Add(shipUnit);
        }

        return shipReferences;
    }





    private void TryAddClick()
    {
        _errorText.text = string.Empty;

        Vector3Int selectedTile = GetTileUnderMouse();

        if (!_selectedAction.IsSingleRangeRespected(_selectedShip, selectedTile))
        {
            if (_selectedAction.isTargetAnArea)
            {
                bool hasAtLeastOneAreaBeenRemoved = false;
                for (int i = 0; i < _positions.Count; i++)
                {
                    List<Vector3Int> areaTiles = ShapeLogic.Instance.GetPositionsInThisShape(_selectedAction.shape, _orientations[i], _positions[i]);

                    if (areaTiles.Contains(selectedTile))
                    {
                        _positions.RemoveAt(i);
                        _orientations.RemoveAt(i);
                        hasAtLeastOneAreaBeenRemoved = true;
                    }
                }
                if (hasAtLeastOneAreaBeenRemoved)
                {
                    UpdateAreaOverlayIfNeeded();
                    return;
                }
            }

            _errorText.text = "Selected target is too far for the selected action range";

            Debug.Log("Not enough range");
            return;
        }

        if (_selectedAction.needsLineOfSight && !_selectedAction.HasLineOfSight(_selectedShip, selectedTile))
        {
            _errorText.text = "This action needs line of sight to the target, you don't have it";

            Debug.Log("No line of sight");
            return;
        }

        if (!Pathfinding.Instance.IsTileWalkable(selectedTile))
        {
            _errorText.text = "You cannot directly target a non walkable tile";
            return;
        }


        if (!_selectedAction.isTargetAnArea)
        {
            if (ShipsPositions.Instance.IsThereAShip(selectedTile))
            {
                ShipUnit clickedShip = ShipsPositions.Instance.GetShip(selectedTile);

                Debug.Log("Clicked on ship:" + clickedShip.name);

                if (!_selectedAction.canTargetSelf && clickedShip == _selectedShip)
                {
                    _errorText.text = "This action cannot target the casting ship";

                    Debug.Log("Can't target self");
                    return;
                }
                if (_selectedAction.isSelfOnly && clickedShip != _selectedShip)
                {
                    _errorText.text = "You can only target the casting ship with this action";

                    Debug.Log("Can't target other");
                    return;
                }

                if (_targets.Count < _selectedAction.amountOfTargets && !_targets.Contains(clickedShip))
                {
                    Debug.Log("Selected target: " + clickedShip.name);
                    _targets.Add(clickedShip);
                    ResetOverlayMap();
                }

                else if (_targets.Contains(clickedShip))
                {
                    Debug.Log("Removed target: " + clickedShip.name);
                    _targets.Remove(clickedShip);
                    ResetOverlayMap();
                }

                else if (_targets.Count >= _selectedAction.amountOfTargets && !_targets.Contains(clickedShip))
                {
                    _errorText.text = "You already selected the maximum amount of targets";

                    return;
                }
            }
        }
        else
        {
            if (!_selectedAction.canTargetSelf && selectedTile == _selectedShip.GetCurrentPosition())
            {
                _errorText.text = "This action cannot target the casting ship";

                return;
            }
            if (_selectedAction.isSelfOnly && selectedTile != _selectedShip.GetCurrentPosition())
            {
                _errorText.text = "You can only target the casting ship with this action";

                return;
            }
            if (_selectedAction.isAffectingOnlyEmptyPositions && ShipsPositions.Instance.IsThereAShip(selectedTile))
            {
                _errorText.text = "You can only target empty positions with this action";

                return;
            }

            bool hasAtLeastOneAreaBeenRemoved = false;

            for (int i = 0; i < _positions.Count; i++)
            {
                List<Vector3Int> areaTiles = ShapeLogic.Instance.GetPositionsInThisShape(_selectedAction.shape, _orientations[i], _positions[i]);

                if (areaTiles.Contains(selectedTile))
                {
                    _positions.RemoveAt(i);
                    _orientations.RemoveAt(i);
                    hasAtLeastOneAreaBeenRemoved = true;
                }
            }

            if (hasAtLeastOneAreaBeenRemoved)
            {
                UpdateAreaOverlayIfNeeded();
            }

            else if (!hasAtLeastOneAreaBeenRemoved && _positions.Count < _selectedAction.amountOfTargets)
            {
                _positions.Add(selectedTile);
                _orientations.Add(_curRotation);
                ResetOverlayMap();
            }

            else if (!hasAtLeastOneAreaBeenRemoved && _positions.Count >= _selectedAction.amountOfTargets)
            {
                _errorText.text = "You already selected the maximum amount of target areas";

                return;
            }

        }
    }

    private void EmptyOverlayTilemap()
    {
        overlayMap.ClearAllTiles();
    }



    private void ResetOverlayMap()
    {
        overlayMap.ClearAllTiles();

        ShowRangeTiles();

        if (_selectedAction != null && !_selectedAction.isTargetAnArea)
        {
            for (int i = 0; i < _targets.Count; i++)
            {
                ShowTargetAreaTiles(_targets[i].GetCurrentPosition());
            }
        }

        else if (_selectedAction != null && _selectedAction.isTargetAnArea)
        {
            for (int i = 0; i < _positions.Count; i++)
            {
                List<Vector3Int> confirmedPositions = ShapeLogic.Instance.GetPositionsInThisShape(_selectedAction.shape, _orientations[i], _positions[i]);
                ShowTargetAreaTiles(confirmedPositions);
            }
        }
    }

    private void UpdateAreaOverlayIfNeeded()
    {
        if (_positions.Count < _selectedAction.amountOfTargets)
        {
            ResetOverlayMap();

            List<Vector3Int> tilesInCurrentArea = ShapeLogic.Instance.GetPositionsInThisShape(_selectedAction.shape, _curRotation, _currentTileUnderMouse);
            ShowTargetAreaTiles(tilesInCurrentArea);
        }
    }


    private void ShowRangeTiles()
    {
        Color color = new Color(1f, 0.5f, 0f, 1f);

        foreach (Vector3Int pos in _rangeTiles)
        {
            overlayMap.SetTile(pos, overlayTile);
            overlayMap.SetTileFlags(pos, TileFlags.None);
            overlayMap.SetColor(pos, color);
        }
    }


    private void ShowTargetAreaTiles(List<Vector3Int> targetAreaTiles)
    {
        Color color = Color.red;

        foreach (Vector3Int pos in targetAreaTiles)
        {
            overlayMap.SetTile(pos, overlayTile);
            overlayMap.SetTileFlags(pos, TileFlags.None);
            overlayMap.SetColor(pos, color);
        }
    }

    private void ShowTargetAreaTiles(Vector3Int targetAreaTile)
    {
        Color color = Color.red;

        overlayMap.SetTile(targetAreaTile, overlayTile);
        overlayMap.SetTileFlags(targetAreaTile, TileFlags.None);
        overlayMap.SetColor(targetAreaTile, color);
    }


    private Vector3Int GetTileUnderMouse()
    {
        Vector2 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        return overlayMap.WorldToCell(mousePosition);
    }


    private bool IsUIPresent()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }


    public bool IsCastingAction()
    {
        return _isCastingAction;
    }
}
