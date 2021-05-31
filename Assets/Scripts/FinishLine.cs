using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.StartsWith("Player"))
        {
            if (!other.gameObject.GetComponent<PlayerInfo>().Backwards)
            {
                if (other.gameObject.GetComponent<PlayerInfo>().CorrectCurrentLap == other.gameObject.GetComponent<PlayerInfo>().CurrentLap)
                {
                    other.gameObject.GetComponent<PlayerInfo>().CurrentLap++;
                }
                other.gameObject.GetComponent<PlayerInfo>().CorrectCurrentLap++;
            }
            else
            {
                other.gameObject.GetComponent<PlayerInfo>().CorrectCurrentLap--;
            }
        }
    }
}
