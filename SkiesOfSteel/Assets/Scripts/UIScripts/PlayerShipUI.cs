using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShipUI : MonoBehaviour
{
    ShipUnit _shipSelected, shipFlagship, shipAttack, shipCargo, shipFast;
    List<Action> shipActions,shipActionsFlagship,shipActionsAttack,shipActionsCargo,shipActionsFast;
    List<List<Action>> ListofAllShipsActions;
    List<ShipUnit> shipList;

    public void ShipClicked(ShipUnit selectedShip)
    {
        //TODO Add to ShipUnit the splashart for the ship and the captain
        //TODO Understand type of ship and get all ships from the same fleet
        _shipSelected = selectedShip;

        //TODO Understand type of ship and get all ships from the same fleet (would be great to have the ships always in the same position es 1=flaghsip, 2=attack, 3=cargo, 4=fast
        /*
        shipList=_shipSelected.GetShips
        shipFlagship=shipList[0]
        shipActionsFlagship = shipFlagship.GetActions();
        ListofAllShipsActions.Add(shipActionsFlagship);

        shipAttack=shipList[1]
        shipActionsAttack = shipAttack.GetActions();
        ListofAllShipsActions.Add(shipActionsAttack);

        shipCargo=shipList[2]
        shipActionsCargo = shipCargo.GetActions();
        ListofAllShipsActions.Add(shipActionsCargo);

        shipFast=shipList[3]
        shipActionsFast = shipFast.GetActions();
        ListofAllShipsActions.Add(shipActionsFast);

         */
        shipActions = _shipSelected.GetActions();
        ChildEnable();
        ListofAllShipsActions.Add(shipActionsFlagship);

    }
    public void NoShipClicked()
    {
        ChildDisable();
    }

    private void ChildEnable()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        for (int i = 0; i < transform.GetChild(0).childCount; i++)
        {
            //TODO do this for each ship type

            //transform.GetChild(i).GetChild(0). change sprites
            transform.GetChild(0).GetChild(i).GetChild(0).GetComponentInChildren<Text>().text = "Attack Damage = " + _shipSelected.GetAttack;
            //TODO Add all other attributes 
            transform.GetChild(0).GetChild(i).GetChild(4).GetComponentInChildren<Text>().text = ListofAllShipsActions[i][1].name;
            transform.GetChild(0).GetChild(i).GetChild(5).GetComponentInChildren<Text>().text = ListofAllShipsActions[i][2].name;
        }
    }
    private void ChildDisable()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }
    //FLASHIP METHODS  (Change Selected Ship)
    public void ToggleCaptainFlagship() {//change sprite to Captain
    }
    public void ToggleShipFlagship() {//change sprite to Ship + Attributes 
    }
    public void HealShipFlagship(){_shipSelected.HealAtPortAction();}
    public void BasicAttackFlagship() { shipActions[0].Activate(_shipSelected);}
    public void RefuelFlagship(){_shipSelected.RefuelToMaxAtPortAction();}
    public void Action1Flagship(){shipActions[1].Activate(_shipSelected);}
    public void Action2Flagship(){shipActions[2].Activate(_shipSelected);}

    //AttackShip METHODS  (Change Selected Ship)
    public void ToggleCaptainAttack()
    {//change sprite to Captain
    }
    public void ToggleShipAttack()
    {//change sprite to Ship + Attributes 
    }
    public void HealShipAttack() { _shipSelected.HealAtPortAction(); }
    public void BasicAttackAttack() { shipActions[0].Activate(_shipSelected); }
    public void RefuelAttack() { _shipSelected.RefuelToMaxAtPortAction(); }
    public void Action1Attack() { shipActions[1].Activate(_shipSelected); }
    public void Action2Attack() { shipActions[2].Activate(_shipSelected); }

    //CARGOSHIP METHODS  (Change Selected Ship)
    public void ToggleCaptainCargo()
    {//change sprite to Captain
    }
    public void ToggleShipCargo()
    {//change sprite to Ship + Attributes 
    }
    public void HealShipCargo() { _shipSelected.HealAtPortAction(); }
    public void BasicAttackCargo() { shipActions[0].Activate(_shipSelected); }
    public void RefuelCargo() { _shipSelected.RefuelToMaxAtPortAction(); }
    public void Action1Cargo() { shipActions[1].Activate(_shipSelected); }
    public void Action2Cargo() { shipActions[2].Activate(_shipSelected); }


    //FastSHIP METHODS  (Change Selected Ship)
    public void ToggleCaptainFast()
    {//change sprite to Captain
    }
    public void ToggleShipFast()
    {//change sprite to Ship + Attributes 
    }
    public void HealShipFast() { _shipSelected.HealAtPortAction(); }
    public void BasicAttackFast() { shipActions[0].Activate(_shipSelected); }
    public void RefuelFast() { _shipSelected.RefuelToMaxAtPortAction(); }
    public void Action1CFast() { shipActions[1].Activate(_shipSelected); }
    public void Action2Fast() { shipActions[2].Activate(_shipSelected); }

}
