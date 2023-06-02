using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipUnit : MonoBehaviour
{
    [SerializeField]
    private ShipScriptableObject shipScriptableValues;

    private int currentHealth;
    private int currentFuel;
    private int attackStage;
    private int defenseStage;

    private SpriteRenderer spriteRenderer;

    public bool CanDoAction { get; set; }

    public bool CanMove { get; set; }

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

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = shipScriptableValues.sprite;

        currentHealth = shipScriptableValues.maxHealth;
        currentFuel = shipScriptableValues.maxFuel;
        attackStage = 0;
        defenseStage = 0;

        CanDoAction = false;
        CanMove = false;
    }


    public void EnableShip()
    {
        CanDoAction = true;
        CanMove = true;
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
}
