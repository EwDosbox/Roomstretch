using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    private void Awake()
    {
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
    [SerializeField] private string version = "1.0";
    [SerializeField] private string seed;
    [SerializeField] private GenerationBounds roomCountBounds;
    [SerializeField] private GenerationBounds widthBounds;
    [SerializeField] private GenerationBounds depthBounds;
    [SerializeField] private string filepath = "D:\\_GIT\\Roomstretch\\documentation\\TestDNDFile.dnd";

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

    public GenerationBounds RoomsCountBounds => roomCountBounds;
    public GenerationBounds WidthBounds => widthBounds;
    public GenerationBounds DepthBounds => depthBounds;
    public BetterRandom Random => new BetterRandom(hashedSeed);

    private int hashedSeed
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
        roomCountBounds = new GenerationBounds(new Bounds(1, 5), new Bounds(1, 10));
        widthBounds = new GenerationBounds(new Bounds(5, 10), new Bounds(5, 20));
        depthBounds = new GenerationBounds(new Bounds(5, 10), new Bounds(5, 20));
    }

    public override string ToString()
    {
        return $"Save: Version = {version}, Seed = {seed}, FilePath = {filepath}, " +
               $"RoomCountBounds = {roomCountBounds.ToString()}, WidthBounds = {widthBounds.ToString()}, DepthBounds = {depthBounds.ToString()}";
    }
}

#region Data Classes
[System.Serializable]
public class RoomData : BaseEntityData
{
    private int lastUsedDoorID;
    private int lastUsedObjectID;

    [SerializeField] private Vector3 size;
    [SerializeField] private List<DoorData> listDoors;
    [SerializeField] private List<ObjectData> listObjects;

    public Vector3 Size
    {
        get { return size; }
    }

    public List<DoorData> Doors
    {
        get { return listDoors; }
    }

    public List<ObjectData> Objects
    {
        get { return listObjects; }
    }

    public RoomData(Vector3 size, Vector3 position, int id) : base(position, id)
    {
        this.size = size;
        this.listDoors = new List<DoorData>();
        this.listObjects = new List<ObjectData>();
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
public class DoorData : BaseEntityData
{
    private int linkedRoomID;

    public int LinkedRoomID => linkedRoomID;

    public DoorData(Vector3 position, int linkedRoomID, int id) : base(position, id)
    {
        this.linkedRoomID = linkedRoomID;
    }
    public override string ToString()
    {
        return base.ToString() + $"Linked Room ID: {linkedRoomID}";
    }
}

[System.Serializable]
public class ObjectData : BaseEntityData
{
    [SerializeField] private GameObject prefab;
    public GameObject Object => prefab;

    public ObjectData(Vector3 position, GameObject prefab, int id) : base(position, id)
    {
        this.prefab = prefab;
    }

    public override string ToString()
    {
        return base.ToString() + $"Object: {prefab.name}";
    }
}
#endregion
#region Helping Classes
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
#endregion
#region Generation


public class BetterRandom
{
    private readonly System.Random rnd;

    public BetterRandom(int seed)
    {
        rnd = new System.Random(seed);
    }

    public int Random(int a, int b)
    {
        ValidateRange(a, b);
        return rnd.Next(a, b + 1);
    }

    public double Random(double a, double b)
    {
        ValidateRange((float)a, (float)b);
        return a + (rnd.NextDouble() * (b - a));
    }

    public float Random(float a, float b)
    {
        ValidateRange(a, b);
        return a + (float)(rnd.NextDouble() * (b - a));
    }
    public Vector3 RandomVector3(GenerationBounds width, GenerationBounds depth, GenerationBounds height)
    {
        return new Vector3(
            this.Random(width.Bounds.Max, width.Bounds.Min),
            this.Random(height.Bounds.Max, height.Bounds.Min),
            this.Random(depth.Bounds.Max, depth.Bounds.Min)
        );
    }

    public Vector3 RandomVector3(GenerationBounds width, GenerationBounds depth)
    {
        return new Vector3(
            this.Random(width.Bounds.Max, width.Bounds.Min),
            0,
            this.Random(depth.Bounds.Max, depth.Bounds.Min)
        );
    }

    public (float, float) RandomBounds(GenerationBounds bounds)
    {
        return (this.Random(bounds.Bounds.Max, bounds.Bounds.Min), this.Random(bounds.Bounds.Max, bounds.Bounds.Min));
    }
    private void ValidateRange(float min, float max)
    {
        if (min > max) throw new ArgumentException("Min must be <= Max");
    }
}
public class GenerationBounds
{
    private int noOfGenerations;
    private bool shouldGenerate;
    private Bounds bounds;
    private Bounds defaultBounds;
    private Bounds extremesBounds;
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

    public Bounds Bounds => bounds;

    public Bounds DefaultBounds
    {
        get { return defaultBounds; }
        set { defaultBounds = value; }
    }

    public Bounds ExtremesBounds
    {
        get { return extremesBounds; }
        set { extremesBounds = value; }
    }
    public GenerationBounds(Bounds defaultBounds, Bounds extremes)
    {
        noOfGenerations = 0;
        shouldGenerate = false;
        bounds = defaultBounds;
        this.defaultBounds = defaultBounds;
        this.extremesBounds = extremes;
    }

    public void Generate(BetterRandom rnd)
    {
        if (shouldGenerate)
        {
            bounds = new Bounds(rnd.Random(extremesBounds.Max, extremesBounds.Min),
                rnd.Random(extremesBounds.Max, extremesBounds.Max));
        }
        else
        {
            bounds = defaultBounds;
        }
    }
    public override string ToString()
    {
        return $"GenerationBounds: NoOfGenerations = {noOfGenerations}, ShouldGenerate = {shouldGenerate}, " +
            $"Bounds = {bounds.ToString()}, DefaultBounds = {defaultBounds.ToString()}, ExtremesBounds = {extremesBounds.ToString()}";
    }
}

[System.Serializable]
public struct Bounds
{
    public float Min;
    public float Max;

    public Bounds(float min, float max)
    {
        Min = min;
        Max = max;
    }

    public bool IsValid => Min <= Max;

    public override string ToString()
    {
        return $"Bounds: Min = {Min}, Max = {Max}";
    }
}
#endregion
#region Abstract Classes

[System.Serializable]
public abstract class BaseEntityData
{
    [SerializeField] protected int id;
    [SerializeField] protected Vector3 position;

    public int ID => id;
    public Vector3 Position => position;

    protected BaseEntityData(Vector3 position, int id)
    {
        this.position = position;
        this.id = id;
    }

    public override string ToString()
    {
        return $"ID: {id}; Position: {position}";
    }
}

#endregion