using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/StartingShipsPositionsData")]
public class StartingPositionsSO : ScriptableObject
{

    public List<Vector3Int> flagshipsPositions;
    public List<Vector3Int> attackShipsPositions;
    public List<Vector3Int> fastShipsPositions;
    public List<Vector3Int> cargoShipsPositions;

}
