using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;


public enum BattleState { START, PLAYERTURN};

public class GameManager : NetworkBehaviour
{
    [SerializeField] private GameObject shipUnitPrefab;
    [SerializeField] private ConnectionApprovalHandler connectionApprovalHandler;

    private NetworkVariable<ushort> _numOfPlayers = new NetworkVariable<ushort>();

    private NetworkVariable<ushort> _currentPlayer = new NetworkVariable<ushort>();

    private NetworkVariable<BattleState> _battleState = new NetworkVariable<BattleState>(BattleState.START);

    private NetworkList<FixedString32Bytes> _playerUsernames; // NetworkList must be initialized in awake

    private ushort _lastPlayer = 0;


    private readonly ulong[] _singleTargetClientArray = new ulong[1];   // This array is used to sent call the clientRpc on one particular client

    [CanBeNull] public static event System.Action<bool> UsernameSelected;
    [CanBeNull] public static event System.Action StartGameEvent;
    [CanBeNull] public static event System.Action LostGameEvent;
    [CanBeNull] public static event System.Action WonGameEvent;


    private void Awake()
    {
        // NetworkList must be initialized in awake
        _playerUsernames = new NetworkList<FixedString32Bytes>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        NetworkManager.Singleton.OnClientConnectedCallback += NewClientConnected;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        NetworkManager.Singleton.OnClientConnectedCallback -= NewClientConnected;
    }

    // Callback method to run on server and on the client that connected everytime a new client connects
    private void NewClientConnected(ulong newClientId)
    {
        Debug.Log("GameManager:NewClientConnected: newClientId = " + newClientId);
    }

    public void SetNumOfPlayers(ushort numOfPlayers)
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

        _singleTargetClientArray[0] = senderId;
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = _singleTargetClientArray
            }
        };

        Debug.Log("Received: " + username);

        if (!_playerUsernames.Contains(username) && username.Length < 32)
        {
            _playerUsernames.Add(username);

            NetworkManager.Singleton.ConnectedClients[senderId].PlayerObject.GetComponent<Player>().SetUsername(username);

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



    private void SetupGame()
    {

        //Setup ships for demo match
        for (int i = 0; i < _numOfPlayers.Value; i++)
        {
            Debug.Log("Spawning ships for player = " + _playerUsernames[i]);

            
            GameObject testShip = Instantiate(shipUnitPrefab, new Vector3(i,i,0), Quaternion.identity);
            testShip.GetComponent<NetworkObject>().Spawn();
            ShipUnit testShipUnit = testShip.GetComponent<ShipUnit>();
            testShipUnit.SetShipScriptableObject("ShipsScriptableObjects/TesterShip");
            testShipUnit.SetOwnerUsername(_playerUsernames[i]);

            
        }

        Debug.Log(NetworkManager.Singleton.ConnectedClients);

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
        // At the start of the game, when the lobby is complete and everyone chose a username, the server setups the game
        if (IsServer && _battleState.Value == BattleState.START && _numOfPlayers.Value == NetworkManager.ConnectedClients.Count && _playerUsernames.Count == _numOfPlayers.Value)
        {
            SetupGame();
            StartGameClientRpc();
        }

        
        if (IsServer && _battleState.Value == BattleState.PLAYERTURN && _lastPlayer != _currentPlayer.Value)
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
