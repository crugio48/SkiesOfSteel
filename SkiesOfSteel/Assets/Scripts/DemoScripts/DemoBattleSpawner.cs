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

    public void SpawnDemoShips(List<string> playerUsernames, int numOfPlayers, Dictionary<string, ulong> _usernameToClientIds)
    {
        if (playerUsernames.Count != numOfPlayers)
        {
            Debug.LogError("Wrong setup to game done by GameManager");
            return;
        }


        DemoPositionsSO demoPositions = Resources.Load<DemoPositionsSO>("DemoPositions");

        //Setup ships for demo match
        for (int i = 0; i < numOfPlayers; i++)
        {
            Debug.Log("Spawning ships for player = " + playerUsernames[i]);

            // Spawning Flagship
            SpawnShip(demoPositions.flagshipsPositions[i], "ShipsScriptableObjects/DefenseFlagship", playerUsernames[i], "Flagship");

            // Spawning AttackShip
            SpawnShip(demoPositions.attackShipsPositions[i], "ShipsScriptableObjects/AttackShip", playerUsernames[i], "AttackShip");

            // Spawning FastShip
            SpawnShip(demoPositions.fastShipsPositions[i], "ShipsScriptableObjects/CargoShip", playerUsernames[i], "CargoShip");

            // Spawning CargoShip
            SpawnShip(demoPositions.cargoShipsPositions[i], "ShipsScriptableObjects/FastShip", playerUsernames[i], "FastShip");

            ulong clientId = _usernameToClientIds[playerUsernames[i]];
            NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<Player>().SetWinningTreasurePosition(demoPositions.playersWinningTreasurePositions[i]);

        }
    }


    private void SpawnShip(Vector3Int gridPosition, string scriptableObjectPath, string playerUsername, string typeOfShip)
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
