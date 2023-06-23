using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class UIStartGame : MonoBehaviour
{
    [SerializeField] private GameObject waitingPlayersText;

    [SerializeField] private GameObject selectUsernameMenu;

    [SerializeField] private TMP_InputField inputField;

    [SerializeField] private Canvas thisCanvas;

    private void Start()
    {
        waitingPlayersText.SetActive(false);
        selectUsernameMenu.SetActive(true);
    }

    public void EnableCanvas()
    {
        thisCanvas.enabled = true;
    }


    private void OnEnable()
    {
        GameManager.Instance.StartGameEvent += DisableCanvas;
        GameManager.Instance.UsernameSelected += UsernameSelected;
    }

    private void OnDisable()
    {
        GameManager.Instance.StartGameEvent -= DisableCanvas;
        GameManager.Instance.UsernameSelected -= UsernameSelected;
    }

    private void DisableCanvas()
    {
        thisCanvas.enabled = false;
    }


    public void SelectedUsernameButtonPress()
    {
        string username = inputField.text;

        if (string.IsNullOrEmpty(username))
        {
            inputField.text = "Cannot be empty or null!";
            return;
        }
        if (username.Length > 15)
        {
            inputField.text = "Max 15 characters!";
            return;
        }

        GameManager.Instance.SelectUsernameServerRpc(username);

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
