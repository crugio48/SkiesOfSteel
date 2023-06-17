
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class TurnCanvas : MonoBehaviour
{
    [SerializeField] private List<Image> playersLogos;

    [SerializeField] private Button passTurnButton;

    private Canvas _canvas;

    List<string> _playersNames;


    private void Start()
    {
        _canvas = GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        GameManager.Instance.StartGameEvent += EnableCanvas;
    }

    private void OnDisable()
    {
        GameManager.Instance.StartGameEvent -= EnableCanvas;
    }
    
    // This will run only on clients
    private void EnableCanvas()
    {
        _canvas.enabled = true;
    }

    public void CurrentPlayerChanged(int newCurrentPlayer)
    {
        string myUsername = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>().GetUsername();

        if (myUsername == _playersNames[newCurrentPlayer])
        {
            passTurnButton.interactable = true;
        }
        else
        {
            passTurnButton.interactable = false;
        }

        HighlightCurrentPlayer(newCurrentPlayer);
    }



    public void SetPlayerNames(List<string> playersNames)
    {
        _playersNames =  new List<string>(playersNames);

        SetCanvas();
    }


    private void SetCanvas()
    {
        for (int i = 0; i < _playersNames.Count; i++)
        {
            playersLogos[i].enabled = true;
            playersLogos[i].color = Color.gray;
        }

        // Highlight player aka first player
        playersLogos[0].color = Color.white;
    }


    public void RemovePlayerAndUpdateCanvas(string playerName, int newCurrentPlayer)
    {
        int index = _playersNames.IndexOf(playerName);

        _playersNames.Remove(playerName);

        playersLogos[index].enabled = false;

        playersLogos.RemoveAt(index);

        CurrentPlayerChanged(newCurrentPlayer);

    }
    

    private void HighlightCurrentPlayer(int newCurrentPlayer)
    {
        for (int i = 0; i < _playersNames.Count; i++)
        { 
            if (i == newCurrentPlayer)
            {
                playersLogos[i].color = Color.white;
            }
            else
            {
                playersLogos[i].color = Color.gray;

            }
        }
    }

}

