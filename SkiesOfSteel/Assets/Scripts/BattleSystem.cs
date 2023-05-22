using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum BattleState { START, PLAYERTURN, WON, LOST};

public class BattleSystem : MonoBehaviour
{

    public int numOfPlayers;

    public List<List<ShipUnit>> playersUnits;

    public List<ShipUnit> debugListPlayer1;
    public List<ShipUnit> debugListPlayer2;

    private BattleState battleState;

    private int currentPlayer;

    private int lastPlayer;

    private void Start()
    {
        battleState = BattleState.START;

        StartCoroutine(SetupBattle());
                
    }

    private IEnumerator SetupBattle()
    {
        currentPlayer = 0;
        lastPlayer = 0;

        playersUnits = new List<List<ShipUnit>>();
        //SetupShips dinamically when real game TODO
        playersUnits.Append(debugListPlayer1);
        playersUnits.Append(debugListPlayer2);


        yield return 2f;

        battleState = BattleState.PLAYERTURN;

        //continue setup


        EnableCurrentPlayer();
    }


    private void EnableCurrentPlayer()
    {
        foreach (ShipUnit unit in playersUnits[currentPlayer])
        {
            unit.EnableShip();
        }
    }



    private void Update()
    {
        if (battleState == BattleState.PLAYERTURN && lastPlayer != currentPlayer)
        {
            lastPlayer = currentPlayer;
            EnableCurrentPlayer();
        }
    }


    public void EndTurn()
    {
        currentPlayer = (currentPlayer + 1) % numOfPlayers;
    }
}
