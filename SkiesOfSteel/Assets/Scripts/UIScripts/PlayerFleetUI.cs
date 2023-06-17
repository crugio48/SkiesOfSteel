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
    [SerializeField] private ShipSelectedUI shipSelectedUI;

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
        ShipUnit.ShipIsDestroyed += ShipGotDestroyed;
    }

    private void OnDisable()
    {
        GameManager.Instance.StartGameEvent -= EnableCanvas;
        ShipUnit.ShipIsDestroyed -= ShipGotDestroyed;
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


    private void ShipGotDestroyed(ShipUnit shipUnit)
    {
        if (!_shipsOfLocalPlayer.Contains(shipUnit)) return;

        int index = _shipsOfLocalPlayer.IndexOf(shipUnit);

        shipsButtons[index].interactable = false;

        shipsButtons[index].GetComponent<Image>().sprite = _shipsOfLocalPlayer[index].GetShipGraphics().buttonShipDead;

    }


    public void ClickedButtonOfShipChange(int index)
    {
        Vector3 globalPositionOfShip = tilemap.GetCellCenterWorld(_shipsOfLocalPlayer[index].GetCurrentPosition());
        cameraManagement.MoveToShipPosition(globalPositionOfShip);
        
        shipSelectedUI.ShipClicked(_shipsOfLocalPlayer[index]);
    }

}
