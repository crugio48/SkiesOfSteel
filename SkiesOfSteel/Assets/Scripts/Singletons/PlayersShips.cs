
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayersShips : Singleton<PlayersShips>
{
    private Dictionary<FixedString32Bytes, List<ShipUnit>> shipsOfPlayer;


    public void Start()
    {
        shipsOfPlayer = new Dictionary<FixedString32Bytes, List<ShipUnit>>();
    }


    public void SetShip(FixedString32Bytes username, ShipUnit ship)
    {
        if (!shipsOfPlayer.ContainsKey(username))
        {
            shipsOfPlayer.Add(username, new List<ShipUnit>());
        }
        List<ShipUnit> currList = shipsOfPlayer[username];

        currList.Append(ship);

        shipsOfPlayer[username] = currList;

    }


    public List<ShipUnit> GetShips(FixedString32Bytes username)
    {
        return shipsOfPlayer[username];
    }
}
