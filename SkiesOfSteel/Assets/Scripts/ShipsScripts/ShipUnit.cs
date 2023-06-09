using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ShipUnit : MonoBehaviour
{
    [SerializeField]
    private ShipScriptableObject shipScriptableValues;

    private int currentHealth;
    private int currentFuel;
    private int attackStage;
    private int defenseStage;

    private SpriteRenderer spriteRenderer;

    private Vector3Int currentPosition;

    private Pathfinding pathfinding;
    private Tilemap tilemap;

    //TODO add hold item parameter

    public bool CanDoAction { get; set; }

    public int MovementLeft { get; set; }

    public float GetAttack
    {
        get
        {
            return Mathf.Floor(shipScriptableValues.attack * GetMultiplier(attackStage));
        }
    }

    public float GetDefense
    {
        get
        {
            return Mathf.Floor(shipScriptableValues.defense * GetMultiplier(defenseStage));
        }
    }

    private void Awake()
    {
        pathfinding = FindObjectOfType<Pathfinding>();
        tilemap = FindObjectOfType<Tilemap>();
    }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = shipScriptableValues.sprite;

        currentHealth = shipScriptableValues.maxHealth;
        currentFuel = shipScriptableValues.maxFuel;
        attackStage = 0;
        defenseStage = 0;

        CanDoAction = false;
        MovementLeft = 0;
    }


    public void EnableShip()
    {
        CanDoAction = true;
        MovementLeft = shipScriptableValues.speed;
    }

    public void ModifyAttack(int stageModification)
    {
        attackStage += stageModification;

        if (attackStage > 2)
            attackStage = 2;

        if (attackStage < -2)
            attackStage = -2;
    }


    public void ModifyDefense(int stageModification)
    {
        defenseStage += stageModification;

        if (defenseStage > 2)
            defenseStage = 2;

        if (defenseStage < -2)
            defenseStage = -2;
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


        if (currentHealth < roundedDamage)
        {
            //TODO add death animation and logic
            Debug.Log(this + " is destroyed");
        }
        else
        {
            currentHealth -= roundedDamage;
        }
    }


    public void RefuelToMaxAtPortAction()
    {
        currentFuel = shipScriptableValues.maxFuel;
    }


    public void HealAtPortAction()
    {
        float healPercentage = 0.2f;

        currentHealth += (int) Mathf.Floor(shipScriptableValues.maxHealth * healPercentage);

        if (currentHealth > shipScriptableValues.maxHealth)
        {
            currentHealth = shipScriptableValues.maxHealth;
        }

    }


    public int GetCurrentFuel()
    {
        return currentFuel;
    }

    public void RemoveFuel(int amount)
    {
        currentFuel -= amount;
        if (currentFuel < 0)
        {
            currentFuel += amount;
            Debug.LogError("Tried to remove too much fuel from ship " + this.name);
        }
    }

    public void AddFuel(int amount)
    {
        currentFuel += amount;
        if (currentFuel > shipScriptableValues.maxFuel)
        {
            currentFuel -= amount;
            Debug.LogError("Tried to add too much fuel to ship " + this.name);
        }
    }


    public List<Action> GetActions()
    {
        return shipScriptableValues.actions;
    }


    public void SetInitialPosition(Vector3Int pos)
    {
        currentPosition = pos;
        transform.position = tilemap.GetCellCenterWorld(currentPosition);
    }

    public void Move(Vector3Int destination)
    {
        Node destinationNode = pathfinding.AStarSearch(currentPosition, destination);

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
            Vector3 worldPos = tilemap.GetCellCenterWorld(step.Position);

            moveSequence.Prepend(transform.DOMove(worldPos, 0.5f).SetEase(Ease.Linear));
            
        }


        // Update this ship position for the data structures
        ShipsPositions.Instance.Move(this, currentPosition, destination);
        currentPosition = destination;

    }

}
