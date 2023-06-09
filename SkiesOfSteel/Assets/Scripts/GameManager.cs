using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;


public enum BattleState { START, PLAYERTURN};

public class GameManager : NetworkBehaviour
{
    private NetworkVariable<ushort> _numOfPlayers = new NetworkVariable<ushort>();

    private NetworkVariable<ushort> _currentPlayer = new NetworkVariable<ushort>();

    private NetworkVariable<BattleState> _battleState = new NetworkVariable<BattleState>(BattleState.START);

    private ushort _lastPlayer = 0;


    private readonly ulong[] _singleTargetClientArray = new ulong[1];   // This array is used to sent call the clientRpc on one particular client

    [CanBeNull] public static event System.Action StartGameEvent;
    [CanBeNull] public static event System.Action LostGameEvent;
    [CanBeNull] public static event System.Action WonGameEvent;



    public void SetNumOfPlayers(ushort numOfPlayers)
    {
        _numOfPlayers.Value = numOfPlayers;
    }

    [ServerRpc]
    public void NewClientConnectedServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("GameManager:NewClientConnected: clientId = " + serverRpcParams.Receive.SenderClientId);
    }

    private void SetupGame()
    {
        //TODO setup ships
        for (int i = 0; i < _numOfPlayers.Value; i++)
        {

        }

        Debug.Log("Setupping game");

        _currentPlayer.Value = 0;
        _battleState.Value = BattleState.PLAYERTURN;

        EnableCurrentPlayer();
    }


    private void EnableCurrentPlayer()
    {
        /*
        foreach (ShipUnit unit in playersUnits[currentPlayer])
        {
            unit.EnableShip();
        }
        */
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        StartGameEvent.Invoke();
    }

    private void Update()
    {
        // At the start of the game, when the lobby is complete, the server setups the game
        if (IsServer && _battleState.Value == BattleState.START && _numOfPlayers.Value == NetworkManager.ConnectedClients.Count)
        {
            SetupGame();
            StartGameClientRpc();
        }

        
        if (_battleState.Value == BattleState.PLAYERTURN && _lastPlayer != _currentPlayer.Value)
        {
            _lastPlayer = _currentPlayer.Value;
            EnableCurrentPlayer();

            Debug.Log("Enabling next player");
        }

    }


    public void EndTurn()
    {
        _currentPlayer.Value = (ushort) ((_currentPlayer.Value + 1) % _numOfPlayers.Value);
    }


    [ClientRpc]
    private void GameLostClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("You lose!");

        LostGameEvent?.Invoke();

        NetworkManager.Singleton.Shutdown();
    }

    [ClientRpc]
    private void GameWonClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("You won!");

        WonGameEvent?.Invoke();

        NetworkManager.Singleton.Shutdown();
    }
}
