using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;


public enum BattleState { START, PLAYERTURN, END };


[RequireComponent(typeof(DemoBattleSpawner))]
public class GameManager : SingletonNetwork<GameManager>
{
    [SerializeField] private TurnCanvas turnCanvas;

    [SerializeField] private ServerCanvasUI serverCanvasUI;

    [SerializeField] private UIStartGame uiStartGame;

    [SerializeField] private int mapNumOfPlayers;

    private List<string> _playerUsernames;

    private BattleState _battleState = BattleState.START;

    private int _numOfPlayers;

    private int _currentPlayer = 0;

    private int _lastPlayer = 0;

    // This dictionary is used only on server
    private Dictionary<string, ulong> _usernameToClientIds;

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

        _playerUsernames = new List<string>();
    }


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            SetNumOfPlayers(mapNumOfPlayers);

            serverCanvasUI.EnableCanvas();

            _demoBattleSpawner = GetComponent<DemoBattleSpawner>();
            _usernameToClientIds = new Dictionary<string, ulong>();

            NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;

            ShipUnit.ShipIsDestroyed += ShipGotDestroyed;
            ShipUnit.ShipRetrievedTheTreasureAndWonGame += ShipRetrievedTreasure;
        }

        else if (IsClient)
        {
            uiStartGame.EnableCanvas();
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
        string dcUsername = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<Player>().GetUsername();

        Debug.Log("DcUsername = " + dcUsername);

        RemovePlayerFromGameLogic(dcUsername);

    }


    // Server only method to setup the number of players of the match
    private void SetNumOfPlayers(int numOfPlayers)
    {
        _numOfPlayers = numOfPlayers;
        NetworkManager.Singleton.GetComponent<ConnectionApprovalHandler>().SetMaxPlayers(numOfPlayers);
    }

    // A connected client calls this to try to register a username, the server will answer in a clientRpc with a response
    [ServerRpc(RequireOwnership = false)]
    public void SelectUsernameServerRpc(string username, ServerRpcParams serverRpcParams = default)
    {
        ulong senderId = serverRpcParams.Receive.SenderClientId;

        ClientRpcParams clientRpcParams = CreateClientRpcParamsTargetClients(new ulong[] {senderId});

        Debug.Log("Received: " + username);

        if (!_playerUsernames.Contains(username) && username.Length <= 15)
        {
            _playerUsernames.Add(username);

            NetworkManager.Singleton.ConnectedClients[senderId].PlayerObject.GetComponent<Player>().SetUsername(username);

            NetworkManager.Singleton.ConnectedClients[senderId].PlayerObject.GetComponent<Player>().SetUsernameClientRpc(username, clientRpcParams);

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

    // This will be executd only by the server once to setup the game
    private void SetupGame()
    {
        UpdateNumOfPlayersClientRpc(_numOfPlayers);
        SetPlayerUsernamesClientRpc(new NetworkStringArray(_playerUsernames));

        // Spawn the ships
        _demoBattleSpawner.SpawnDemoShips(_playerUsernames, _numOfPlayers, _usernameToClientIds);

        // Start game and enable first player
        _battleState = BattleState.PLAYERTURN;
        EnableCurrentPlayer();
    }


    //----------------------------------- Turn logic:

    // ServerOnly method that enables the current player
    private void EnableCurrentPlayer()
    {
        string playerToEnable = _playerUsernames[_currentPlayer];

        Debug.Log("Enabling the player: " + playerToEnable);

        foreach (ShipUnit shipUnit in PlayersShips.Instance.GetShips(playerToEnable))
        {
            Debug.Log("Enabling ship: " + shipUnit.name);

            shipUnit.EnableShip();
        }
    }

    private void DisableCurrentPlayer()
    {
        string playerToDisable = _playerUsernames[_currentPlayer];

        Debug.Log("Disabling the player: " + playerToDisable);

        foreach (ShipUnit shipUnit in PlayersShips.Instance.GetShips(playerToDisable))
        {
            shipUnit.DisableShip();
            if (shipUnit.GetCurrentFuel() > 0) shipUnit.RemoveFuel(1);
        }
    }


    // Called by the client
    public void PassTurn()
    {
        EndTurnServerRpc();
    }


    [ServerRpc(RequireOwnership = false)]
    private void EndTurnServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong senderId = serverRpcParams.Receive.SenderClientId;

        if (_playerUsernames[_currentPlayer] != NetworkManager.Singleton.ConnectedClients[senderId].PlayerObject.GetComponent<Player>().GetUsername())
        {
            Debug.LogError("A client that is not the current enabled player sent a request to end turn");
            return;
        }

        DisableCurrentPlayer();

        _currentPlayer = (_currentPlayer + 1) % _numOfPlayers;
        UpdateCurrentPlayerClientRpc(_currentPlayer);
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        StartCoroutine(InvokeStartGameAfterDelay());
    }

    private IEnumerator InvokeStartGameAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);

        StartGameEvent?.Invoke();
    }

    //----------------------------------- Update method:

    private void Update()
    {
        // Only the server script can start the execution of methods in the update of GameManager
        if (!IsServer) return;


        // At the start of the game, when the lobby is complete and everyone chose a username, the server setups the game
        if (_battleState == BattleState.START && _numOfPlayers == NetworkManager.ConnectedClients.Count && _playerUsernames.Count == _numOfPlayers)
        {
            SetupGame();

            StartGameClientRpc();
        }

        // After a client ended their turn we enable the next player
        if (_battleState == BattleState.PLAYERTURN && _lastPlayer != _currentPlayer)
        {
            _lastPlayer = _currentPlayer;

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
            string usernameOfPlayerThatLost = shipUnit.GetOwnerUsername();

            ClientRpcParams clientRpcParams = CreateClientRpcParamsTargetClients(new ulong[] { _usernameToClientIds[usernameOfPlayerThatLost] });

            RemovePlayerFromGameLogic(usernameOfPlayerThatLost);

            GameLostClientRpc(clientRpcParams);

            Debug.Log(usernameOfPlayerThatLost + " lost!");
        }

    }

    // Destroys the ships of the player and removes him from the turn logic
    private void RemovePlayerFromGameLogic(string player)
    {
        foreach (ShipUnit ship in PlayersShips.Instance.GetShips(player))
        {
            ship.SetDestroyed();
        }

        RemovePlayerFromTurnLogic(player);
    }

    // Updates the turn logic after removing player
    private void RemovePlayerFromTurnLogic(string player)
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
        if (turnOrderOfPlayer > _currentPlayer)
        {
            // Easy case
            _numOfPlayers -= 1;
        }
        else if (turnOrderOfPlayer == _currentPlayer)
        {
            _numOfPlayers -= 1;
            

            // In this case the player lost during its turn so we need to fake the endTurn logic 
            _currentPlayer = (_currentPlayer) % _numOfPlayers;
            _lastPlayer = -1;

        }
        else // turnOrderOfPlayer < _currentPlayer.Value
        {
            _numOfPlayers -= 1;

            // We want the current player to keep playing
            _currentPlayer -= 1;
            _lastPlayer = _currentPlayer;
        }

        UpdateNumOfPlayersClientRpc(_numOfPlayers);

        RemovePlayerAndUpdateCurrentPlayerClientRpc(player, _currentPlayer);
    }


    // ServerOnly callback when a ship wins the game by retrieving the treasure, sends the correct ClientRpcs for winning or losing the game
    private void ShipRetrievedTreasure(ShipUnit shipUnit)
    {
        // Calling winning method on winner client
        ulong winnerId = _usernameToClientIds[shipUnit.GetOwnerUsername()];
        MakePlayerWin(winnerId);


        // Calling looser method on loosers clients
        ulong[] loosersIds = new ulong[_numOfPlayers - 1];
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

        serverCanvasUI.EnableBackToMainMenuButton();
    }



    // ClientRpc of loosing the game
    [ClientRpc]
    private void GameLostClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("You lose!");

        NetworkManager.Singleton.Shutdown();

        LostGameEvent?.Invoke();
    }

    // ClientRpc of winning the game
    [ClientRpc]
    private void GameWonClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("You won!");

        NetworkManager.Singleton.Shutdown();

        WonGameEvent?.Invoke();
    }


    [ClientRpc]
    private void UpdateCurrentPlayerClientRpc(int newCurrentPlayer)
    {
        _currentPlayer = newCurrentPlayer;

        turnCanvas.CurrentPlayerChanged(newCurrentPlayer);
    }

    [ClientRpc]
    private void UpdateNumOfPlayersClientRpc(int newNumOfPlayers)
    {
        _numOfPlayers = newNumOfPlayers;
    }


    [ClientRpc]
    private void SetPlayerUsernamesClientRpc(NetworkStringArray newUsernames)
    {
        _playerUsernames = new List<string>(new string[newUsernames.GetLenght()]);
        
        for (int i = 0; i < newUsernames.GetLenght(); i++)
        {
            _playerUsernames[i] = newUsernames.GetIthUsername(i);
            Debug.Log(_playerUsernames[i]);
        }

        turnCanvas.SetPlayerNames(_playerUsernames);
    }

    [ClientRpc]
    private void RemovePlayerAndUpdateCurrentPlayerClientRpc(string playerUsername, int newCurrentPlayer)
    {
        _playerUsernames.Remove(playerUsername);

        _currentPlayer = newCurrentPlayer;

        turnCanvas.RemovePlayerAndUpdateCanvas(playerUsername, newCurrentPlayer);
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

    public string GetCurrentPlayer()
    {
        return _playerUsernames[_currentPlayer];
    }
}



public struct NetworkStringArray : INetworkSerializable
{
    public string[] Array;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        var length = 0;
        if (!serializer.IsReader)
            length = Array.Length;

        serializer.SerializeValue(ref length);

        if (serializer.IsReader)
            Array = new string[length];

        for (var n = 0; n < length; ++n)
            serializer.SerializeValue(ref Array[n]);
    }


    public NetworkStringArray (List<string> list)
    {
        Array = new string[list.Count];

        for (int i = 0; i < list.Count; ++i)
        {
            Array[i] = list[i];
        }
    }

    public int GetLenght()
    {
        return Array.Length;
    }


    public string GetIthUsername(int i)
    {
        return Array[i];
    }
}