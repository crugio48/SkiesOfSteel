
using System.Collections.Generic;

public class PlayersShips : Singleton<PlayersShips>
{
    private Dictionary<string, List<ShipUnit>> shipsOfPlayer;


    public void Start()
    {
        shipsOfPlayer = new Dictionary<string, List<ShipUnit>>();
    }


    public void SetShip(string username, ShipUnit ship)
    {
        if (!shipsOfPlayer.ContainsKey(username))
        {
            shipsOfPlayer.Add(username, new List<ShipUnit>());
        }

        List<ShipUnit> currList = shipsOfPlayer[username];

        currList.Add(ship);

        shipsOfPlayer[username] = currList;

    }


    public List<ShipUnit> GetShips(string username)
    {
        return new List<ShipUnit>(shipsOfPlayer[username]);
    }
}
