using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class UIStartGame : MonoBehaviour
{
    private Canvas _waitingStartCanvas;

    private void Start()
    {
        _waitingStartCanvas = GetComponent<Canvas>();
    }


    private void OnEnable()
    {
        GameManager.StartGameEvent += DisableCanvas;
    }

    private void OnDisable()
    {
        GameManager.StartGameEvent -= DisableCanvas;
    }

    private void DisableCanvas()
    {
        _waitingStartCanvas.enabled = false;
    }
}
