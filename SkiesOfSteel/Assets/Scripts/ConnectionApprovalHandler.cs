
using UnityEngine;
using Unity.Netcode;

public class ConnectionApprovalHandler : MonoBehaviour
{
    private int _maxPlayers = 4;


    private void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
    }


    public void SetMaxPlayers(int maxPlayers)
    {
        _maxPlayers = maxPlayers;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        Debug.Log("Connect Approval");

        response.Approved = true;
        response.CreatePlayerObject = true;
        response.PlayerPrefabHash = null;

        if (NetworkManager.Singleton.ConnectedClients.Count >= _maxPlayers)
        {
            response.Approved = false;
            response.CreatePlayerObject = false;
            response.Reason = "Server is full";
        }

        if (GameManager.Instance.HasBattleStarted())
        {
            response.Approved = false;
            response.CreatePlayerObject = false;
            response.Reason = "Battle has already started";
        }

        response.Pending = false;
    }
}