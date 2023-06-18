using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InstructionButton : MonoBehaviour
{
    [SerializeField] private Image instructionBackground;
    [SerializeField] private TextMeshProUGUI instructionText;

    [SerializeField] private Button showInstructionsButton;

    void Start()
    {
        
    }

    // Update is called once per frame
    public void ShowInstructions()
    {
        if (!instructionBackground.enabled)
        {
            instructionBackground.enabled = true;
            instructionText.enabled = true;
        }
        else
        {
            instructionBackground.enabled = false;
            instructionText.enabled = false;
        }
    }
}
