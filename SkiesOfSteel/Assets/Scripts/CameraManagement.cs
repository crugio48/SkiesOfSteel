using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CameraManagement : MonoBehaviour
{
    private Camera _camera;

    private bool _gameStarted = false;

    [SerializeField] private float maxX;
    [SerializeField] private float maxY;

    [SerializeField] private float minSize;
    [SerializeField] private float maxSize;

    // Start is called before the first frame update
    void Start()
    {
        _camera = Camera.main;
    }


    private void OnEnable()
    {
        GameManager.Instance.StartGameEvent += StartReceivingInput;
    }

    private void OnDisable()
    {
        GameManager.Instance.StartGameEvent -= StartReceivingInput;
    }

    private void StartReceivingInput()
    {
        _gameStarted = true;
    }


    // Update is called once per frame
    void Update()
    {
        if (_camera == null) return;

        if (!_gameStarted) return;

        if (NetworkManager.Singleton.IsServer) return;


        Move();

        Zoom();
        
    }


    private void Move()
    {
        float reducerValue = 0.02f;

        float xAxisValue = Input.GetAxis("Horizontal") * reducerValue;
        float yAxisValue = Input.GetAxis("Vertical") * reducerValue;

        Vector3 position = _camera.transform.position;

        position.x = Mathf.Max(-maxX, Mathf.Min(maxX, position.x + xAxisValue));

        position.y = Mathf.Max(-maxY, Mathf.Min(maxY, position.y + yAxisValue));

        _camera.transform.position = position;
    }


    void Zoom()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && (Input.GetAxis("Mouse ScrollWheel") + _camera.orthographicSize) > minSize)
        {
            for (int sensitivityOfScrolling = 1; sensitivityOfScrolling > 0; sensitivityOfScrolling--)
                _camera.orthographicSize--;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0 && (Input.GetAxis("Mouse ScrollWheel") + _camera.orthographicSize) < maxSize)
        {
            for (int sensitivityOfScrolling = 1; sensitivityOfScrolling > 0; sensitivityOfScrolling--)
                _camera.orthographicSize++;
        }
    }
}
