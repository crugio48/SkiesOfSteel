using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum BattleState { START, PLAYERTURN};

[RequireComponent(typeof(DemoBattleSpawner))]
public class GameManager : NetworkBehaviour
{
    [SerializeField] private ConnectionApprovalHandler connectionApprovalHandler;

    private DemoBattleSpawner _demoBattleSpawner;

    private NetworkVariable<int> _numOfPlayers = new NetworkVariable<int>();

    private NetworkVariable<int> _currentPlayer = new NetworkVariable<int>();

    private BattleState _battleState = BattleState.START;

    // This list of usernames will also be used as turn order so if a player quits or looses it needs to be removed from here TODO
    private NetworkList<FixedString32Bytes> _playerUsernames; // NetworkList must be initialized in awake

    private int _lastPlayer = 0;

    // This dictionary is created only on server
    private Dictionary<FixedString32Bytes, ulong> _usernameToClientIds;


    private readonly ulong[] _singleTargetClientArray = new ulong[1];   // This array is used to sent call the clientRpc on one particular client

    [CanBeNull] public static event System.Action<bool> UsernameSelected;
    [CanBeNull] public static event System.Action StartGameEvent;
    [CanBeNull] public static event System.Action LostGameEvent;
    [CanBeNull] public static event System.Action WonGameEvent;


    private void Awake()
    {
        // NetworkList must be initialized in awake
        _playerUsernames = new NetworkList<FixedString32Bytes>();
        _usernameToClientIds = new Dictionary<FixedString32Bytes, ulong>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _demoBattleSpawner = GetComponent<DemoBattleSpawner>();

        NetworkManager.Singleton.OnClientConnectedCallback += NewClientConnected;

        if (IsServer)
        {
            ShipUnit.ShipIsDestroyed += ShipGotDestroyed;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        NetworkManager.Singleton.OnClientConnectedCallback -= NewClientConnected;

        if (IsServer)
        {
            ShipUnit.ShipIsDestroyed -= ShipGotDestroyed;
        }
    }

    // Callback method to run on server and on the client that connected everytime a new client connects
    private void NewClientConnected(ulong newClientId)
    {
        Debug.Log("GameManager:NewClientConnected: newClientId = " + newClientId);
    }

    public void SetNumOfPlayers(int numOfPlayers)
    {
        _numOfPlayers.Value = numOfPlayers;
        connectionApprovalHandler.SetMaxPlayers(numOfPlayers);
    }

    // This method is called on the local GameManager of the player that selected the username
    public void SelectUsername(string username)
    {
        SelectUsernameServerRpc(username);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectUsernameServerRpc(string username, ServerRpcParams serverRpcParams = default)
    {
        ulong senderId = serverRpcParams.Receive.SenderClientId;

        ClientRpcParams clientRpcParams = CreateClientRpcParamsToSingleTargetClient(senderId);

        Debug.Log("Received: " + username);

        if (!_playerUsernames.Contains(username) && username.Length < 30)
        {
            _playerUsernames.Add(username);

            NetworkManager.Singleton.ConnectedClients[senderId].PlayerObject.GetComponent<Player>().SetUsername(username);

            _usernameToClientIds.Add(username, senderId);

            UsernameInsertedClientRpc(true, clientRpcParams);
            Debug.Log("Accepted: " + username);
        }
        else
        {
            UsernameInsertedClientRpc(false, clientRpcParams);
            Debug.Log("Rejected: " + username);
        }
    }

    [ClientRpc]
    private void UsernameInsertedClientRpc(bool outcome, ClientRpcParams clientRpcParams = default)
    { 
        Debug.Log("I selected the username with outcome = " + outcome);
        UsernameSelected.Invoke(outcome);
    }


    // This will be executd only by the server once to setup the game
    private void SetupGame()
    {
        // Spawn the ships
        _demoBattleSpawner.SpawnDemoShips(_playerUsernames, _numOfPlayers.Value);

        // Set first player
        _currentPlayer.Value = 0;

        // Start game and enable first player
        _battleState = BattleState.PLAYERTURN;
        EnableCurrentPlayer();
    }


    private void EnableCurrentPlayer()
    {
        FixedString32Bytes playerToEnable = _playerUsernames[_currentPlayer.Value];

        Debug.Log("Enabling the player: " + playerToEnable);

        foreach (ShipUnit shipUnit in PlayersShips.Instance.GetShips(playerToEnable))
        {
            shipUnit.EnableShip();
        }
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        StartGameEvent.Invoke();
    }

    private void Update()
    {
        // Only the server script can start the execution of methods in the update of GameManager
        if (!IsServer) return;


        // At the start of the game, when the lobby is complete and everyone chose a username, the server setups the game
        if (_battleState == BattleState.START && _numOfPlayers.Value == NetworkManager.ConnectedClients.Count && _playerUsernames.Count == _numOfPlayers.Value)
        {
            SetupGame();
            StartGameClientRpc();
        }

        
        if (_battleState == BattleState.PLAYERTURN && _lastPlayer != _currentPlayer.Value)
        {
            _lastPlayer = _currentPlayer.Value;

            EnableCurrentPlayer();
        }

    }


    public void EndTurnCalledByClient()
    {
        EndTurnServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void EndTurnServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong senderId = serverRpcParams.Receive.SenderClientId;

        if (_playerUsernames[_currentPlayer.Value] != NetworkManager.Singleton.ConnectedClients[senderId].PlayerObject.GetComponent<Player>().GetUsername())
        {
            Debug.LogError("A client that is not the current enabled player sent a request to end turn");
            return;
        }

        _currentPlayer.Value = (_currentPlayer.Value + 1) % _numOfPlayers.Value;
    }


    private void ShipGotDestroyed(ShipUnit shipUnit)
    {
        // Should already be called only on server but better to check
        if (!IsServer) return;

        if (shipUnit.IsFlagship())
        {
            FixedString32Bytes usernameOfPlayerThatLost = shipUnit.GetOwnerUsername();

            ClientRpcParams clientRpcParams = CreateClientRpcParamsToSingleTargetClient(_usernameToClientIds[usernameOfPlayerThatLost]);

            _numOfPlayers.Value -= 1;

            _playerUsernames.Remove(usernameOfPlayerThatLost);

            //TODO destroy all remaining ships of that player

            GameLostClientRpc(clientRpcParams);

            Debug.Log(usernameOfPlayerThatLost + " lost!");

        }

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



    private ClientRpcParams CreateClientRpcParamsToSingleTargetClient(ulong targetClientId)
    {
        _singleTargetClientArray[0] = targetClientId;

        return new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = _singleTargetClientArray
            }
        };
    }
}
