using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTeleportScript : MonoBehaviour
{
    [SerializeField] public Vector3 Destination;
    [SerializeField] public GameObject Player;
    private PlayerInputScript playerInputScript;

    private void Awake()
    {
        playerInputScript = Player.GetComponent<PlayerInputScript>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == Player && playerInputScript.ShouldTeleport)
        {
            TeleportPlayer();
        }
    }

    public void TeleportPlayer()
    {
        Player.transform.position = Destination;
    }
}
