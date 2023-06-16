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


    private Canvas canvas;

    ShipUnit _shipSelected, shipFlagship, shipAttack, shipCargo, shipFast;

    List<List<Action>> ListofAllShipsActions;

    List<ShipUnit> shipList;

    string playerName;


    private void Start()
    {
        canvas = GetComponent<Canvas>();
        ListofAllShipsActions = new List<List<Action>>();
    }
    public void ShipClicked(ShipUnit selectedShip)
    {
        //TODO Add to ShipUnit the splashart for the ship and the captain
        _shipSelected = selectedShip;


        playerName = _shipSelected.GetOwnerUsername();
        shipList = PlayersShips.Instance.GetShips(playerName);
        ListofAllShipsActions.Clear();


        shipFlagship = shipList[0];
        ListofAllShipsActions.Add(shipFlagship.GetActions());

        shipAttack = shipList[1];
        ListofAllShipsActions.Add(shipAttack.GetActions());

        shipCargo = shipList[2];
        ListofAllShipsActions.Add(shipCargo.GetActions());

        shipFast = shipList[3];
        ListofAllShipsActions.Add(shipFast.GetActions());
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
            //TODO Get Sprites

            //transform.GetChild(i).GetChild(0). change sprites

            transform.GetChild(i).GetChild(0).GetComponentInChildren<Text>().text = "Health = " + shipList[i].GetCurrentHealth() + " / " + shipList[i].GetMaxHealth() +
                                                                                                "\nFuel = " + shipList[i].GetCurrentFuel() + " / " + shipList[i].GetMaxFuel() +
                                                                                                "\nCurrent Bonus Attack Stage = " + shipList[i].GetAttackStage() +
                                                                                                "\nCurrent Bonus Defence Stage = " + shipList[i].GetDefenseStage() +
                                                                                                "\nMovements Left = " + shipList[i].GetMovementLeft();

            transform.GetChild(i).GetChild(4).GetComponentInChildren<TextMeshProUGUI>().text = ListofAllShipsActions[i][1].name;
            transform.GetChild(i).GetChild(5).GetComponentInChildren<TextMeshProUGUI>().text = ListofAllShipsActions[i][2].name;
        }

        canvas.enabled = true;
    }

    private void ChildDisable()
    {
        canvas.enabled = false;
    }


    //FLASHIP METHODS  (Change Selected Ship)
    public void ToggleCaptainFlagship()
    {//TODO change sprite to Captain
    }
    public void ToggleShipFlagship()
    {//TODO change sprite to Ship + Attributes 
    }
    public void HealShipFlagship()
    {
        shipFlagship.HealActionServerRpc();
    }
    public void RefuelFlagship()
    {
        shipFlagship.RefuelToMaxAtPortActionServerRpc();
    }
    public void BasicAttackFlagship()
    {
        ActionSelected(shipFlagship,0);
    }
    public void Action1Flagship()
    {
        ActionSelected(shipFlagship, 1);
    }
    public void Action2Flagship()
    {

        ActionSelected(shipFlagship, 2);
    }



    //AttackShip METHODS  (Change Selected Ship)
    public void ToggleCaptainAttack()
    {//TODO change sprite to Captain
    }
    public void ToggleShipAttack()
    {//TODO change sprite to Ship + Attributes 
    }
    public void HealShipAttack()
    {         
        shipAttack.HealActionServerRpc();
    }
    public void RefuelAttack()
    {
        shipAttack.RefuelToMaxAtPortActionServerRpc();
    }
    public void BasicAttackAttack()
    {
        ActionSelected(shipAttack, 0);
    }
    public void Action1Attack()
    {
        ActionSelected(shipAttack, 1);
    }
    public void Action2Attack()
    {
        ActionSelected(shipAttack, 2);
    }

    //CARGOSHIP METHODS  (Change Selected Ship)
    public void ToggleCaptainCargo()
    {//TODO change sprite to Captain
    }
    public void ToggleShipCargo()
    {//TODO change sprite to Ship + Attributes 
    }

    public void HealShipCargo()
    {
        shipCargo.HealActionServerRpc();
    }
    public void RefuelCargo()
    {
        
        shipCargo.RefuelToMaxAtPortActionServerRpc();
    }

    public void BasicAttackCargo()
    {
        ActionSelected(shipCargo, 0);
    }

    public void Action1Cargo()
    {
        ActionSelected(shipCargo, 1);
    }

    public void Action2Cargo()
    {

        ActionSelected(shipCargo, 2);
    }


    //FastSHIP METHODS  (Change Selected Ship)
    public void ToggleCaptainFast()
    {//TODO change sprite to Captain
    }
    public void ToggleShipFast()
    {//TODO change sprite to Ship + Attributes 
    }
    public void HealShipFast()
    {
        shipFast.HealActionServerRpc();
    }
    public void RefuelFast()
    {
        shipFast.RefuelToMaxAtPortActionServerRpc();
    }
    public void BasicAttackFast()
    {
        ActionSelected(shipFast, 0);

    }
    public void Action1Fast()
    {
        ActionSelected(shipFast, 1);
    }
    public void Action2Fast()
    {
        ActionSelected(shipFast, 2);
    }


    public void ActionSelected(ShipUnit actionShip, int actionIndex)
    {
        actionInstructionCanvas.ActionOfShipSelected(actionShip, actionIndex);
    }
    
}
