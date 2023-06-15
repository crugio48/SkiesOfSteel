
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private string _username;

    private Vector3Int _winningTreasurePosition; // This only needs to be checked on server scripts

    // Called on server
    public void SetUsername(string username)
    {
        _username = username;
    }

    [ClientRpc]
    public void SetUsernameClientRpc(string username, ClientRpcParams clientRpcParams = default)
    {
        _username = username;
    }

    public string GetUsername()
    {
        return _username;
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
