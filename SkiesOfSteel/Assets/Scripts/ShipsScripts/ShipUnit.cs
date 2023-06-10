using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;


[RequireComponent(typeof(SpriteRenderer))]
public class ShipUnit : NetworkBehaviour
{
    [SerializeField] private ShipScriptableObject shipSO;

    private NetworkVariable<FixedString32Bytes> _ownerUsername = new NetworkVariable<FixedString32Bytes>();

    private int _currentHealth;
    private int _currentFuel;
    private int _attackStage;
    private int _defenseStage;

    private SpriteRenderer _spriteRenderer;

    private Vector3Int _currentPosition;

    private Pathfinding _pathfinding;
    private Tilemap _tilemap;

    //TODO add hold item parameter


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Run both on server and on clients
        _ownerUsername.OnValueChanged += RegisterShipOfOwner;
    }

    private void RegisterShipOfOwner(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        PlayersShips.Instance.SetShip(newValue, this);
    }

    // Only the server will be running this function
    public void SetShipScriptableObject(string shipSOpath)
    {
        SetInitialValuesFromSO(shipSOpath);

        // Make clients also set the shipSO
        SetShipScriptableObjectClientRpc(shipSOpath);
    }

    [ClientRpc]
    private void SetShipScriptableObjectClientRpc(string shipSOpath)
    {
        SetInitialValuesFromSO(shipSOpath);
    }


    private void SetInitialValuesFromSO(string shipSOpath)
    {
        if (shipSOpath != null)
        {
            shipSO = Resources.Load<ShipScriptableObject>(shipSOpath);
        }

        _spriteRenderer.sprite = shipSO.sprite; // TODO implement logic for more sprites
        _currentHealth = shipSO.maxHealth;
        _currentFuel = shipSO.maxFuel;
    }
    
    // This will only be called on the server gameObject by server gameManager
    public void SetOwnerUsername(FixedString32Bytes username)
    {
        _ownerUsername.Value = username;
    }


    public bool CanDoAction { get; set; }

    public int MovementLeft { get; set; }


    public float GetAttack
    {
        get
        {
            return Mathf.Floor(shipSO.attack * GetMultiplier(_attackStage));
        }
    }

    public float GetDefense
    {
        get
        {
            return Mathf.Floor(shipSO.defense * GetMultiplier(_defenseStage));
        }
    }

    private void Awake()
    {
        _pathfinding = FindObjectOfType<Pathfinding>();
        _tilemap = FindObjectOfType<Tilemap>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        CanDoAction = false;
        MovementLeft = 0;

        _attackStage = 0;
        _defenseStage = 0;


        // If we are testing the game in a custom scene then the shipSO will be set from unity so we enter here
        if (shipSO != null)
        {
            SetInitialValuesFromSO(null);
        }
    }


    public void EnableShip()
    {
        CanDoAction = true;
        MovementLeft = shipSO.speed;
    }

    public void ModifyAttack(int stageModification)
    {
        _attackStage += stageModification;

        if (_attackStage > 2)
            _attackStage = 2;

        if (_attackStage < -2)
            _attackStage = -2;
    }


    public void ModifyDefense(int stageModification)
    {
        _defenseStage += stageModification;

        if (_defenseStage > 2)
            _defenseStage = 2;

        if (_defenseStage < -2)
            _defenseStage = -2;
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


        if (_currentHealth < roundedDamage)
        {
            //TODO add death animation and logic
            Debug.Log(this + " is destroyed");
        }
        else
        {
            _currentHealth -= roundedDamage;
        }
    }


    public void RefuelToMaxAtPortAction()
    {
        _currentFuel = shipSO.maxFuel;
    }


    public void HealAtPortAction()
    {
        float healPercentage = 0.2f;

        _currentHealth += (int) Mathf.Floor(shipSO.maxHealth * healPercentage);

        if (_currentHealth > shipSO.maxHealth)
        {
            _currentHealth = shipSO.maxHealth;
        }

    }


    public int GetCurrentFuel()
    {
        return _currentFuel;
    }

    public void RemoveFuel(int amount)
    {
        _currentFuel -= amount;
        if (_currentFuel < 0)
        {
            _currentFuel += amount;
            Debug.LogError("Tried to remove too much fuel from ship " + this.name);
        }
    }

    public void AddFuel(int amount)
    {
        _currentFuel += amount;
        if (_currentFuel > shipSO.maxFuel)
        {
            _currentFuel -= amount;
            Debug.LogError("Tried to add too much fuel to ship " + this.name);
        }
    }


    public List<Action> GetActions()
    {
        return shipSO.actionList;
    }


    public void SetInitialPosition(Vector3Int pos)
    {
        _currentPosition = pos;
        transform.position = _tilemap.GetCellCenterWorld(_currentPosition);
    }

    public void Move(Vector3Int destination)
    {
        Node destinationNode = _pathfinding.AStarSearch(_currentPosition, destination);

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

        if (pathLenght > MovementLeft)
        {
            Debug.Log("pathLenght = " + pathLenght);
            Debug.Log("MovementLeft = " + MovementLeft);
            Debug.LogError("Trying to move too far for how much movement this ship has left " + this);
            return;
        }
        
        // Here we passed all checks:

        Sequence moveSequence = DOTween.Sequence();

        for (Node step = destinationNode; step.Parent != null; step = step.Parent)
        {
            Vector3 worldPos = _tilemap.GetCellCenterWorld(step.Position);

            moveSequence.Prepend(transform.DOMove(worldPos, 0.5f).SetEase(Ease.Linear));
            
        }


        // Update this ship position for the data structures
        ShipsPositions.Instance.Move(this, _currentPosition, destination);
        _currentPosition = destination;

    }

}
