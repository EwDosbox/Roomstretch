using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GizmosDrawer : MonoBehaviour
{
    [SerializeField] private DNDFileData fileData;

    private void OnDrawGizmos()
    {
        foreach (DoorConection door in fileData.Doors)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(door.Door.Position, 0.5f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(door.TeleportDoor.Position, 0.5f);
        }
        /*
        foreach (RoomData room in fileData.Rooms)
        {
            Gizmos.color = Color.blue;
            Vector3 roomCenter = new Vector3(room.Position.x + room.Size.x / 2, 0, room.Position.z + room.Size.z / 2);
            Gizmos.DrawCube(roomCenter, room.Size);
        }        
        */
    }
}
