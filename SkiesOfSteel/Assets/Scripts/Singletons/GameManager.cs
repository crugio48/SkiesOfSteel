using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;


public enum BattleState { START, PLAYERTURN, END };


[RequireComponent(typeof(DemoBattleSpawner))]
public class GameManager : SingletonNetwork<GameManager>
{
    [SerializeField] private ConnectionApprovalHandler connectionApprovalHandler;

    private NetworkVariable<int> _numOfPlayers = new NetworkVariable<int>();

    private NetworkVariable<int> _currentPlayer = new NetworkVariable<int>();

    private NetworkList<FixedString32Bytes> _playerUsernames; // NetworkList must be initialized in awake

    private BattleState _battleState = BattleState.START;

    private int _lastPlayer = 0;

    // This dictionary is used only on server
    private Dictionary<FixedString32Bytes, ulong> _usernameToClientIds;

    [CanBeNull] public event System.Action<bool> UsernameSelected;
    [CanBeNull] public event System.Action StartGameEvent;
    [CanBeNull] public event System.Action LostGameEvent;
    [CanBeNull] public event System.Action WonGameEvent;

    // This will be used by the server only script to spawn and intialize the ships
    private DemoBattleSpawner _demoBattleSpawner;


    //----------------------------------- Overrides:

    public override void Awake()
    {
        base.Awake();

        // NetworkList must be initialized in awake
        _playerUsernames = new NetworkList<FixedString32Bytes>();
    }


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            _demoBattleSpawner = GetComponent<DemoBattleSpawner>();
            _usernameToClientIds = new Dictionary<FixedString32Bytes, ulong>();

            NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;

            ShipUnit.ShipIsDestroyed += ShipGotDestroyed;
            ShipUnit.ShipRetrievedTheTreasureAndWonGame += ShipRetrievedTreasure;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= ClientDisconnected;

