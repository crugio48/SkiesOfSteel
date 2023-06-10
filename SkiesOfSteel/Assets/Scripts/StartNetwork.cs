
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class StartNetwork : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIStartGame uiStartGame;

    private Canvas _startNetworkCanvas;

    private void Start()
    {
        _startNetworkCanvas = GetComponent<Canvas>();
        _startNetworkCanvas.enabled = true;
    }

    public void StartServer(int numOfPlayers)
    {
        NetworkManager.Singleton.StartServer();

        gameManager.SetNumOfPlayers((ushort) numOfPlayers);
    }

    public void StartClient()
    {
        bool outcome = NetworkManager.Singleton.StartClient();

        if (outcome == false)
        {
            uiStartGame.ServerIsFull();
        }
    }
}