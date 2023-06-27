
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InstructionButton : MonoBehaviour
{
    [SerializeField] private Image instructionBackground;
    [SerializeField] private TextMeshProUGUI instructionText;

    [SerializeField] private Button showInstructionsButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;

    private int currentPage;
    Dictionary<int, string> Pages = new Dictionary<int, string>()
        {
            {0,"Press Left Click to select a ship. Once a ship is selected you can cycle between the ships of the same fleet by clicking on the other ships in the map or on its icon in the left of the screen (this works for both for your fleet and for the enemies).This is usefull to see ships stats and abilities.The menu on the rights displays the selected ship.By clicking on the Captain art you will see the ship's art which contains all the important attributes. By hovering over the buttons abilities you can see their effects and stats. By clickcing on them and selecting a target (if necessary) you will be able to use the ability. Click the right button once a ship is selected to move." },
            {1, "The triangles in the map are ports, in which you can use the refuel and heal up abilities. You can use the ports both inside its tiles and in all adjacent ones.The objective is to obtain the treasure in the middle of the map and bringing it back to the port you spawned from.You can also eliminate all enemies ships."},

        };

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
        _canvas.enabled = true;
    }


    public void ShowInstructions()
    {
        if (!instructionBackground.enabled)
        {
            instructionBackground.enabled = true;
            instructionText.enabled = true;
            nextButton.enabled = true;
            previousButton.enabled = true;
            currentPage = 0;
        }
        else
        {
            instructionBackground.enabled = false;
            instructionText.enabled = false;
            nextButton.enabled = false;
            previousButton.enabled = false;
        }
    }
    public void NextButton()
    {
        if (currentPage != Pages.Count)
        {
            currentPage += 1;
            instructionText.text = Pages[currentPage];

        }


    }
    public void PreviousButton()
    {
        if (currentPage != 0)
        {
            currentPage -= 1;
            instructionText.text = Pages[currentPage];
        }


    }
}
