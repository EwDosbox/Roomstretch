using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GizmosDrawer : MonoBehaviour
{
    [SerializeField] private DNDFileData fileData;


    private void OnDrawGizmos()
    {
        foreach (RoomData room in fileData.Rooms)
        {
            Gizmos.color = Color.red;
            Vector3 roomCenter = room.Position + room.Size / 2;
            Gizmos.DrawWireCube(roomCenter, room.Size);
        }
        foreach (DoorData door in fileData.Doors)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(door.Position, 0.5f);
        }

    }

}
