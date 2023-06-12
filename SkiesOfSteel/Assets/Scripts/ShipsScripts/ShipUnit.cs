using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;


[RequireComponent(typeof(SpriteRenderer))]
public class ShipUnit : NetworkBehaviour
{
    [SerializeField] private ShipScriptableObject shipSO;

    private NetworkVariable<FixedString32Bytes> _ownerUsername = new NetworkVariable<FixedString32Bytes>();

    private NetworkVariable<int> _currentHealth = new NetworkVariable<int>();
    private NetworkVariable<int> _currentFuel = new NetworkVariable<int>();
    private NetworkVariable<int> _attackStage = new NetworkVariable<int>(0);
    private NetworkVariable<int> _defenseStage = new NetworkVariable<int>(0);

    private NetworkVariable<bool> _canDoAction = new NetworkVariable<bool>(false);
    private NetworkVariable<int> _movementLeft = new NetworkVariable<int>(0);

    private static Vector3Int NOT_SET_POSITION = new Vector3Int(-1000, -1000, -1000);
    private NetworkVariable<Vector3IntSerializable> _currentPosition = new NetworkVariable<Vector3IntSerializable>(NOT_SET_POSITION);

    private NetworkVariable<bool> _isDestroyed = new NetworkVariable<bool>(false);

    private SpriteRenderer _spriteRenderer;
    private Pathfinding _pathfinding;
    private Tilemap _tilemap;

    [CanBeNull] public static event System.Action<ShipUnit> ShipIsDestroyed;
    [CanBeNull] public static event System.Action<ShipUnit> ShipRetrievedTheTreasureAndWonGame;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _pathfinding = FindObjectOfType<Pathfinding>();
        _tilemap = FindObjectOfType<Tilemap>();
        _spriteRenderer = GetComponent<SpriteRenderer>();


