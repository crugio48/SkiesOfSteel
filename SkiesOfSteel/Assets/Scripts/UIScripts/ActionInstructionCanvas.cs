using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionInstructionCanvas : MonoBehaviour
{
    void Update()
    {
        
    }
    public void ChangeActionDescription(string text)
    {
        transform.GetChild(0).GetChild(1).GetComponent<Text>().text = text;
    }
    public void EnableCanvas()
    {
        transform.GetChild(0).gameObject.SetActive(true);
    }
    public void DisableCanvas()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }
    public void ChangeTextDescription(string text)
    {
        transform.GetChild(0).GetChild(0).GetComponent<Text>().text = text;
    }
}
