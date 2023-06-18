
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InstructionButton : MonoBehaviour
{
    [SerializeField] private Image instructionBackground;
    [SerializeField] private TextMeshProUGUI instructionText;

    [SerializeField] private Button showInstructionsButton;


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