        // Run both on server and on clients the callbacks that update the info databases classes
        _ownerUsername.OnValueChanged += RegisterShipOfOwner;
        _currentPosition.OnValueChanged += PositionChangedCallback;
        _isDestroyed.OnValueChanged += HideShip;
    }


    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        _ownerUsername.OnValueChanged -= RegisterShipOfOwner;
        _currentPosition.OnValueChanged -= PositionChangedCallback;
        _isDestroyed.OnValueChanged -= HideShip;
    }



    private void RegisterShipOfOwner(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        PlayersShips.Instance.SetShip(newValue, this);
    }

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


    private void HideShip(bool previousValue, bool newValue)
    {
        // If ship got destroyed then hide it from the game visually and from the position dictionary
        if (previousValue == false && newValue == true)
        {
            _spriteRenderer.enabled = false;

            ShipsPositions.Instance.RemoveShip(_currentPosition.Value.GetValues());
        }
    }


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
        if (shipSOpath != null)
        {
            shipSO = Resources.Load<ShipScriptableObject>(shipSOpath);
        }

        _spriteRenderer.sprite = shipSO.sprite; // TODO implement logic for more sprites
        _currentHealth.Value = shipSO.maxHealth;
        _currentFuel.Value = shipSO.maxFuel;
    }


    // Only the server will be running this function once
    public void SetOwnerUsername(FixedString32Bytes username)
    {
        _ownerUsername.Value = username;
    }

    // Only the server will be running this function once
    public void SetInitialGridPosition(Vector3Int gridPos)
    {
        _currentPosition.Value = gridPos;
    }


    public float GetAttack
    {
        get
        {
            return Mathf.Floor(shipSO.attack * GetMultiplier(_attackStage.Value));
        }
    }

    public float GetDefense
    {
        get
        {
            return Mathf.Floor(shipSO.defense * GetMultiplier(_defenseStage.Value));
        }
    }

    public void EnableShip()
    {
        _canDoAction.Value = true;
        _movementLeft.Value = shipSO.speed;
    }

    public void ModifyAttack(int stageModification)
    {
        _attackStage.Value += stageModification;

        if (_attackStage.Value > 2)
            _attackStage.Value = 2;

        if (_attackStage.Value < -2)
            _attackStage.Value = -2;
    }


    public void ModifyDefense(int stageModification)
    {
        _defenseStage.Value += stageModification;

        if (_defenseStage.Value > 2)
            _defenseStage.Value = 2;

        if (_defenseStage.Value < -2)
            _defenseStage.Value = -2;
    }

    private float GetMultiplier(int stage)
    {
        //taking example from: https://www.dragonflycave.com/mechanics/stat-stages

        return stage switch
        {
            -2 => 0.5f,
            -1 => 0.66f,
            0 => 1.0f,
            1 => 1.5f,
            2 => 2.0f,

            _ => 1.0f,
        };
    }


    public void TakeHit(ShipUnit attackingShip, int power)
    {
        float divisorValue = 2.0f;
        int critChance = 24;

        float damage = (power * (attackingShip.GetAttack / GetDefense) / divisorValue) * Random.Range(0.9f, 1.0f);

        float critMultiplier = Random.Range(0, critChance) == 0 ? 1.5f : 1.0f;

        damage *= critMultiplier;
        
        int roundedDamage = (int) Mathf.Floor(damage);


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


    public void RefuelToMaxAtPortAction()
    {
        _currentFuel.Value = shipSO.maxFuel;
    }


    public void HealAtPortAction()
    {
        float healPercentage = 0.2f;

        _currentHealth.Value += (int) Mathf.Floor(shipSO.maxHealth * healPercentage);

        if (_currentHealth.Value > shipSO.maxHealth)
        {
            _currentHealth.Value = shipSO.maxHealth;
        }

    }


    public int GetCurrentFuel()
    {
        return _currentFuel.Value;
    }

    public void RemoveFuel(int amount)
    {
        if (_currentFuel.Value < amount)
        {
            Debug.LogError("Tried to remove too much fuel from ship " + this.name);
            return;
        }

        _currentFuel.Value -= amount;
    }

    public void AddFuel(int amount)
    {
        if (_currentFuel.Value + amount > shipSO.maxFuel)
        {
            Debug.LogError("Tried to add too much fuel to ship " + this.name);
            return;
        }

        _currentFuel.Value += amount;
    }

    public void Move(Vector3Int destination)
    {
        ulong senderId = 0; // TODO make method a ServerRpc

        Node destinationNode = _pathfinding.AStarSearch(_currentPosition.Value.GetValues(), destination);

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
            Debug.Log("pathLenght = " + pathLenght);
            Debug.Log("MovementLeft = " + _movementLeft.Value);
            Debug.LogError("Trying to move too far for how much movement this ship has left " + this.name);
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
        _currentPosition.Value = destination;

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


    // NetworkBehaviourReference is the easy way of referencing a specific NetworkBehaviour gameobject in an Rpc call
    [ServerRpc]
    public void ActivateActionServerRpc(int actionIndex, NetworkBehaviourReference[] targetShips, int customParam, ServerRpcParams serverRpcParams = default)
    {
        ulong senderId = serverRpcParams.Receive.SenderClientId;

        if (!IsSenderIdIsOwnerOfShip(senderId))
        {
            FixedString32Bytes cheaterUsername = NetworkManager.Singleton.ConnectedClients[senderId].PlayerObject.GetComponent<Player>().GetUsername();
            Debug.Log("Player " + cheaterUsername + " that has id of " + senderId + " tryed to cheat!!!!!!");
            return;
        }

        if (actionIndex >= shipSO.actionList.Count)
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

        shipSO.actionList[actionIndex].Activate(this, targets, customParam);
    }


    private bool IsSenderIdIsOwnerOfShip(ulong senderId)
    {
        return _ownerUsername.Value == NetworkManager.Singleton.ConnectedClients[senderId].PlayerObject.GetComponent<Player>().GetUsername();
    }

    public void SetDestroyed()
    {
        _isDestroyed.Value = true;
    }


    public List<Action> GetActions()
    {
        return shipSO.actionList;
    }

    public bool IsFlagship()
    {
        return shipSO.isFlagship;
    }

    public FixedString32Bytes GetOwnerUsername()
    {
        return _ownerUsername.Value;
    }

    public List<ShipUnit> GetShipsOfThisOwner()
    {
        return PlayersShips.Instance.GetShips(_ownerUsername.Value);
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