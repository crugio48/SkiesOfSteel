using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;


[RequireComponent(typeof(SpriteRenderer))]
public class ShipUnit : NetworkBehaviour
{
    private ShipScriptableObject _shipSO;

    private string _ownerUsername;

    private NetworkVariable<int> _currentHealth = new NetworkVariable<int>();
    private NetworkVariable<int> _currentFuel = new NetworkVariable<int>();
    private NetworkVariable<int> _attackStage = new NetworkVariable<int>(0);
    private NetworkVariable<int> _defenseStage = new NetworkVariable<int>(0);

    private NetworkVariable<int> _oneTurnTemporaryAttackStage = new NetworkVariable<int>(0);
    private NetworkVariable<int> _oneTurnTemporaryDefenseStage = new NetworkVariable<int>(0);

    private int MAX_STAGE = 6;
    private int MIN_STAGE = -6;

    private NetworkVariable<bool> _canDoAction = new NetworkVariable<bool>(false);
    private NetworkVariable<int> _movementLeft = new NetworkVariable<int>(0);

    private static Vector3Int NOT_SET_POSITION = new Vector3Int(-1000, -1000, -1000);
    private NetworkVariable<Vector3IntSerializable> _currentPosition = new NetworkVariable<Vector3IntSerializable>(NOT_SET_POSITION);

    private NetworkVariable<bool> _isDestroyed = new NetworkVariable<bool>(false);

    private SpriteRenderer _spriteRenderer;
    private Tilemap _tilemap;

    [CanBeNull] public static event System.Action<ShipUnit> ShipIsDestroyed;
    [CanBeNull] public static event System.Action<ShipUnit> ShipRetrievedTheTreasureAndWonGame;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _tilemap = FindObjectOfType<Tilemap>();
        _spriteRenderer = GetComponent<SpriteRenderer>();


