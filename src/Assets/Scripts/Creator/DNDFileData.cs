using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FileData", menuName = "ScriptableObjects/FileData", order = 1)]
public class DNDFileData : ScriptableObject
{
    private int lastUsedID;
    [SerializeField]
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
        set { save = value; }
    }


    public void AddRoom(Vector3 size, Vector3 position, List<DoorData> listDoors, List<ObjectData> listObjects)
    {
        lastUsedID++;
        RoomData room = new RoomData(size, position, lastUsedID);

        foreach (DoorData door in listDoors)
        {
            room.AddDoor(door.Position, door.LinkedRoomID);
        }

        foreach (ObjectData obj in listObjects)
        {
            room.AddObject(obj.Position, obj.Object);
        }

        rooms.Add(room);
    }

    private void OnEnable()
    {
        lastUsedID = 0;
        rooms = new List<RoomData>();
        settings = new Settings();
        save = new Save();
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
    private float fov;

    [SerializeField]
    private float sensitivity;

    public float FOV
    {
        get { return fov; }
        set { fov = value; }
    }
    public float Sensitivity
    {
        get { return sensitivity; }
        set { sensitivity = value; }
    }

    public Settings()
    {
        fov = 60;
        sensitivity = 2f;
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
    private string filepath = "D:\\_GIT\\Roomstretch\\documentation\\TestDNDFile.dnd";
    private string version;
    private string seed;
    private bool shouldGenRanNoOfRooms;

    private GenerationBounds roomsBounds;
    private GenerationBounds mapWidthBounds;
    private GenerationBounds mapDepthBounds;

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

    public GenerationBounds RoomsBounds
    {
        get { return roomsBounds; }
    }

    public GenerationBounds MapWidthBounds
    {
        get { return mapWidthBounds; }
    }

    public GenerationBounds MapDepthBounds
    {
        get { return mapDepthBounds; }
    }

    public BetterRandom Random
    {
        get { return new BetterRandom(HashedSeed); }
    }

    private int HashedSeed
    {
        get
        {
            if (string.IsNullOrEmpty(seed))
                return 0;

            const uint FNV_offset_basis = 2166136261;
            const uint FNV_prime = 16777619;

            uint hash = FNV_offset_basis;

            foreach (char c in seed)
            {
                hash ^= c;
                hash *= FNV_prime;
            }

            return (int)hash;
        }
    }

    public Save()
    {
        version = "1.0";
        roomsBounds = new GenerationBounds((1, 10),20, 1);
        mapWidthBounds = new GenerationBounds((-10, 10), 20, -20);
        mapDepthBounds = new GenerationBounds((-10, 10), 20, -20);
    }

    public override string ToString()
    {
        return $"Save: FilePath = {filepath}, Version = {version}, Seed = {seed}, Should Generate Random Number of Rooms = {shouldGenRanNoOfRooms}, Number of Rooms = {roomsBounds.NoOfGenerations}";
    }
}

[System.Serializable]
public class RoomData
{
    private int lastUsedDoorID;
    private int lastUsedObjectID;

    private int id;
    [SerializeField]
    private Vector3 size;
    [SerializeField]
    private Vector3 position;
    [SerializeField]
    private List<DoorData> listDoors;
    [SerializeField]
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

    public RoomData(Vector3 size, Vector3 position, int id)
    {
        this.size = size;
        this.position = position;
        this.listDoors = new List<DoorData>();
        this.listObjects = new List<ObjectData>();
        this.id = id;
        lastUsedDoorID = 0;
        lastUsedObjectID = 0;
    }

    public void AddDoor(Vector3 position, int linkedRoomID)
    {
        DoorData door = new DoorData(position, linkedRoomID, ++lastUsedDoorID);
        listDoors.Add(door);
    }

    public void AddObject(Vector3 position, GameObject prefab)
    {
        ObjectData obj = new ObjectData(position, prefab, ++lastUsedObjectID);
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

[System.Serializable]
public class DoorData
{
    private int iD;
    private int linkedRoomID;
    [SerializeField]
    private Vector3 position;

    public int ID
    {
        get { return iD; }
    }
    public int LinkedRoomID
    {
        get { return linkedRoomID; }
    }

    public Vector3 Position
    {
        get { return position; }
    }

    public DoorData(Vector3 position, int linkedRoomID, int id)
    {
        this.position = position;
        this.linkedRoomID = linkedRoomID;
        this.iD = id;
    }

    public override string ToString()
    {
        return $"Door ID: {ID}; Position: {position}; Linked Room ID: {linkedRoomID}";
    }
}

[System.Serializable]
public class ObjectData
{
    private int iD;
    [SerializeField]
    private Vector3 position;
    [SerializeField]
    private GameObject prefab;

    public int ID
    {
        get { return iD; }
    }
    public Vector3 Position
    {
        get { return position; }
    }

    public GameObject Object
    {
        get { return prefab; }
    }

    public ObjectData(Vector3 position, GameObject prefab, int id)
    {
        this.iD = id;
        this.position = position;
        this.prefab = prefab;
    }
    public override string ToString()
    {
        return $"Position: {position}; Object: {prefab.name}";
    }
}

public class BetterRandom
{
    private System.Random rnd;

    public BetterRandom(int seed)
    {
        rnd = new System.Random(seed);
    }

    public int Random(int a, int b)
    {
        return rnd.Next(a, b + 1);
    }

    public double Random(double a, double b)
    {
        return a + (rnd.NextDouble() * (b - a));
    }

    public float Random(float a, float b)
    {
        return a + (float)(rnd.NextDouble() * (b - a));
    }

    public Vector3 RandomVector3(GenerationBounds width, GenerationBounds depth, GenerationBounds height)
    {
        return new Vector3(
            this.Random(width.Bounds.Item1, width.Bounds.Item2),
            this.Random(height.Bounds.Item1, height.Bounds.Item2),
            this.Random(depth.Bounds.Item1, depth.Bounds.Item2)
        );
    }

    public Vector3 RandomVector3(GenerationBounds width, GenerationBounds depth)
    {
        return new Vector3(
            this.Random(width.Bounds.Item1, width.Bounds.Item2),
            0,
            this.Random(depth.Bounds.Item1, depth.Bounds.Item2)
        );
    }
    public (float, float) RandomBounds(GenerationBounds bounds)
    {
        return (this.Random(bounds.MinBounds.Item1, bounds.MaxBounds.Item1), this.Random(bounds.MinBounds.Item2, bounds.MaxBounds.Item2));
    }
}
public class Rectangle
{
    private float width;
    private float depth;
    private float x;
    private float z;

    public Vector2 Position
    {
        get { return new Vector2(x, z); }
    }
    public Vector2 Size
    {
        get { return new Vector2(width, depth); }
    }

    public Rectangle(float Width, float Depth, float X, float Z)
    {
        this.width = Width;
        this.depth = Depth;
        this.x = X;
        this.z = Z;
    }

    public Rectangle(Vector2 position, Vector2 size)
    {
        this.width = size.x;
        this.depth = size.y;
        this.x = position.x;
        this.z = position.y;
    }

    public bool AreOverlapping(Rectangle other)
    {
        bool notOverlapping =
            this.x + this.width < other.x || // This rectangle is to the left of the other
            other.x + other.width < this.x || // This rectangle is to the right of the other
            this.z + this.depth < other.z || // This rectangle is above the other
            other.z + other.depth < this.z;  // This rectangle is below the other

        return !notOverlapping;
    }
    public override string ToString()
    {
        return $"Rectangle: Position = ({x}, {z}), Size = ({width}, {depth})";
    }
}

public class GenerationBounds
{
    private int noOfGenerations;
    private bool shouldGenerate;
    private (float, float) bounds;
    private (float, float) defaultBounds;
    private (float, float) maxBounds;
    private (float, float) minBounds;

    public int NoOfGenerations
    {
        get { return noOfGenerations; }
        set { noOfGenerations = value; }
    }

    public bool ShouldGenerate
    {
        get { return shouldGenerate; }
        set { shouldGenerate = value; }
    }

    public (float, float) Bounds
    {
        get { return bounds; }
        set { bounds = value; }
    }

    public (float, float) DefaultBounds
    {
        get { return defaultBounds; }
        set { defaultBounds = value; }
    }

    public (float, float) MaxBounds
    {
        get { return maxBounds; }
        set { maxBounds = value; }
    }
    public (float, float) MinBounds
    {
        get { return minBounds; }
        set { minBounds = value; }
    }
    public GenerationBounds((float, float) defaultBounds, float maxBound, float minBound)
    {
        noOfGenerations = 0;
        shouldGenerate = false;
        bounds = defaultBounds;
        this.defaultBounds = defaultBounds;
        this.maxBounds = (maxBound, maxBound);
        this.minBounds = (minBound, minBound);

    }
    public GenerationBounds((float, float) defaultBounds, (float, float) maxBounds, (float, float) minBounds)
    {
        noOfGenerations = 0;
        shouldGenerate = false;
        bounds = defaultBounds;
        this.defaultBounds = defaultBounds;
        this.maxBounds = maxBounds;
        this.minBounds = minBounds;
    }

    public void Generate(BetterRandom rnd){
        if(shouldGenerate)
        {
            bounds = rnd.RandomBounds(this);
        }
        else
        {
            bounds = defaultBounds;
        }
    }
}