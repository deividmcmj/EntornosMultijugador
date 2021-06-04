using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private UIManager _uiManager;
    private void Awake()
    {
        _uiManager = FindObjectOfType<UIManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        other.gameObject.GetComponent<PlayerInfo>().CrossFinishLine();
        if (other.gameObject.GetComponent<PlayerInfo>().GetFinished())
        {
            _uiManager.FinishRace();
        }
    }
}
