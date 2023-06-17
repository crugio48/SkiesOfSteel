using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ActionButtonDescription : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI _actionDescriptionText;

    [SerializeField] private bool isHealButton;

    [SerializeField] private bool isRefuelButton;

    [SerializeField] private int actionIndex;

    private string _textToDisplay;

    private ShipUnit _selectedShip;

    void Start()
    {
        if (isHealButton)
        {
            _textToDisplay = "Heal action that can be used everywhere.\n" +
                "If in a port heals by 20% at no fuel cost.\n" +
                "If outside a port heals by 10% at 2 fuel cost";
        }
        else if (isRefuelButton)
        {
            _textToDisplay = "Refuel action that can be used inside of a port or adjacent to it.\n" +
                "It refuels the entire ship fuel tank.";
        }
        else _textToDisplay = "";
    }

    public void SetSelectedShip(ShipUnit selectedShip)
    {
        _selectedShip = selectedShip;
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isHealButton && !isRefuelButton && _selectedShip != null)
        {
            _textToDisplay = _selectedShip.GetActions()[actionIndex].description;
        }

        _actionDescriptionText.text = _textToDisplay;
        _actionDescriptionText.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _actionDescriptionText.gameObject.SetActive(false);

    }
}
