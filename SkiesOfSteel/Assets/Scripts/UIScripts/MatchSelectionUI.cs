using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchSelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject matchSelectionUI;
    [SerializeField] private GameObject serverClientSelectionUI;

    [SerializeField] private TextMeshProUGUI errorTextField;
    [SerializeField] private TMP_InputField inputIP;

    private string _matchSelectedSceneName;


    public void PlayButtonClickedRefresh()
    {
        matchSelectionUI.SetActive(true);
        serverClientSelectionUI.SetActive(false);
    }


    public void MatchTypeSelected(string sceneName)
    {
        _matchSelectedSceneName = sceneName;

        errorTextField.text = "";

        matchSelectionUI.SetActive(false);
        serverClientSelectionUI.SetActive(true);
    }


    private bool CheckAndSetIpAndPort()
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
                return false;
            }
        }


        // TODO get inputfield to change the ip address to connect to
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
            ip,  // The IP address is a string
            port // The port number is an unsigned short
        );

        return true;
    }


    public void ServerSelected()
    {
        if (CheckAndSetIpAndPort() == false) return;

        NetworkManager.Singleton.StartServer();


        NetworkManager.Singleton.SceneManager.LoadScene(_matchSelectedSceneName, LoadSceneMode.Single);
    }


    public void ClientSelected()
    {
        if (CheckAndSetIpAndPort() == false) return;

        NetworkManager.Singleton.OnClientDisconnectCallback += ConnectionFailed;

        bool outcome = NetworkManager.Singleton.StartClient();  // If connection is successful then the scene will be changed automatically on the client

        if (outcome == false)
        {
            errorTextField.text = "No server found at that IP address!";

            NetworkManager.Singleton.OnClientDisconnectCallback -= ConnectionFailed;
        }
    }


    private void ConnectionFailed(ulong param)
    {
        errorTextField.text = "Sorry but server is already full!";

        NetworkManager.Singleton.OnClientDisconnectCallback -= ConnectionFailed;
    }
}
