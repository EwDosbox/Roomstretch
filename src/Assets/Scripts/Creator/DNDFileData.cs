using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

#region  DNDFileData
[CreateAssetMenu(fileName = "FileData", menuName = "ScriptableObjects/FileData", order = 1)]
public class DNDFileData : ScriptableObject
{
    [SerializeField] private int lastUsedID;
    [SerializeField] private List<RoomData> rooms;
    [SerializeField] private Settings settings;
    [SerializeField] private Save save;

    public List<RoomData> Rooms => rooms;
    public Settings Settings => settings;
    public Save Save = > Save;

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

    private void Initialize()
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
            s += room.ToString() + "\n";
        }
        return s;
    }
}

#endregion
#region Settings and Saves
[System.Serializable]
public class Settings
{
    [SerializeField] private float fov;

    [SerializeField] private float sensitivity;

    public float FOV
    {
        get => fov;
        set => fov = value;
    }
    public float Sensitivity
    {
        get => return sensitivity;
        set => sensitivity = value;
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
    [SerializeField] private string filepath = Application.persistentDataPath + "//TestDNDFile.dnd";

    public string FilePath
    {
        get => return filepath; 
        set => filepath = value; 
    }
    public string Version
    {
        get => return version; 
        set => version = value; 
    }
    public string Seed
    {
        get => return seed; 
        set => seed = value; 
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
#endregion
#region Data Classes
[System.Serializable]
public class RoomData : BaseEntityData
{
    private int lastUsedDoorID;
    private int lastUsedObjectID;

    [SerializeField] private Vector3 size;
    [SerializeField] private List<DoorData> listDoors;
    [SerializeField] private List<ObjectData> listObjects;

    public Vector3 Size => size;
    public List<DoorData> Doors => listDoors;
    public List<ObjectData> Objects => listObjects;

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
            s += door.ToString() + "\n";
        }
        s += "\n Objects: ";
        foreach (ObjectData obj in listObjects)
        {
            s += obj.ToString() + "\n";
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

    public T Random<T>(T min, T max) where T : IComparable<T>
    {
        ValidateRange(Convert.ToSingle(min), Convert.ToSingle(max));
        if (typeof(T) == typeof(int))
        {
            return (T)(object)rnd.Next(Convert.ToInt32(min), Convert.ToInt32(max) + 1);
        }
        else if (typeof(T) == typeof(double))
        {
            return (T)(object)(Convert.ToDouble(min) + (rnd.NextDouble() * (Convert.ToDouble(max) - Convert.ToDouble(min))));
        }
        else if (typeof(T) == typeof(float))
        {
            return (T)(object)(Convert.ToSingle(min) + (float)(rnd.NextDouble() * (Convert.ToSingle(max) - Convert.ToSingle(min))));
        }
        else if(typeof(T) == typeof(Bounds))
        {
            return new Bounds<T>(Random(bounds.ExtremesBounds.Min, bounds.ExtremesBounds.Max), bounds.ExtremesBounds);
        }
        else
        {
            throw new ArgumentException("Unsupported type");
        }
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

    private void ValidateRange(float min, float max)
    {
        if (min > max) throw new ArgumentException("Min must be <= Max");
    }
}
[System.Serializable]
public class GenerationBounds<T> where T : IComparable<T>
{
    private bool shouldGenerate;
    private T value;
    private T defaultValue;
    private Bounds<T> extremesBounds;

    public bool ShouldGenerate
    {
        get => shouldGenerate;
        set => shouldGenerate = value;
    }
    public T Value => value;
    public T DefaultValue => defaultValue;
    public Bounds ExtremesBounds => extremesBounds;

    public GenerationBounds(T defaultValue, Bounds<T> extremes)
    {
        shouldGenerate = false;
        this.defaultValue = defaultValue;
        this.extremesBounds = extremes;
        this.value = defaultValue;
    }

    public void Generate(BetterRandom rnd)
    {
        if (shouldGenerate)
        {
            if (defaultValue is int)
                value = rnd.Random(extremesBounds.Min, extremesBounds.Max);
            else if (defaultValue is float)
                value = rnd.Random(extremesBounds.Min, extremesBounds.Max);
            else if (defaultValue is double)
                value = rnd.Random(extremesBounds.Min, extremesBounds.Max);
            else if (defaultValue is Vector3)
                value = rnd.RandomVector3(extremesBounds, extremesBounds);
        }
        else
        {
            value = defaultValue;
        }
    }
    public override string ToString() =>
            $"GenerationBounds: ShouldGenerate={shouldGenerate}, Value={value}, Default={defaultValue}, Bounds={extremesBounds.ToString()}";
}

[System.Serializable]
public struct Bounds<T> where T : IComparable<T>
{
    private T min;
    private T max;
    public T Min => min;
    public T Max => max;

    public Bounds(T min, T max)
    {
        this.min = min;
        this.max = max;
    }

    public bool IsValid => Min.CompareTo(Max) <= 0;

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