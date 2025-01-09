using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DNDFileData
{
    private int lastUsedID;
    private string version;
    private string seed;
    private List<RoomData> rooms;

    public string Version
    {
        get { return version; }
    }
    public string Seed
    {
        get { return seed; }
    }
    public List<RoomData> Rooms
    {
        get { return rooms; }
    }

    public DNDFileData(string version, string seed)
    {
        lastUsedID = 0;
        this.version = version;
        this.seed = seed;
        rooms = new List<RoomData>();
    }

    public void AddRoom(Vector3 size, Vector3 position, List<DoorData> listDoors, List<ObjectData> listObjects)
    {
        lastUsedID++;
        rooms.Add(new RoomData(size, position, listDoors, listObjects, lastUsedID));
    }

    public override string ToString()
    {
        string s = $"Version: {version}, Seed: {seed},\n Rooms : ";
        foreach (RoomData room in rooms)
        {
            s += room.ToString();
        }
        return s;
    }
}

public class RoomData
{
    private int id;
    private Vector3 size;
    private Vector3 position;
    private List<DoorData> listDoors
    ;
    private List<ObjectData> listObjects
    ;


    public int Id
    {
        get { return id; }
    }
    public Vector3 Size
    {
        get { return size; }
    }
    public Vector3 Position
    {
        get { return position; }
    }
    public List<DoorData> Doors
    {
        get { return listDoors; }
    }
    public List<ObjectData> Objects
    {
        get { return listObjects; }
    }

    public RoomData(Vector3 size, Vector3 position, List<DoorData> listDoors, List<ObjectData> listObjects, int id)
    {
        this.size = size;
        this.position = position;
        this.listDoors = listDoors;
        this.listObjects = listObjects;
        this.id = id;
    }

    public override string ToString()
    {
        string s = $"\nRoom ID: {id};\nSize: {size};\nPosition: {position};\nDoors: ";
        foreach (DoorData door in listDoors)
        {
            s += door.ToString();
        }
        s += "\n Objects: ";
        foreach (ObjectData obj in listObjects)
        {
            s += obj.ToString();
        }
        return s;
    }
}

public class DoorData
{
    private int doorID;
    private RoomData linkedRoom;
    private Vector3 position;

    public int DoorID
    {
        get { return doorID; }
    }
    public RoomData LinkedRoom
    {
        get { return linkedRoom; }
    }
    public Vector3 Position
    {
        get { return position; }
    }

    public DoorData(Vector3 position , RoomData linkedRoom, int id)
    {
        this.position = position;
        this.linkedRoom = linkedRoom;
        this.doorID = id;
    }

    public override string ToString()
    {
        return $"Door ID: {doorID}; Position: {position}; Linked Room ID: {linkedRoom.Id}";
    }
}

public class ObjectData
{
    private Vector3 position;
    private GameObject prefab;

    public Vector3 Position
    {
        get { return position; }
    }
    public GameObject Object
    {
        get { return prefab; }
    }

    public ObjectData(Vector3 position, GameObject prefab)
    {
        this.position = position;
        this.prefab = prefab;
    }
    public override string ToString()
    {
        return $"Position: {position}; Object: {prefab.name}";
    }
}
