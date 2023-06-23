using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DemoBattleSpawner : NetworkBehaviour
{
    [SerializeField] private string demoPositionsSOPath;
    [SerializeField] private GameObject shipUnitPrefab;
    [SerializeField] private Tilemap tilemap;

    public void SpawnDemoShips(List<string> playerUsernames, int numOfPlayers, Dictionary<string, ulong> _usernameToClientIds)
    {
        if (playerUsernames.Count != numOfPlayers)
        {
            Debug.LogError("Wrong setup to game done by GameManager");
            return;
        }

        DemoPositionsSO demoPositions = Resources.Load<DemoPositionsSO>(demoPositionsSOPath);

        List<Color> playersColors = demoPositions.playersColors;

        //Setup ships for demo match
        for (int i = 0; i < numOfPlayers; i++)
        {
            Debug.Log("Spawning ships for player = " + playerUsernames[i]);

            // Spawning Flagship
            SpawnShip(demoPositions.flagshipsPositions[i], "ShipsScriptableObjects/DefenseFlagship", playerUsernames[i], "Flagship", playersColors[i]);

            // Spawning AttackShip
            SpawnShip(demoPositions.attackShipsPositions[i], "ShipsScriptableObjects/AttackShip", playerUsernames[i], "AttackShip", playersColors[i]);

            // Spawning FastShip
            SpawnShip(demoPositions.fastShipsPositions[i], "ShipsScriptableObjects/CargoShip", playerUsernames[i], "CargoShip", playersColors[i]);

            // Spawning CargoShip
            SpawnShip(demoPositions.cargoShipsPositions[i], "ShipsScriptableObjects/FastShip", playerUsernames[i], "FastShip", playersColors[i]);

            ulong clientId = _usernameToClientIds[playerUsernames[i]];
            NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<Player>().SetWinningTreasurePosition(demoPositions.playersWinningTreasurePositions[i]);

        }

        Treasure.Instance.SetInitialPosition(demoPositions.treasureStartingGridPosition);
    }


    private void SpawnShip(Vector3Int gridPosition, string scriptableObjectPath, string playerUsername, string typeOfShip, Color color)
    {
        GameObject newShip = Instantiate(shipUnitPrefab, tilemap.GetCellCenterWorld(gridPosition), Quaternion.identity);
        newShip.name = typeOfShip + " of " + playerUsername; // Server only 
        newShip.GetComponent<NetworkObject>().Spawn();
        ShipUnit shipUnit = newShip.GetComponent<ShipUnit>();
        shipUnit.SetShipScriptableObject(scriptableObjectPath);
        shipUnit.SetInitialGridPosition(gridPosition);
        shipUnit.SetOwnerUsername(playerUsername);
        shipUnit.SetOutlineAndColorClientRpc(color);
    }
}