            ShipUnit.ShipIsDestroyed -= ShipGotDestroyed;
            ShipUnit.ShipRetrievedTheTreasureAndWonGame -= ShipRetrievedTreasure;
        }
    }

    //----------------------------------- Lobby population and disconnections:

    // Callback when a client disconnects, the client will be deleted by NetworkManager AFTER this callback is run
    private void ClientDisconnected(ulong clientId)
    {
        Debug.Log("A client DCed, remaining players count PRE client deletion = " + NetworkManager.Singleton.ConnectedClients.Count);

        if (NetworkManager.Singleton.ConnectedClients.Count == 1)
        {
            return;
        }

        // If the second to last player disconnects then make last player remaining win and return
        if (_battleState == BattleState.PLAYERTURN && NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            int winnerId = -1;

            foreach(ulong id in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (id != clientId)
                {
                    winnerId = (int)id;
                }
            }
            if (winnerId == -1)
            {
                Debug.LogError("Should not happen that there is not the last player to assign the win to");
                return;
            }

            Debug.Log("Making the player with id " + winnerId + " win the game due to disconnections");
            MakePlayerWin((ulong)winnerId);
            return;
        }

        // The client will be deleted by NetworkManager AFTER this callback is run so I can still retrieve his username from the player gameObject
        FixedString32Bytes dcUsername = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<Player>().GetUsername();

        Debug.Log("DcUsername = " + dcUsername);

        RemovePlayerFromGameLogic(dcUsername);

    }


    // Server only method to setup the number of players of the match
    public void SetNumOfPlayers(int numOfPlayers)
    {
        _numOfPlayers.Value = numOfPlayers;
        connectionApprovalHandler.SetMaxPlayers(numOfPlayers);
    }

    // A connected client calls this to try to register a username, the server will answer in a clientRpc with a response
    [ServerRpc(RequireOwnership = false)]
    public void SelectUsernameServerRpc(FixedString32Bytes username, ServerRpcParams serverRpcParams = default)
    {
        ulong senderId = serverRpcParams.Receive.SenderClientId;

        ClientRpcParams clientRpcParams = CreateClientRpcParamsTargetClients(new ulong[] {senderId});

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

    // Response of the server to a user request of username
    [ClientRpc]
    private void UsernameInsertedClientRpc(bool outcome, ClientRpcParams clientRpcParams = default)
    { 
        Debug.Log("I selected the username with outcome = " + outcome);
        UsernameSelected?.Invoke(outcome);
    }


    //----------------------------------- Game setup after every player setup their username:

    [ClientRpc]
    private void StartGameClientRpc()
    {
        StartGameEvent?.Invoke();
    }

    // This will be executd only by the server once to setup the game
    private void SetupGame()
    {
        // Spawn the ships
        _demoBattleSpawner.SpawnDemoShips(_playerUsernames, _numOfPlayers.Value, _usernameToClientIds);

        // Set first player
        _currentPlayer.Value = 0;

        // Start game and enable first player
        _battleState = BattleState.PLAYERTURN;
        EnableCurrentPlayer();
    }


    //----------------------------------- Turn logic:

    // ServerOnly method that enables the current player
    private void EnableCurrentPlayer()
    {
        FixedString32Bytes playerToEnable = _playerUsernames[_currentPlayer.Value];

        Debug.Log("Enabling the player: " + playerToEnable);

        foreach (ShipUnit shipUnit in PlayersShips.Instance.GetShips(playerToEnable))
        {
            shipUnit.EnableShip();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndTurnServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong senderId = serverRpcParams.Receive.SenderClientId;

        if (_playerUsernames[_currentPlayer.Value] != NetworkManager.Singleton.ConnectedClients[senderId].PlayerObject.GetComponent<Player>().GetUsername())
        {
            Debug.LogError("A client that is not the current enabled player sent a request to end turn");
            return;
        }

        _currentPlayer.Value = (_currentPlayer.Value + 1) % _numOfPlayers.Value;
    }

    //----------------------------------- Update method:

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

        // After a client ended their turn we enable the next player
        if (_battleState == BattleState.PLAYERTURN && _lastPlayer != _currentPlayer.Value)
        {
            _lastPlayer = _currentPlayer.Value;

            EnableCurrentPlayer();
        }

    }


    //----------------------------------- Game logic methods and callbacks:

    // ServerOnly callback when a ship health goes down to zero 
    private void ShipGotDestroyed(ShipUnit shipUnit)
    {
        // Should already be called only on server but better to check
        if (!IsServer) return;

        if (shipUnit.IsFlagship())
        {
            FixedString32Bytes usernameOfPlayerThatLost = shipUnit.GetOwnerUsername();

            ClientRpcParams clientRpcParams = CreateClientRpcParamsTargetClients(new ulong[] { _usernameToClientIds[usernameOfPlayerThatLost] });

            RemovePlayerFromGameLogic(usernameOfPlayerThatLost);

            GameLostClientRpc(clientRpcParams);

            Debug.Log(usernameOfPlayerThatLost + " lost!");
        }

    }

    // Destroys the ships of the player and removes him from the turn logic
    private void RemovePlayerFromGameLogic(FixedString32Bytes player)
    {
        foreach (ShipUnit ship in PlayersShips.Instance.GetShips(player))
        {
            ship.SetDestroyed();
        }

        RemovePlayerFromTurnLogic(player);
    }

    // Updates the turn logic after removing player
    private void RemovePlayerFromTurnLogic(FixedString32Bytes player)
    {
        if (!_playerUsernames.Contains(player)) // If Player already got removed then don't do anything
        {
            return;
        }

        int turnOrderOfPlayer = -1;

        for (int i = 0; i < _playerUsernames.Count; i++)
        {
            if (player == _playerUsernames[i])
            {
                turnOrderOfPlayer = i;
            }
        }

        if (turnOrderOfPlayer == -1)
        {
            Debug.LogError("Error in managing the players usernames and turn orders");
        }

        _playerUsernames.Remove(player); // This maintains the relative order of the remaining usernames

        // 3 cases based on this distinction:
        if (turnOrderOfPlayer > _currentPlayer.Value)
        {
            // Easy case
            _numOfPlayers.Value -= 1;
        }
        else if (turnOrderOfPlayer == _currentPlayer.Value)
        {
            _numOfPlayers.Value -= 1;

            // In this case the player lost during its turn so we need to fake the endTurn logic 
            _currentPlayer.Value = (_currentPlayer.Value) % _numOfPlayers.Value;
            _lastPlayer = -1;

        }
        else // turnOrderOfPlayer < _currentPlayer.Value
        {
            _numOfPlayers.Value -= 1;

            // We want the current player to keep playing
            _currentPlayer.Value -= 1;
            _lastPlayer = _currentPlayer.Value;
        }

    }


    // ServerOnly callback when a ship wins the game by retrieving the treasure, sends the correct ClientRpcs for winning or losing the game
    private void ShipRetrievedTreasure(ShipUnit shipUnit)
    {
        // Calling winning method on winner client
        ulong winnerId = _usernameToClientIds[shipUnit.GetOwnerUsername()];
        MakePlayerWin(winnerId);


        // Calling looser method on loosers clients
        ulong[] loosersIds = new ulong[_numOfPlayers.Value - 1];
        int i = 0;
        foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (id != winnerId)
            {
                loosersIds[i] = id;
                i++;
            }
        }

        ClientRpcParams clientRpcParams = CreateClientRpcParamsTargetClients(loosersIds);

        GameLostClientRpc(clientRpcParams);

    }

    // Server sends a ClientRpc to the client that won the game and sets the _battleState to END
    private void MakePlayerWin(ulong winnerId)
    {
        ClientRpcParams clientRpcParams = CreateClientRpcParamsTargetClients(new ulong[] { winnerId });

        GameWonClientRpc(clientRpcParams);

        _battleState = BattleState.END;
    }



    // ClientRpc of loosing the game
    [ClientRpc]
    private void GameLostClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("You lose!");

        LostGameEvent?.Invoke();

        NetworkManager.Singleton.Shutdown();
    }

    // ClientRpc of winning the game
    [ClientRpc]
    private void GameWonClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("You won!");

        WonGameEvent?.Invoke();

        NetworkManager.Singleton.Shutdown();
    }

    //----------------------------------- Custom methods:


    // Custom function to create the ClientRpc destination clients structure
    private ClientRpcParams CreateClientRpcParamsTargetClients(ulong[] targetClientsIds)
    {
        return new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = targetClientsIds
            }
        };
    }


    //----------------------------------- Getter methods:

    public bool HasBattleStarted()
    {
        if (_battleState != BattleState.START)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
