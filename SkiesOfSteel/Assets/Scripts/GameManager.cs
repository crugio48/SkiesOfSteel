using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;


public enum BattleState { START, PLAYERTURN};

public class GameManager : NetworkBehaviour
{
    [SerializeField] private GameObject shipUnitPrefab;

    private NetworkVariable<ushort> _numOfPlayers = new NetworkVariable<ushort>();

    private NetworkVariable<ushort> _currentPlayer = new NetworkVariable<ushort>();

    private NetworkVariable<BattleState> _battleState = new NetworkVariable<BattleState>(BattleState.START);

    private List<ulong> _clientIds;

    private ushort _lastPlayer = 0;


    private readonly ulong[] _singleTargetClientArray = new ulong[1];   // This array is used to sent call the clientRpc on one particular client

    [CanBeNull] public static event System.Action StartGameEvent;
    [CanBeNull] public static event System.Action LostGameEvent;
    [CanBeNull] public static event System.Action WonGameEvent;



    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            _clientIds = new List<ulong>();
            NetworkManager.Singleton.OnClientConnectedCallback += NewClientConnected;
        }

    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= NewClientConnected;
        }
    }

    // Callback method to run on server everytime a new client connects
    private void NewClientConnected(ulong newClientId)
    {
        Debug.Log("GameManager:NewClientConnected: newClientId = " + newClientId);

        _clientIds.Append(newClientId);
    }

    public void SetNumOfPlayers(ushort numOfPlayers)
    {
        _numOfPlayers.Value = numOfPlayers;
    }

    private void SetupGame()
    {
        //Setup ships for demo match
        foreach (ulong id in NetworkManager.Singleton.ConnectedClients.Keys)
        {
            Debug.Log("PRINTING id = " + id);

            // TODO complete this code with initial spawn of ;
            if (NetworkManager.Singleton.ConnectedClients[id].PlayerObject.TryGetComponent(out Player player))
            {
                GameObject testShip = Instantiate(shipUnitPrefab, new Vector3(id,id,0), Quaternion.identity);
                testShip.GetComponent<NetworkObject>().Spawn();
                testShip.GetComponent<ShipUnit>().SetShipScriptableObject("ShipsScriptableObjects/TesterShip");

                player.SetShips(new List<ShipUnit>
                {
                    testShip.GetComponent<ShipUnit>()
                });
            }
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
