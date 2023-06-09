
using TMPro;
using UnityEngine;


[RequireComponent(typeof(Canvas))]
public class UIEndGame : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI endGameText;

    private Canvas _endGameCanvas;

    private void Start()
    {
        _endGameCanvas = GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        GameManager.WonGameEvent += WonGame;
        GameManager.LostGameEvent += LostGame;
    }

    private void OnDisable()
    {
        GameManager.WonGameEvent -= WonGame;
        GameManager.LostGameEvent -= LostGame;
    }

    private void LostGame()
    {
        endGameText.text = "You lost!";
        
        _endGameCanvas.enabled = true;
    }

    private void WonGame()
    {
        endGameText.text = "You won!";

        _endGameCanvas.enabled = true;
    }

}
