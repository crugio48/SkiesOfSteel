using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShipUI : MonoBehaviour
{

    private Canvas canvas;
    ShipUnit _shipSelected, shipFlagship, shipAttack, shipCargo, shipFast;
    List<Action> shipActionsFlagship, shipActionsAttack, shipActionsCargo, shipActionsFast;
    List<List<Action>> ListofAllShipsActions;
    List<ShipUnit> shipList;
    string playerName;
    //TODO Find InputManager & ActionInstructionCanvas
    [SerializeField]
    private InputManager inputManager;

    [SerializeField]
    private ActionInstructionCanvas actionInstructionCanvas;
    List<ShipUnit> targetList;
    private void Start()
    {
        canvas = GetComponent<Canvas>();
        ListofAllShipsActions = new List<List<Action>>();
        shipList = new List<ShipUnit>();
    }
    public void ShipClicked(ShipUnit selectedShip)
    {
        //TODO Add to ShipUnit the splashart for the ship and the captain
        _shipSelected = selectedShip;


        playerName = _shipSelected.GetOwnerUsername();
        shipList = PlayersShips.Instance.GetShips(playerName);

        shipFlagship = shipList[0];
        shipActionsFlagship = shipFlagship.GetActions();
        ListofAllShipsActions.Add(shipActionsFlagship);

        shipAttack = shipList[1];
        shipActionsAttack = shipAttack.GetActions();
        ListofAllShipsActions.Add(shipActionsAttack);

        shipCargo = shipList[2];
        shipActionsCargo = shipCargo.GetActions();
        ListofAllShipsActions.Add(shipActionsCargo);

        shipFast = shipList[3];
        shipActionsFast = shipFast.GetActions();
        ListofAllShipsActions.Add(shipActionsFast);
        ChildEnable();

    }
    public void NoShipClicked()
    {
        ChildDisable();
    }

    private void ChildEnable()
    {
        canvas.enabled = true;
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
        BasicAttack(shipFlagship,0);
    }
    public void Action1Flagship()
    {
        Action1(shipFlagship, 0);
    }
    public void Action2Flagship()
    {

        Action2(shipFlagship, 0);
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
        BasicAttack(shipAttack,1);
    }
    public void Action1Attack()
    {
        Action1(shipAttack, 1);
    }
    public void Action2Attack()
    {
        Action2(shipAttack, 1);
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

        BasicAttack(shipCargo,2);

    }
    public void Action1Cargo()
    {
        Action1(shipCargo, 2);
    }
    public void Action2Cargo()
    {

        Action2(shipCargo, 2);
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
        BasicAttack(shipFast,3);

    }
    public void Action1Fast()
    {
        Action1(shipFast, 3);
    }
    public void Action2Fast()
    {
        Action2(shipFast, 3);
    }

    public void ReceiveTargets(int indexAction, ShipUnit casterShip, List<ShipUnit> _targetList)
    {
        targetList = _targetList;

        NetworkBehaviourReference[] targetListNet = new NetworkBehaviourReference[targetList.Count];
        for (int i = 0; i < targetList.Count; i++)
        {
            targetListNet[i] = targetList[i];
        }
        //casterShip.ActivateActionServerRpc(indexAction, targetListNet, 0);
        targetList.Clear();
        actionInstructionCanvas.DisableCanvas();
    }

    public void BasicAttack(ShipUnit actionShip, int indexShip)
    {

        actionInstructionCanvas.ChangeTextDescription("Select 1 Target");
        //actionInstructionCanvas.ChangeActionDescription(ListofAllShipsActions[indexShip][0].description);
        actionInstructionCanvas.EnableCanvas();
        //inputManager.startLookingForTarget(actionShip, 0, 1);
        NetworkBehaviourReference[] targetListNet = new NetworkBehaviourReference[] { targetList[0] };
        actionShip.ActivateActionServerRpc(0, targetListNet, new Vector3Int[0], new Orientation[0], 0);

    }
    public void Action1(ShipUnit actionShip, int indexShip)
    {
        actionInstructionCanvas.ChangeActionDescription("Select " + ListofAllShipsActions[indexShip][2].amountOfTargets + " Target");
        //actionInstructionCanvas.ChangeActionDescription(ListofAllShipsActions[indexShip][1].description);
        actionInstructionCanvas.EnableCanvas();
        if (ListofAllShipsActions[indexShip][1].needsTarget == false)
        {
            //actionShip.ActivateActionServerRpc(1, null, 0);
        }
        else
        {
            if (ListofAllShipsActions[indexShip][1].isSelfOnly == true)
            {
                NetworkBehaviourReference[] targetListNet = new NetworkBehaviourReference[] { actionShip };
                //actionShip.ActivateActionServerRpc(1, targetListNet, 0);
            }
            else
            {
                actionInstructionCanvas.ChangeTextDescription("Select "+ ListofAllShipsActions[indexShip][1].amountOfTargets +" Target");
                //inputManager.startLookingForTarget(actionShip, 1, ListofAllShipsActions[indexShip][1].amountOfTargets);
            }
        }
    }
    public void Action2(ShipUnit actionShip, int indexShip)
    {
        actionInstructionCanvas.ChangeActionDescription("Select " + ListofAllShipsActions[indexShip][2].amountOfTargets + " Target");
        //actionInstructionCanvas.ChangeActionDescription(ListofAllShipsActions[indexShip][2].description);
        actionInstructionCanvas.EnableCanvas();
        if (ListofAllShipsActions[indexShip][2].needsTarget == false)
        {
            //actionShip.ActivateActionServerRpc(2, null, 0);
        }
        else
        {
            if (ListofAllShipsActions[indexShip][2].isSelfOnly == true)
            {
                NetworkBehaviourReference[] targetListNet = new NetworkBehaviourReference[] { actionShip };
                //actionShip.ActivateActionServerRpc(2, targetListNet, 0);
            }
            else
            {
                actionInstructionCanvas.ChangeTextDescription("Select " + ListofAllShipsActions[indexShip][2].amountOfTargets + " Target");                
                //inputManager.startLookingForTarget(actionShip, 2, ListofAllShipsActions[indexShip][2].amountOfTargets);
                
            }
        }



    }
}
