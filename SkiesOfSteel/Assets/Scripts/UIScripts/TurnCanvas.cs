using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnCanvas : MonoBehaviour
{

    void Update()
    {
        //Take the names of the players and show them into 
        transform.GetChild(0).GetChild(1).GetComponentInChildren<Text>().text = "Current Player = " + GameManager.Instance.GetCurrentPlayer();
        transform.GetChild(0).GetChild(1).GetComponentInChildren<Text>().text = "Current Player = " + GameManager.Instance.GetNextPlayer();

    }
}
