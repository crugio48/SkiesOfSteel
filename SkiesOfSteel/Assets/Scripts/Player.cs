
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private NetworkVariable<FixedString32Bytes> _username = new NetworkVariable<FixedString32Bytes>();

    private Vector3Int _winningTreasurePosition; // This only needs to be checked on server scripts

    public void SetUsername(FixedString32Bytes username)
    {
        _username.Value = username;
    }

    public FixedString32Bytes GetUsername()
    {
        return _username.Value;
    }


    public void SetWinningTreasurePosition(Vector3Int winningTreasurePosition)
    {
        _winningTreasurePosition = winningTreasurePosition;
    }

    public Vector3Int GetWinningTreasurePosition()
    {
        return _winningTreasurePosition;
    }
}
