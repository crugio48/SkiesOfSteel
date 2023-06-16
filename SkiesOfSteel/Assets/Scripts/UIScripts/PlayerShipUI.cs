using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShipUI : MonoBehaviour
{
    [SerializeField]
    private InputManager inputManager;

    [SerializeField]
    private ActionInstructionCanvas actionInstructionCanvas;

    [SerializeField] private TextMeshProUGUI _selectedShipStatsText;

    [SerializeField] private Button healButton;
    [SerializeField] private Button refuelButton;
    [SerializeField] private Button action0Button;
    [SerializeField] private Button action1Button;
    [SerializeField] private Button action2Button;

    private Canvas canvas;

    ShipUnit _shipSelected;

    List<ShipUnit> _shipList;

    private string _playerName;


    private void Start()
    {
        canvas = GetComponent<Canvas>();
    }
    public void ShipClicked(ShipUnit selectedShip)
    {
        //TODO Add to ShipUnit the splashart for the ship and the captain
        _shipSelected = selectedShip;


        _playerName = _shipSelected.GetOwnerUsername();
        _shipList = PlayersShips.Instance.GetShips(_playerName);

        _shipList.Remove(_shipSelected);

        EnableCanvas();
    }

    public void NoShipClicked()
    {
        DisableCanvas();
    }

    private void EnableCanvas()
    {
        _selectedShipStatsText.text = "Health = " + _shipSelected.GetCurrentHealth() + " / " + _shipSelected.GetMaxHealth() +
                                                                                            "\nFuel = " + _shipSelected.GetCurrentFuel() + " / " + _shipSelected.GetMaxFuel() +
                                                                                            "\nCurrent Bonus Attack Stage = " + _shipSelected.GetAttackStage() +
                                                                                            "\nCurrent Bonus Defence Stage = " + _shipSelected.GetDefenseStage() +
                                                                                            "\nMovements Left = " + _shipSelected.GetMovementLeft();

        action0Button.GetComponentInChildren<TextMeshProUGUI>().text = _shipSelected.GetActions()[0].name;
        action1Button.GetComponentInChildren<TextMeshProUGUI>().text = _shipSelected.GetActions()[1].name;
        action2Button.GetComponentInChildren<TextMeshProUGUI>().text = _shipSelected.GetActions()[2].name;

        canvas.enabled = true;
    }

    private void DisableCanvas()
    {
        canvas.enabled = false;
    }


    //FLASHIP METHODS  (Change Selected Ship)
    public void ToggleImage()
    {
        //TODO change sprite to Captain
    }

    public void HealShip()
    {
        _shipSelected.HealActionServerRpc();
    }

    public void RefuelShip()
    {
        _shipSelected.RefuelToMaxAtPortActionServerRpc();
    }

    public void StartActionOfShipAtIndex(int index)
    {
        actionInstructionCanvas.ActionOfShipSelected(_shipSelected, index);
    }
    

    public void ClickedButtonOfShipChange(int index)
    {
        ShipClicked(_shipList[index]);
    }
}
