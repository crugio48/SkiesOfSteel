
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InstructionButton : MonoBehaviour
{
    [SerializeField] private GameObject fatherPanel;
    [SerializeField] private List<GameObject> instuctionsPanels;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;

    private int _currentPanel;

    private bool _isEnabled = false;

    private Canvas _canvas;


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

    private void EnableCanvas()
    {
        fatherPanel.SetActive(false);

        _currentPanel = 0;

        for (int i = 0; i < instuctionsPanels.Count; i++)
        {
            if (i == _currentPanel)
            {
                instuctionsPanels[i].SetActive(true);
            }
            else
            {
                instuctionsPanels[i].SetActive(false);
            }
        }

        UpdateButtonsInteractions();

        _canvas.enabled = true;
    }


    public void ShowInstructions()
    {
        _isEnabled = !_isEnabled;

        fatherPanel.SetActive(_isEnabled);
    }


    public void NextButton()
    {
        if (_currentPanel >= instuctionsPanels.Count - 1) return;

        instuctionsPanels[_currentPanel].SetActive(false);

        _currentPanel++;

        instuctionsPanels[_currentPanel].SetActive(true);


        UpdateButtonsInteractions();
    }


    public void PreviousButton()
    {
        if (_currentPanel <= 0) return;

        instuctionsPanels[_currentPanel].SetActive(false);

        _currentPanel--;

        instuctionsPanels[_currentPanel].SetActive(true);

        UpdateButtonsInteractions();
    }


    private void UpdateButtonsInteractions()
    {
        if (_currentPanel <= 0)
        {
            nextButton.interactable = true;
            previousButton.interactable = false;
        }
        else if (_currentPanel >= instuctionsPanels.Count - 1)
        {
            nextButton.interactable = false;
            previousButton.interactable = true;
        }
        else
        {
            nextButton.interactable = true;
            previousButton.interactable = true;
        }
    }
}
