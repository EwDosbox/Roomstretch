using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DNDFileData
{
    public int LastUsedID { get; set; }
    public string Version { get; set; }
    public string Seed { get; set; }
    public List<RoomData> Rooms { get; set; }

    public DNDFileData(string version, string seed)
    {
        LastUsedID = 0;
        Version = version;
        Seed = seed;
        Rooms =  new List<RoomData>();
    }

    public void AddRoom(RoomData room)
    {
        Rooms.Add(room);
    }
}

public class RoomData
{
    public int id { get; set; }
    public Vector3 Size { get; set; }
    public List<DoorData> Doors { get; set; }
    public List<ObjectData> Objects { get; set; }

    public RoomData(Vector3 size, List<DoorData> doors, List<ObjectData> objects, DNDFileData file)
    {
        id = file.LastUsedID++;
        Size = size;
        Doors = doors;
        Objects = objects;
    }
}

public class DoorData
{
    public int DoorID { get; set; }
    public Vector3 Position { get; set; }
    public int LinkedRoomID { get; set; }

    public DoorData(string doorName, Vector3 position = default, int linkedRoomID)
    {
        DoorName = doorName;
        Position = position;
        LinkedRoomID = linkedRoomID;
    }
}

public class ObjectData
{
    public string ObjectName { get; set; }
    public Vector3 Position { get; set; }
    public string PrefabName { get; set; }

    public ObjectData(string objectName = "Default Object", Vector3 position = default, string prefabName = "DefaultPrefab")
    {
        ObjectName = objectName;
        Position = position;
        PrefabName = prefabName;
    }
}
