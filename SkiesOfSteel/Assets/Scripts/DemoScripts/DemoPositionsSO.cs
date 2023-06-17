using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/DemoPositionsData")]
public class DemoPositionsSO : ScriptableObject
{

    public List<Vector3Int> flagshipsPositions;
    public List<Vector3Int> attackShipsPositions;
    public List<Vector3Int> fastShipsPositions;
    public List<Vector3Int> cargoShipsPositions;

    public Vector3Int treasureStartingGridPosition;

    public List<Vector3Int> playersWinningTreasurePositions;

    public List<Color> playersColors;
}
