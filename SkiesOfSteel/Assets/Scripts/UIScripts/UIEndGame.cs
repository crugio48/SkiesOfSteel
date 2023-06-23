
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        GameManager.Instance.WonGameEvent += WonGame;
        GameManager.Instance.LostGameEvent += LostGame;
    }

    private void OnDisable()
    {
        GameManager.Instance.WonGameEvent -= WonGame;
        GameManager.Instance.LostGameEvent -= LostGame;
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


    public void BackToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
