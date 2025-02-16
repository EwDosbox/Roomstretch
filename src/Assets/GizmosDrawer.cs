using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmosDrawer : MonoBehaviour
{
    [SerializeField] private DNDFileData fileData;

    private void OnDrawGizmos()
    {
        foreach(RoomData room in fileData.Rooms)
        {
            Gizmos.color = Color.green;
            Vector3 roomCenter = room.Position + room.Size / 2;
            Gizmos.DrawWireCube(roomCenter, room.Size);
        }
    }
}
