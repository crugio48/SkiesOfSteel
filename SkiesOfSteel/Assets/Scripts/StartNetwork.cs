
using Unity.Netcode;
using UnityEngine;

public class StartNetwork : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    public void StartServer(ushort numOfPlayers)
    {
        NetworkManager.Singleton.StartServer();

        gameManager.SetNumOfPlayers(numOfPlayers);
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();

        gameManager.NewClientConnectedServerRpc();
    }
}