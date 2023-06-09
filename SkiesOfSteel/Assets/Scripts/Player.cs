
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private List<ShipUnit> _myShips;



    public void SetShips(List<ShipUnit> ships)
    {
        _myShips = ships;
    }
}
