
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayersShips : Singleton<PlayersShips>
{
    private Dictionary<FixedString32Bytes, List<ShipUnit>> shipsOfPlayer;


    public override void Awake()
    {
        base.Awake();

        shipsOfPlayer = new Dictionary<FixedString32Bytes, List<ShipUnit>>();

    }

    public void SetShip(FixedString32Bytes username, ShipUnit ship)
    {
        if (!shipsOfPlayer.ContainsKey(username))
        {
            shipsOfPlayer.Add(username, new List<ShipUnit>());
        }
        List<ShipUnit> currList = shipsOfPlayer[username];

        currList.Add(ship);

        shipsOfPlayer[username] = currList;
    }


    public List<ShipUnit> GetShips(FixedString32Bytes username)
    {
        return shipsOfPlayer[username];
    }
}
