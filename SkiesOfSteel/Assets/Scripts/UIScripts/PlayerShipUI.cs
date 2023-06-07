using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShipUI : MonoBehaviour
{
    ShipUnit _shipSelected, shipFlagship, shipAttack, shipRefuel, shipFast;
    List<Action> shipActions;

    public void ShipClicked(ShipUnit selectedShip)
    {
        //TODO Add to ShipUnit the splashart for the ship and the captain
        //TODO Understand type of ship and get all ships from the same fleet
        _shipSelected = selectedShip;
        shipActions = _shipSelected.GetActions();
        ChildEnable();


    }
    public void NoShipClicked()
    {
        ChildDisable();
    }

    private void ChildEnable()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            //TODO do this for each ship type
            transform.GetChild(i).gameObject.SetActive(true);
            //transform.GetChild(i).GetChild(0). change sprites
            transform.GetChild(i).GetChild(0).GetComponentInChildren<Text>().text = "Attack Damage = " + _shipSelected.GetAttack;
            //TODO Add all other attributes 
            transform.GetChild(i).GetChild(4).GetComponentInChildren<Text>().text = shipActions[1].name;
            transform.GetChild(i).GetChild(5).GetComponentInChildren<Text>().text = shipActions[2].name;
        }
    }
    private void ChildDisable()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }
    //TODO do each function for each ship's type
    public void ToggleCaptain() {//change sprite to Captain
    }
    public void ToggleShip() {//change sprite to Ship + Attributes 
    }
    public void HealShip(){_shipSelected.HealAtPortAction();}
    public void BasicAttack() { shipActions[0].Activate(_shipSelected);}
    public void Refuel(){_shipSelected.RefuelToMaxAtPortAction();}
    public void Action1(){shipActions[1].Activate(_shipSelected);}
    public void Action2(){shipActions[2].Activate(_shipSelected);}
}
