using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private GameObject cam;
    [SerializeField]
    private GameObject player;

    private void Awake()
    {
        cam = this.gameObject;
    }

    private void LateUpdate()
    {
        cam.transform.position = player.transform.position;
    }
}
