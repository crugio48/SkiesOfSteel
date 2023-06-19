
using JetBrains.Annotations;
using System;
using System.Net;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class StartNetwork : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI errorTextField;
    [SerializeField] private TMP_InputField inputIP;

    private Canvas _startNetworkCanvas;

    [CanBeNull] public static event System.Action ClientConnectedCorrectly;

    private void Start()
    {
        _startNetworkCanvas = GetComponent<Canvas>();
        _startNetworkCanvas.enabled = true;
    }

    public void StartServer(int numOfPlayers)
    {
        string ip = "127.0.0.1";
        ushort port = 7777;

        if (!string.IsNullOrEmpty(inputIP.text))
        {
            if (IPAddress.TryParse(inputIP.text, out IPAddress address))
            {
                ip = address.ToString();
            }
            else
            {
                errorTextField.text = "Not a real IP address!";
                return;
            }
        }


        // TODO get inputfield to change the ip address to connect to
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
            ip,  // The IP address is a string
            port // The port number is an unsigned short
        );


        NetworkManager.Singleton.StartServer();

        GameManager.Instance.SetNumOfPlayers(numOfPlayers);
    }

    public void StartClient()
    {
        string ip = "127.0.0.1";
        ushort port = 7777;

        if (!string.IsNullOrEmpty(inputIP.text))
        {
            if (IPAddress.TryParse(inputIP.text, out IPAddress address))
            {
                ip = address.ToString();
            }
            else
            {
                errorTextField.text = "Not a real IP address!";
                return;
            }
        }


        // TODO get inputfield to change the ip address to connect to
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
            ip,  // The IP address is a string
            port // The port number is an unsigned short
        );

        NetworkManager.Singleton.OnClientConnectedCallback += ConnectionSucceded;
        NetworkManager.Singleton.OnClientDisconnectCallback += ConnectionFailed;

        
        bool outcome = NetworkManager.Singleton.StartClient();
        
        if (outcome == false)
        {
            errorTextField.text = "No server found at that IP address!";

            NetworkManager.Singleton.OnClientConnectedCallback -= ConnectionSucceded;
            NetworkManager.Singleton.OnClientDisconnectCallback -= ConnectionFailed;
        }

    }


    private void ConnectionSucceded(ulong param)
    {
        _startNetworkCanvas.enabled = false;
        ClientConnectedCorrectly?.Invoke();

        NetworkManager.Singleton.OnClientConnectedCallback -= ConnectionSucceded;
        NetworkManager.Singleton.OnClientDisconnectCallback -= ConnectionFailed;
    }


    private void ConnectionFailed(ulong param)
    {
        errorTextField.text = "Sorry but server is already full!";

        NetworkManager.Singleton.OnClientConnectedCallback -= ConnectionSucceded;
        NetworkManager.Singleton.OnClientDisconnectCallback -= ConnectionFailed;
    }
}