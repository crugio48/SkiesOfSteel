using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class ActionInstructionCanvas : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _actionDescriptionText;

    [SerializeField] private TextMeshProUGUI _inputRequiredText;

    [SerializeField] private Button targetsSelectedButton;

    [SerializeField] private GameObject customParameterUI;

    [SerializeField] private TMP_InputField customParameterInput;

    [SerializeField] private Button customParameterSelectedButton;

    [SerializeField] private TextMeshProUGUI _errorText;

    [SerializeField] private Tilemap overlayMap;

    [SerializeField] private Tile overlayTile;

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
            if (Input.GetMouseButtonDown(0))
            {
                TryAddClick();
            }


            if (_selectedAction.isTargetAnArea)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    _curRotation = ShapeLogic.Instance.GetNextClockwiseOrientation(_curRotation);
                }

                Vector3Int newTileUnderMouse = GetTileUnderMouse();

                if (_currentTileUnderMouse != newTileUnderMouse)
                {
                    List<Vector3Int> tilesInCurrentArea = ShapeLogic.Instance.GetPositionsInThisShape(_selectedAction.shape, _curRotation, _currentTileUnderMouse);
                    ResetOverlayMap();
                    DisplayTargetAreaTiles(tilesInCurrentArea);
                    _currentTileUnderMouse = newTileUnderMouse;
                }
            }
        }


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DisableCanvas();
        }

    }

    //TODO make private
    public void EnableCanvas()
    {
        _canvas.enabled = true;
        _targets.Clear();
        _positions.Clear();
        _orientations.Clear();
        _curRotation = Orientation.TOP;
        _currentTileUnderMouse = GetTileUnderMouse();
        ResetOverlayMap();
        if (_selectedAction.isTargetAnArea)
        {
            List<Vector3Int> tilesInCurrentArea = ShapeLogic.Instance.GetPositionsInThisShape(_selectedAction.shape, _curRotation, _currentTileUnderMouse);
            DisplayTargetAreaTiles(tilesInCurrentArea);
        }
        
    }

    //TODO make private
    public void DisableCanvas()
    {
        _canvas.enabled = false;
        _selectedAction = null;
        _selectedShip = null;
        ResetOverlayMap();
    }

    public void ChangeActionDescription(string text)
    {
        // DEPRECATED
    }

    public void ChangeTextDescription(string text)
    {
        // DEPRECATED
    }


    public void ActionOfShipSelected(ShipUnit shipUnit, int actionIndex)
    {
        Action action = shipUnit.GetActions()[actionIndex];

        _actionDescriptionText.text = action.description;

        if (action.needsTarget)
        {
            _inputRequiredText.text = "Select " + action.amountOfTargets + " targets";

            _isSelectingTargets = true;
            targetsSelectedButton.gameObject.SetActive(true);
            customParameterUI.SetActive(false);
        }
        else if (action.needsCustomParameter)
        {
            _isSelectingTargets = false;
            targetsSelectedButton.gameObject.SetActive(false);
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
        if (_targets.Count == 0 && _selectedAction.needsTarget)
        {
            _errorText.text = "You have to select at least one target";
        }

        _isSelectingTargets = false;

        if (_selectedAction.needsCustomParameter)
        {
            _inputRequiredText.text = _selectedAction.stringToDisplayWhenAskingForCustomParam;
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
        if (string.IsNullOrEmpty(customParameterInput.text)) return;

        int value;
        if (!int.TryParse(customParameterInput.text, out value)) return;

        if (!_selectedAction.IsCustomParamRangeRespected(_selectedShip, _targets, _positions, _orientations, value))
        {
            _errorText.text = "the value must be greater than " + _selectedAction.GetMinAmountForCustomParam(_selectedShip, _targets, _positions, _orientations) +
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
        Vector3Int selectedTile = GetTileUnderMouse();

        if (!_selectedAction.IsSingleRangeRespected(_selectedShip, selectedTile)) return;

        if (_selectedAction.needsLineOfSight && !_selectedAction.HasLineOfSight(_selectedShip, selectedTile)) return;

        if (!_selectedAction.isTargetAnArea)
        {
            if (ShipsPositions.Instance.IsThereAShip(selectedTile))
            {
                ShipUnit clickedShip = ShipsPositions.Instance.GetShip(selectedTile);

                if (!_selectedAction.canTargetSelf && clickedShip == _selectedShip) return;

                if (_selectedAction.isSelfOnly && clickedShip != _selectedShip) return;


                if (_targets.Count < _selectedAction.amountOfTargets && !_targets.Contains(clickedShip))
                {
                    _targets.Add(clickedShip);
                    AddOverlayTileAtPos(selectedTile);
                }

                else if (_targets.Contains(clickedShip))
                {
                    _targets.Remove(clickedShip);
                    RemoveOverlayTileAtPos(selectedTile);
                }
            }
        }
        else
        {
            if (!_selectedAction.canTargetSelf && selectedTile == _selectedShip.GetCurrentPosition()) return;

            if (_selectedAction.isSelfOnly && selectedTile != _selectedShip.GetCurrentPosition()) return;

            if (_selectedAction.isAffectingOnlyEmptyPositions && ShipsPositions.Instance.IsThereAShip(selectedTile)) return;


            if (_positions.Count < _selectedAction.amountOfTargets && !_positions.Contains(selectedTile))
            {
                _positions.Add(selectedTile);
                _orientations.Add(_curRotation);
            }
            else if(_positions.Contains(selectedTile))
            {
                int index = _positions.IndexOf(selectedTile);

                _positions.RemoveAt(index);
                _orientations.RemoveAt(index);
            }
        }
    }


    private void AddOverlayTileAtPos(Vector3Int pos)
    {
        Color color = Color.red;
        overlayMap.SetTile(pos, overlayTile);
        overlayMap.SetTileFlags(pos, TileFlags.None);
        overlayMap.SetColor(pos, color);
    }

    private void RemoveOverlayTileAtPos(Vector3Int pos)
    {
        overlayMap.SetTile(pos, null);
    }


    private void DisplayTargetAreaTiles(List<Vector3Int> targetAreaTiles)
    {
        Color color = Color.red;

        foreach (Vector3Int pos in targetAreaTiles)
        {
            overlayMap.SetTile(pos, overlayTile);
            overlayMap.SetTileFlags(pos, TileFlags.None);
            overlayMap.SetColor(pos, color);
        }
    }


    private void ResetOverlayMap()
    {
        overlayMap.ClearAllTiles();

        if (_selectedAction != null && _selectedAction.isTargetAnArea)
        {
            for (int i = 0; i < _positions.Count; i++)
            {
                List<Vector3Int> confirmedPositions = ShapeLogic.Instance.GetPositionsInThisShape(_selectedAction.shape, _orientations[i], _positions[i]);
                DisplayTargetAreaTiles(confirmedPositions);
            }
        }
    }


    private Vector3Int GetTileUnderMouse()
    {
        return overlayMap.WorldToCell(_mainCamera.ScreenToWorldPoint(Input.mousePosition));
    }
}
