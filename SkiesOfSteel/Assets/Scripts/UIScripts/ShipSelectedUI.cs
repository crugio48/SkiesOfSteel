using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipSelectedUI : MonoBehaviour
{
    [SerializeField]
    private InputManager inputManager;

    [SerializeField]
    private ActionCastingUI actionCastingUI;

    [SerializeField] private Toggle toggle;

    [SerializeField] private Image captainCardFace;
    [SerializeField] private Image shipCardFace;

    [SerializeField] private TextMeshProUGUI _selectedShipStatsText;

    [SerializeField] private Slider healthBar;

    [SerializeField] private Slider fuelBar;


    [SerializeField] private Button healButton;
    [SerializeField] private Button refuelButton;

    [SerializeField] private List<Button> actionsButtons;

    private Canvas canvas;

    ShipUnit _shipSelected;


    private void Start()
    {
        canvas = GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        ShipUnit.StatsGotModified += CheckRefreshUI;
    }

    private void OnDisable()
    {
        ShipUnit.StatsGotModified -= CheckRefreshUI;
    }

    public void CheckRefreshUI(ShipUnit shipModified)
    {
        if (shipModified == _shipSelected)
        {
            ShipClicked(_shipSelected);
        }
    }


    public void ShipClicked(ShipUnit selectedShip)
    {
        if (selectedShip.IsDestroyed()) NoShipClicked();

        if (_shipSelected != null) _shipSelected.RemoveHighlight();

        _shipSelected = selectedShip;

        _shipSelected.SetHighlight();

        EnableCanvas();
    }

    public void NoShipClicked()
    {
        if (_shipSelected != null && !_shipSelected.IsDestroyed()) _shipSelected.RemoveHighlight();
        _shipSelected = null;
        DisableCanvas();
    }

    private void EnableCanvas()
    {
        captainCardFace.sprite = _shipSelected.GetShipGraphics().captainCardFace;
        shipCardFace.sprite = _shipSelected.GetShipGraphics().shipCardFace;

        _selectedShipStatsText.text = "Base Attack = " + _shipSelected.GetBaseAttack() +
                                    "\nAttack stage: " + (_shipSelected.GetAttackStage() + _shipSelected.GetOneTurnTemporaryAttackStage()) +
                                    "\nBase Defense = " + _shipSelected.GetBaseDefense() +
                                    "\nDefense stage: " + (_shipSelected.GetDefenseStage() + _shipSelected.GetOneTurnTemporaryDefenseStage()) +
                                    "\nMovements Left = " + _shipSelected.GetMovementLeft() +
                                    "\nHas Action Left: " + (_shipSelected.CanDoAction() ? "yes" : "no");

        healthBar.maxValue = _shipSelected.GetMaxHealth();
        healthBar.value = _shipSelected.GetCurrentHealth();

        healthBar.GetComponentInChildren<TextMeshProUGUI>().text = "Health  " + healthBar.value + " / " + healthBar.maxValue;

        fuelBar.maxValue = _shipSelected.GetMaxFuel();
        fuelBar.value = _shipSelected.GetCurrentFuel();

        fuelBar.GetComponentInChildren<TextMeshProUGUI>().text = "Fuel  " + fuelBar.value + " / " + fuelBar.maxValue;


        // MAYBE TODO spawn actions buttons dinamically based on ships actions list lenght

        for (int i = 0; i < actionsButtons.Count; i++)
        {
            actionsButtons[i].GetComponent<ActionButtonDescription>().SetSelectedShip(_shipSelected);

            actionsButtons[i].GetComponent<Image>().sprite = _shipSelected.GetActions()[i].sprite;

            if (_shipSelected.IsMyShip() && _shipSelected.CanDoAction() && _shipSelected.GetCurrentFuel() >= _shipSelected.GetActions()[i].fuelCost)
            {
                actionsButtons[i].interactable = true;
            }
            else
            {
                actionsButtons[i].interactable = false;
            }
        }


        if (_shipSelected.IsMyShip() && _shipSelected.CanDoAction())
        {
            if (Pathfinding.Instance.IsPosOnTopOfAPortOrAdjacent(_shipSelected.GetCurrentPosition()))
            {
                refuelButton.interactable = true;
            }
            else
            {
                refuelButton.interactable = false;
            }



            if (Pathfinding.Instance.IsOnTopOfAPort(_shipSelected.GetCurrentPosition()))
            {
                healButton.interactable = true;
            }
            else
            {
                healButton.interactable = false;
            }
        }
        else
        {
            healButton.interactable = false;
            refuelButton.interactable = false;
        }

        canvas.enabled = true;
    }

    private void DisableCanvas()
    {
        canvas.enabled = false;
    }


    public void ToggleImage()
    {
        _selectedShipStatsText.enabled = toggle.isOn;
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
        actionCastingUI.ActionOfShipSelected(_shipSelected, index);
    }
   
}
