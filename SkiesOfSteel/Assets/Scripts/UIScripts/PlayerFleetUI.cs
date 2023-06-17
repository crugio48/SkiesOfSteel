using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class PlayerFleetUI : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;

    [SerializeField] private CameraManagement cameraManagement;

    [SerializeField] private Tilemap tilemap;

    [SerializeField] private List<Button> shipsButtons;

    List<ShipUnit> _shipsOfLocalPlayer;

    private Canvas _canvas;


    private void Start()
    {
        _canvas = GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        GameManager.Instance.StartGameEvent += EnableCanvas;
        ShipUnit.StatsGotModified += CheckIfShipGotDestroyed;
    }

    private void OnDisable()
    {
        GameManager.Instance.StartGameEvent -= EnableCanvas;
        ShipUnit.StatsGotModified -= CheckIfShipGotDestroyed;
    }

    private void EnableCanvas()
    {
        _shipsOfLocalPlayer = PlayersShips.Instance.GetShips(NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>().GetUsername());

        // MAYBE TODO spawn a custom amount of buttons based on _shipsOfLocalPlayer.Count

        for (int i = 0; i < _shipsOfLocalPlayer.Count; i++)
        {
            shipsButtons[i].GetComponent<Image>().sprite = _shipsOfLocalPlayer[i].GetShipGraphics().buttonShipAlive;
            shipsButtons[i].GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;
        }
        _canvas.enabled = true;
    }


    private void CheckIfShipGotDestroyed(ShipUnit shipUnit)
    {
        if (_shipsOfLocalPlayer == null) return;

        if (!_shipsOfLocalPlayer.Contains(shipUnit)) return;

        if (!shipUnit.IsDestroyed()) return;

        int index = _shipsOfLocalPlayer.IndexOf(shipUnit);

        shipsButtons[index].interactable = false;

        shipsButtons[index].GetComponent<Image>().sprite = _shipsOfLocalPlayer[index].GetShipGraphics().buttonShipDead;

    }


    public void ClickedButtonOfShipChange(int index)
    {
        if (_shipsOfLocalPlayer[index].IsDestroyed()) return;

        Vector3 globalPositionOfShip = tilemap.GetCellCenterWorld(_shipsOfLocalPlayer[index].GetCurrentPosition());
        cameraManagement.MoveToShipPosition(globalPositionOfShip);
        
        inputManager.Click(_shipsOfLocalPlayer[index]);
    }

}
