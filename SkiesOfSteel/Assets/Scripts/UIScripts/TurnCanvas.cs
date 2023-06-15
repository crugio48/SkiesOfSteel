
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class TurnCanvas : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentPlayerText;
    [SerializeField] private TextMeshProUGUI nextPlayerText;

    [SerializeField] private Button passTurnButton;

    private Canvas _canvas;


    private void Start()
    {
        _canvas = GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        GameManager.Instance.StartGameEvent += EnableCanvas;
        GameManager.Instance.EndTurnEvent += UpdateTheTextFields;
    }

    private void OnDisable()
    {
        GameManager.Instance.StartGameEvent -= EnableCanvas;
        GameManager.Instance.EndTurnEvent -= UpdateTheTextFields;
    }
    
    // This will run only on clients
    private void EnableCanvas()
    {
        _canvas.enabled = true;
    }

    private void UpdateTheTextFields()
    {
        //Take the names of the players and show them into 
        currentPlayerText.text = "Current Player = " + GameManager.Instance.GetCurrentPlayer();
        nextPlayerText.text = "Next Player = " + GameManager.Instance.GetNextPlayer();

        string myUsername = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>().GetUsername();

        Debug.Log("My username is: " + myUsername);

        Debug.Log(GameManager.Instance.GetCurrentPlayer());

        if (myUsername == GameManager.Instance.GetCurrentPlayer())
        {
            passTurnButton.interactable = true;
        }
        else
        {
            passTurnButton.interactable = false;
        }
    }
}

