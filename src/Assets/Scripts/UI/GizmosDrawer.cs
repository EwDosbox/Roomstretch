using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GizmosDrawer : MonoBehaviour
{
    [SerializeField] private DNDFileData fileData;

    private void OnDrawGizmos()
    {
        foreach(Wall wall in fileData.Walls)
        {
            if(wall.Orientation == Orientation.N) Gizmos.color = Color.red;
            else if(wall.Orientation == Orientation.E) Gizmos.color = Color.green;
            else if(wall.Orientation == Orientation.S) Gizmos.color = Color.blue;
            else if(wall.Orientation == Orientation.W) Gizmos.color = Color.yellow;

            Gizmos.DrawLine(wall.Start, wall.End);
        }
        foreach (DoorData door in fileData.Doors)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(door.Position, 0.5f);
        }
        foreach (RoomData room in fileData.Rooms)
        {
            Gizmos.color = Color.blue;
            Vector3 roomCenter = new Vector3(room.Position.x + room.Size.x / 2, 0, room.Position.z + room.Size.z / 2);
            Gizmos.DrawCube(roomCenter, room.Size);
        }        
    }
}
