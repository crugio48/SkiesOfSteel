using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerCanvasUI : MonoBehaviour
{
    [SerializeField] private GameObject serverBackToMainMenuButton;

    [SerializeField] private Canvas thisCanvas;


    public void EnableCanvas()
    {
        thisCanvas.enabled = true;
    }


    public void EnableBackToMainMenuButton()
    {
        serverBackToMainMenuButton.SetActive(true);
    }


    public void BackToMainMenuServer()
    {
        NetworkManager.Singleton.Shutdown();

        SceneManager.LoadScene(0);
    }

}
