using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FileData", menuName = "ScriptableObjects/FileData", order = 1)]
public class DNDFileData : ScriptableObject
{
    private int lastUsedID;
    private List<RoomData> rooms;
    [SerializeField]
    private Settings settings;
    [SerializeField]
    private Save save;

    public List<RoomData> Rooms
    {
        get { return rooms; }
    }

    public Settings Settings
    {
        get { return settings; }
    }
    public Save Save
    {
        get { return save; }
    }

    private void OnEnable()
    {
        lastUsedID = 0;
        rooms = new List<RoomData>();
        settings = new Settings();
        save = new Save();
    }

    public void AddRoom(Vector3 size, Vector3 position, List<DoorData> listDoors, List<ObjectData> listObjects)
    {
        lastUsedID++;
        RoomData room = new RoomData(size, position, listDoors, listObjects,lastUsedID);
        rooms.Add(room);
    }

    public override string ToString()
    {
        string s = $"Version: {Save.Version}, Seed: {Save.Seed},\n Rooms : ";
        foreach (RoomData room in rooms)
        {
            s += room.ToString();
        }
        return s;
    }
}

[System.Serializable]
public class Settings
{
    [SerializeField]
    private float fov = 60f;

    public float FOV
    {
        get { return fov; }
        set { fov = value; }
    }

    public Settings()
    {
        fov = 60;
    }

    public override string ToString()
    {
        return $"Settings: FOV = {fov}";
    }
}

[System.Serializable]
public class Save
{
    [SerializeField]
    private string filepath = "D:\\1_Git\\Roomstretch\\documentation\\TestDNDFile.dnd";
    private string version;
    private string seed;
    private bool shouldGenRanNoOfRooms;
    private int noOfRooms;
    private int upperBoundNoOfRooms = 10;
    private int lowerBoundNoOfRooms = 1;


    private bool shouldUseNormalBounds;
    private float maxWidth = 10;
    private float minWidth = 2;
    private float maxDepth = 10;
    private float minDepth = 2;


    public string FilePath
    {
        get { return filepath; }
        set { filepath = value; }
    }
    public string Version
    {
        get { return version; }
        set { version = value; }
    }
    public string Seed
    {
        get { return seed; }
        set { seed = value; }
    }
    public System.Random Random
    {
        get { return new System.Random(seed.GetHashCode()); }
    }
    public bool ShouldGenRanNoOfRooms
    {
        get { return shouldGenRanNoOfRooms; }
        set { shouldGenRanNoOfRooms = value; }
    }
    public int NoOfRooms
    {
        get { return noOfRooms; }
        set { noOfRooms = value; }
    }
    public int UpperBoundNoOfRooms
    {
        get { return upperBoundNoOfRooms; }
        set { upperBoundNoOfRooms = value; }
    }
    public int LowerBoundNoOfRooms
    {
        get { return lowerBoundNoOfRooms; }
        set { lowerBoundNoOfRooms = value; }
    }

    public bool ShouldUseNormalBounds
    {
        get { return shouldUseNormalBounds; }
        set { shouldUseNormalBounds = value; }
    }
    public float MaxWidth
    {
        get { return maxWidth; }
        set { maxWidth = value; }
    }
    public float MinWidth
    {
        get { return minWidth; }
        set { minWidth = value; }
    }
    public float MaxDepth
    {
        get { return maxDepth; }
        set { maxDepth = value; }
    }
    public float MinDepth
    {
        get { return minDepth; }
        set { minDepth = value; }
    }

    public override string ToString()
    {
        return $"Save: FilePath = {filepath}, Version = {version}, Seed = {seed}, Should Generate Random Number of Rooms = {shouldGenRanNoOfRooms}, Number of Rooms = {noOfRooms}";
    }
}

public class RoomData
{
    private int id;
    private Vector3 size;
    private Vector3 position;
    private List<DoorData> listDoors;
    private List<ObjectData> listObjects;

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

    public void AddDoor(DoorData door)
    {
        listDoors.Add(door);
    }

    public void AddDoor(Vector3 position, RoomData linkedRoom, int id)
    {
        DoorData door = new DoorData(position, linkedRoom, id);
        listDoors.Add(door);
    }

    public void AddObject(ObjectData obj)
    {
        listObjects.Add(obj);
    }

    public void AddObject(Vector3 position, GameObject prefab)
    {
        ObjectData obj = new ObjectData(position, prefab);
        listObjects.Add(obj);
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

    public DoorData(Vector3 position, RoomData linkedRoom, int id)
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
