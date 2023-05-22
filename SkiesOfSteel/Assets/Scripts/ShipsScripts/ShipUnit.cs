using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipUnit : MonoBehaviour
{
    [SerializeField]
    private ShipScriptableObject shipScriptableValues;

    private int currentHealth;
    private int currentFuel;

    private SpriteRenderer spriteRenderer;

    public bool CanDoAction { get; set; }

    public bool CanMove { get; set; }


    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = shipScriptableValues.sprite;

        currentHealth = shipScriptableValues.maxHealth;
        currentFuel = shipScriptableValues.maxFuel;

        CanDoAction = false;
        CanMove = false;
    }


    public void EnableShip()
    {
        CanDoAction = true;
        CanMove = true;
    }



}
