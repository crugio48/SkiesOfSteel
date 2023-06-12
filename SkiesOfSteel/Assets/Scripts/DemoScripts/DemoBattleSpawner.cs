using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DemoBattleSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject shipUnitPrefab;
    [SerializeField] private Tilemap tilemap;

    public void SpawnDemoShips(NetworkList<FixedString32Bytes> playerUsernames, int numOfPlayers)
    {
        if (playerUsernames.Count != numOfPlayers)
        {
            Debug.LogError("Wrong setup to game done by GameManager");
            return;
        }


        StartingPositionsSO startingPositionsForDemo = Resources.Load<StartingPositionsSO>("DemoStartingPositions");

        //Setup ships for demo match
        for (int i = 0; i < numOfPlayers; i++)
        {
            Debug.Log("Spawning ships for player = " + playerUsernames[i]);

            // Spawning Flagship
            SpawnShip(startingPositionsForDemo.flagshipsPositions[i], "ShipsScriptableObjects/DefenseFlagship", playerUsernames[i], "Flagship");

            // Spawning AttackShip
            SpawnShip(startingPositionsForDemo.attackShipsPositions[i], "ShipsScriptableObjects/AttackShip", playerUsernames[i], "AttackShip");

            // Spawning FastShip
            SpawnShip(startingPositionsForDemo.fastShipsPositions[i], "ShipsScriptableObjects/FastShip", playerUsernames[i], "FastShip");

            // Spawning CargoShip
            SpawnShip(startingPositionsForDemo.cargoShipsPositions[i], "ShipsScriptableObjects/CargoShip", playerUsernames[i], "CargoShip");

        }
    }


    private void SpawnShip(Vector3Int gridPosition, string scriptableObjectPath, FixedString32Bytes playerUsername, string typeOfShip)
    {
        GameObject newShip = Instantiate(shipUnitPrefab, tilemap.GetCellCenterWorld(gridPosition), Quaternion.identity);
        newShip.name = typeOfShip + " of " + playerUsername;
        newShip.GetComponent<NetworkObject>().Spawn();
        ShipUnit shipUnit = newShip.GetComponent<ShipUnit>();
        shipUnit.SetShipScriptableObject(scriptableObjectPath);
        shipUnit.SetInitialGridPosition(gridPosition);
        shipUnit.SetOwnerUsername(playerUsername);
    }
}