        // Run both on server and on clients the callbacks that update the info databases classes
        _currentPosition.OnValueChanged += PositionChangedCallback;
        _isDestroyed.OnValueChanged += HideShip;
    }


    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        _currentPosition.OnValueChanged -= PositionChangedCallback;
        _isDestroyed.OnValueChanged -= HideShip;
    }

    //----------------------------------- Callback methods of onValueChanged:


    // Callback of _currentPosition.OnValueChanged
    private void PositionChangedCallback(Vector3IntSerializable previousValue, Vector3IntSerializable newValue)
    {
        if (previousValue.GetValues() == NOT_SET_POSITION)
        {
            Debug.Log("Placing the ship = " + this.name + " in grid position  = " + newValue.GetValues());

            ShipsPositions.Instance.Place(this, newValue.GetValues());
        }
        else
        {
            Debug.Log("Moving the ship = " + this.name + " from grid position = " + previousValue.GetValues() + " to grid position = " + newValue.GetValues());

            ShipsPositions.Instance.Move(this, previousValue.GetValues(), newValue.GetValues());
        }
    }

    // Callback of _isDestroyed.OnValueChanged
    private void HideShip(bool previousValue, bool newValue)
    {
        // If ship got destroyed then hide it from the game visually and from the position dictionary
        if (previousValue == false && newValue == true)
        {
            _spriteRenderer.enabled = false;

            ShipsPositions.Instance.RemoveShip(_currentPosition.Value.GetValues());

            if (Treasure.Instance.GetCarryingShip() == this) Treasure.Instance.RemoveCarryingShip();

        }
    }

    //----------------------------------- Methods run at the start of the game when the server spawns the ships:

    // Only the server will be running this function once
    public void SetShipScriptableObject(string shipSOpath)
    {
        SetInitialValuesFromSO(shipSOpath);

        // Make clients also set the shipSO
        SetShipScriptableObjectClientRpc(shipSOpath);
    }

    // Set also on the clients ships the scriptable object
    [ClientRpc]
    private void SetShipScriptableObjectClientRpc(string shipSOpath)
    {
        SetInitialValuesFromSO(shipSOpath);
    }

    // Actual method that sets the ships scriptable value object
    private void SetInitialValuesFromSO(string shipSOpath)
    {       
        _shipSO = Resources.Load<ShipScriptableObject>(shipSOpath);

        if (IsServer)
        {
            _currentHealth.Value = _shipSO.maxHealth;
            _currentFuel.Value = _shipSO.maxFuel;
        }

        _spriteRenderer.sprite = _shipSO.sprite; // TODO implement logic for more sprites
    }


    // Only the server will be running this function once
    public void SetOwnerUsername(string username)
    {
        _ownerUsername = username;

        PlayersShips.Instance.SetShip(username, this);

        // Set also client information
        SetOwnerUsernameClientRpc(username);
    }

    [ClientRpc]
    public void SetOwnerUsernameClientRpc(string username)
    {
        _ownerUsername = username;
        PlayersShips.Instance.SetShip(username, this);
    }

    // Only the server will be running this function once
    public void SetInitialGridPosition(Vector3Int gridPos)
    {
        _currentPosition.Value = gridPos;
    }


    //----------------------------------- Private custom getters that are used for the ship stats logic:

    // Get attack value modified by attack stage
    private float GetAttack
    {
        get
        {
            return Mathf.Floor(_shipSO.attack * GetMultiplier(_attackStage.Value + _oneTurnTemporaryAttackStage.Value));
        }
    }

    // Get defense value modified by defense stage
    private float GetDefense
    {
        get
        {
            return Mathf.Floor(_shipSO.defense * GetMultiplier(_defenseStage.Value + _oneTurnTemporaryDefenseStage.Value));
        }
    }

    // Get multiplier value given the multiplier stage
    private float GetMultiplier(int stage)
    {
        //taking example from: https://www.dragonflycave.com/mechanics/stat-stages

        // Limit the stage between -6 and +6
        stage = Mathf.Min(MAX_STAGE, stage);
        stage = Mathf.Max(MIN_STAGE, stage);

        return stage switch
        {
            -6 => 0.25f,
            -5 => 0.28f,
            -4 => 0.33f,
            -3 => 0.4f,
            -2 => 0.5f,
            -1 => 0.66f,
            0 => 1.0f,
            1 => 1.5f,
            2 => 2.0f,
            3 => 2.5f,
            4 => 3.0f,
            5 => 3.5f,
            6 => 4.0f,

            _ => 1.0f,
        };
    }


    //----------------------------------- ServerRpc of Actions and movements logic:
    
    private bool IsItThisPlayersTurn()
    {
        if (GameManager.Instance.GetCurrentPlayer() != _ownerUsername)
        {
            Debug.LogError("A client tried to call a Ship action during another players turn");
            return false;
        }
        else
        {
            return true;
        }
    }

    private bool IsSenderIdIsOwnerOfShip(ulong senderId)
    {
        if (_ownerUsername != NetworkManager.Singleton.ConnectedClients[senderId].PlayerObject.GetComponent<Player>().GetUsername())
        {
            Debug.LogError("A client tried to call a Ship action of another players ship");
            return false;
        }
        else
        {
            return true;
        }
    }

    // Initial checks to do before all Ships actions or movements
    private bool PassedInitialChecks(ServerRpcParams serverRpcParams)
    {
        if (!IsItThisPlayersTurn()) return false;


        ulong senderId = serverRpcParams.Receive.SenderClientId;

        if (!IsSenderIdIsOwnerOfShip(senderId)) return false;


        return true;
    }


    [ServerRpc(RequireOwnership = false)]
    public void RefuelToMaxAtPortActionServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (!PassedInitialChecks(serverRpcParams)) return;

        if (_canDoAction.Value == false)
        {
            Debug.LogError("A client tried to call a Ship action on a ship that had its action disabled");
            return;
        }

        if (!Pathfinding.Instance.IsPosOnTopOfAPortOrAdjacent(_currentPosition.Value.GetValues()))
        {
            Debug.LogError("A client tried to call a refuel at port action without being near a port");
            return;
        }

        _currentFuel.Value = _shipSO.maxFuel;

        _canDoAction.Value = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void HealActionServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (!PassedInitialChecks(serverRpcParams)) return;

        if (_canDoAction.Value == false)
        {
            Debug.LogError("A client tried to call a Ship action on a ship that had its action disabled");
            return;
        }

        float healPercentage = 0.1f;
        if (Pathfinding.Instance.IsPosOnTopOfAPortOrAdjacent(_currentPosition.Value.GetValues())) healPercentage = 0.2f; // If is on top of a port then it heals more

        _currentHealth.Value = Mathf.Min(_shipSO.maxHealth, _currentHealth.Value + (int) Mathf.Floor(_shipSO.maxHealth * healPercentage));

        _canDoAction.Value = false;
    }

    // NetworkBehaviourReference is the easy way of referencing a specific NetworkBehaviour gameobject in an Rpc call
    [ServerRpc(RequireOwnership = false)]
    public void ActivateActionServerRpc(int actionIndex, NetworkBehaviourReference[] targetShips, Vector3Int[] positions, Orientation[] orientations, int customParam, ServerRpcParams serverRpcParams = default)
    {
        if (!PassedInitialChecks(serverRpcParams)) return;

        if (_canDoAction.Value == false)
        {
            Debug.LogError("A client tried to call a Ship action on a ship that had its action disabled");
            return;
        }

        if (actionIndex >= _shipSO.actionList.Count)
        {
            Debug.Log(this.name + " is trying to use an action that doesn't exits, the index passed is too high");
            return;
        }

        List<ShipUnit> targets = new List<ShipUnit>();

        foreach (NetworkBehaviourReference shipRef in targetShips)
        {
            if (shipRef.TryGet(out ShipUnit shipUnit))
            {
                targets.Add(shipUnit);
            }
        }

        List<Vector3Int> positionsList = new List<Vector3Int>(positions);

        List<Orientation> orientationsList = new List<Orientation>(orientations);

        bool actionDoneCorrectly = _shipSO.actionList[actionIndex].Activate(this, targets, positionsList, orientationsList, customParam);

        if (actionDoneCorrectly) _canDoAction.Value = false;
    }


    [ServerRpc(RequireOwnership = false)]
    public void MoveServerRpc(Vector3Int destination, ServerRpcParams serverRpcParams = default)
    {
        if (!PassedInitialChecks(serverRpcParams)) return;

        ulong senderId = 0; // TODO make method a ServerRpc

        Node destinationNode = Pathfinding.Instance.AStarSearch(_currentPosition.Value.GetValues(), destination, this);

        if (destinationNode == null)
        {
            Debug.LogError("AStarSearch was called on a bad couple of tiles " + this);
            return;
        }

        //Calculate the lenght of the path
        int pathLenght = 0;
        for (Node step = destinationNode; step.Parent != null; step = step.Parent)
        {
            pathLenght++;
        }

        if (pathLenght > _movementLeft.Value)
        {
            Debug.LogError("A client tried to move a ship too far than the movement it had left");
            return;
        }

        // Here we passed all checks:

        Sequence moveSequence = DOTween.Sequence();

        for (Node step = destinationNode; step.Parent != null; step = step.Parent)
        {
            Vector3 worldPos = _tilemap.GetCellCenterWorld(step.Position);

            moveSequence.Prepend(transform.DOMove(worldPos, 0.5f).SetEase(Ease.Linear));
        }

        // Update this ship position and the _currentPosition.onValueChanged event will trigger both on server and on client
        _movementLeft.Value -= pathLenght;
        _currentPosition.Value = destination;


        // Check if this ship won the game by retrieving the treasure back to the base
        Treasure treasureInstance = Treasure.Instance;

        if (!treasureInstance.IsBeingCarried() && destination == treasureInstance.GetCurGridPosition())
        {
            treasureInstance.SetCarryingShip(this);
        }

        if (treasureInstance.GetCarryingShip() == this)
        {
            treasureInstance.SetCurGridPosition(destination);

            if (destination == NetworkManager.Singleton.ConnectedClients[senderId].PlayerObject.GetComponent<Player>().GetWinningTreasurePosition())
            {
                ShipRetrievedTheTreasureAndWonGame?.Invoke(this);
            }
        }
    }


    //----------------------------------- Server only methods of logic that modifies ship values:

    // Not ServerRpc but only called by the server that enables the ship at the start of the players turn
    public void EnableShip()
    {
        // Extra check just to be sure
        if (!IsServer) return;

        if (_isDestroyed.Value) return;

        _oneTurnTemporaryAttackStage.Value = 0;
        _oneTurnTemporaryDefenseStage.Value = 0;

        _canDoAction.Value = true;
        _movementLeft.Value = _shipSO.speed;

        
    }

    // Not ServerRpc but only called by the server that disables the ship at the end of the players turn
    public void DisableShip()
    {
        // Extra check just to be sure
        if (!IsServer) return;

        _canDoAction.Value = false;
        _movementLeft.Value = 0;
    }


    public void ModifyAttack(int stageModification, bool isOneTurnTemp)
    {
        // Extra check just to be sure
        if (!IsServer) return;

        int finalValue;

        if (!isOneTurnTemp) finalValue = _attackStage.Value + stageModification;
        else finalValue = _oneTurnTemporaryAttackStage.Value + stageModification;

        finalValue = Mathf.Min(MAX_STAGE, finalValue);

        finalValue = Mathf.Max(MIN_STAGE, finalValue);

        if (!isOneTurnTemp) _attackStage.Value = finalValue;
        else _oneTurnTemporaryAttackStage.Value = finalValue;

    }


    public void ModifyDefense(int stageModification, bool isOneTurnTemp)
    {
        // Extra check just to be sure
        if (!IsServer) return;

        int finalValue;

        if (!isOneTurnTemp) finalValue = _defenseStage.Value + stageModification;
        else finalValue = _oneTurnTemporaryDefenseStage.Value + stageModification;

        finalValue = Mathf.Min(MAX_STAGE, finalValue);

        finalValue = Mathf.Max(MIN_STAGE, finalValue);

        if (!isOneTurnTemp) _defenseStage.Value = finalValue;
        else _oneTurnTemporaryDefenseStage.Value = finalValue;
    }


    public void TakeHit(ShipUnit attackingShip, int power)
    {
        // Extra check just to be sure
        if (!IsServer) return;

        float divisorValue = 2.0f;
        int critChance = 24;

        float damage = (power * (attackingShip.GetAttack / GetDefense) / divisorValue) * Random.Range(0.9f, 1.0f);

        float critMultiplier = Random.Range(0, critChance) == 0 ? 1.5f : 1.0f;

        damage *= critMultiplier;

        int roundedDamage = (int)Mathf.Floor(damage);


        if (_currentHealth.Value < roundedDamage)
        {
            _currentHealth.Value = 0;

            //TODO add death animation 

            _isDestroyed.Value = true;  // This will trigger the event of _isDestroyed.onValueChanged -> HideShip on server and clients

            ShipIsDestroyed?.Invoke(this);  // This event will trigger only on server

            Debug.Log(this.name + " is destroyed");
        }
        else
        {
            _currentHealth.Value -= roundedDamage;
        }
    }

    public void RemoveFuel(int amount)
    {
        // Extra check just to be sure
        if (!IsServer) return;

        if (_currentFuel.Value < amount)
        {
            Debug.LogError("Tried to remove too much fuel from ship " + this.name);
            return;
        }

        _currentFuel.Value -= amount;
    }

    public void AddFuel(int amount)
    {
        // Extra check just to be sure
        if (!IsServer) return;

        if (_currentFuel.Value + amount > _shipSO.maxFuel)
        {
            Debug.LogError("Tried to add too much fuel to ship " + this.name);
            return;
        }

        _currentFuel.Value += amount;
    }

    public void SetDestroyed()
    {
        // Extra check just to be sure
        if (!IsServer) return;

        _isDestroyed.Value = true;
    }


    //----------------------------------- Getters:


    public string GetOwnerUsername()
    {
        return _ownerUsername;
    }

    public int GetCurrentHealth()
    {
        return _currentHealth.Value;
    }

    public int GetCurrentFuel()
    {
        return _currentFuel.Value;
    }

    public int GetAttackStage()
    {
        return _attackStage.Value;
    }

    public int GetDefenseStage()
    {
        return _defenseStage.Value;
    }

    public int GetOneTurnTemporaryAttackStage()
    {
        return _oneTurnTemporaryAttackStage.Value;
    }

    public int GetOneTurnTemporaryDefenseStage()
    {
        return _oneTurnTemporaryDefenseStage.Value;
    }

    public bool CanDoAction()
    {
        return _canDoAction.Value;
    }

    public int GetMovementLeft()
    {
        return _movementLeft.Value;
    }

    public Vector3Int GetCurrentPosition()
    {
        return _currentPosition.Value.GetValues();
    }

    public bool IsDestroyed()
    {
        return _isDestroyed.Value;
    }

    public int GetMaxHealth()
    {
        return _shipSO.maxHealth;
    }

    public int GetMaxFuel()
    {
        return _shipSO.maxFuel;
    }

    public int GetBaseAttack()
    {
        return _shipSO.attack;
    }

    public int GetBaseDefense()
    {
        return _shipSO.defense;
    }

    public List<Action> GetActions()
    {
        return _shipSO.actionList;
    }

    public bool IsFlagship()
    {
        return _shipSO.isFlagship;
    }


    public bool IsMyShip()
    {
        if (IsServer) return false; // This is a local client check method only

        return _ownerUsername == NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>().GetUsername();
    }
}



public struct Vector3IntSerializable : INetworkSerializable
{
    private int x;
    private int y;
    private int z;

    public Vector3IntSerializable(Vector3Int values)
    {
        this.x = values.x;
        this.y = values.y;
        this.z = values.z;
    }

    public static implicit operator Vector3IntSerializable(Vector3Int newValue)
    {
        return new Vector3IntSerializable(newValue);
    }


    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref x);
        serializer.SerializeValue(ref y);
        serializer.SerializeValue(ref z);

    }



    public Vector3Int GetValues()
    {
        return new Vector3Int(x, y, z);
    }
}