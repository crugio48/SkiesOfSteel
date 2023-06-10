using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class UIStartGame : MonoBehaviour
{
    [SerializeField] private GameObject waitingPlayersText;

    [SerializeField] private GameObject selectUsernameMenu;

    [SerializeField] private TMP_InputField inputField;

    [SerializeField] private GameManager gameManager;

    private Canvas _waitingStartCanvas;

    private void Start()
    {
        _waitingStartCanvas = GetComponent<Canvas>();
        waitingPlayersText.SetActive(false);
        selectUsernameMenu.SetActive(true);
    }


    private void OnEnable()
    {
        GameManager.StartGameEvent += DisableCanvas;
        GameManager.UsernameSelected += UsernameSelected;
    }

    private void OnDisable()
    {
        GameManager.StartGameEvent -= DisableCanvas;
        GameManager.UsernameSelected -= UsernameSelected;
    }

    private void DisableCanvas()
    {
        _waitingStartCanvas.enabled = false;
    }


    public void SelectedUsernameButtonPress()
    {
        string username = inputField.text;

        if (string.IsNullOrEmpty(username))
        {
            inputField.text = "Cannot be empty or null!";
            return;
        }
        if (username.Length > 30)
        {
            inputField.text = "Max 30 characters!";
            return;
        }

        gameManager.SelectUsername(username);

    }

    private void UsernameSelected(bool outcome)
    {
        if (!outcome)
        {
            inputField.text = "Username already selected!";
        }
        else
        {
            waitingPlayersText.SetActive(true);
            selectUsernameMenu.SetActive(false);
        }
    }

}
