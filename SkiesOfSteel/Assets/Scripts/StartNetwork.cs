
using Unity.Netcode;
using UnityEngine;

public class StartNetwork : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    public void StartServer(int numOfPlayers)
    {
        NetworkManager.Singleton.StartServer();

        gameManager.SetNumOfPlayers((ushort) numOfPlayers);
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
}