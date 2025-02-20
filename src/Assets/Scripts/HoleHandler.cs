using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleHandler : MonoBehaviour
{
    [SerializeField] private int playerLayer;
    [SerializeField] private int holeLayer;
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == playerLayer)
        {
            other.gameObject.layer = holeLayer;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.layer == holeLayer)
        {
            other.gameObject.layer = playerLayer;
        }
    }
}
