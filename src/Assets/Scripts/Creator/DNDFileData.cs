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
    [SerializeField] private List<DoorData> doors;
    [SerializeField] private List<Wall> walls;
    [SerializeField] private Settings settings;
    [SerializeField] private Save save;

    public List<RoomData> Rooms
    {
        get => rooms;
        set => rooms = value;
    }
    public List<DoorData> Doors => doors;
    public List<Wall> Walls => walls;
    public Settings Settings => settings;

    public Save Save => save;

    public void AddRoom(Vector3 size, Vector3 position, List<ObjectData> listObjects)
    {
        lastUsedID++;
        RoomData room = new RoomData(size, position, lastUsedID);

        foreach (ObjectData obj in listObjects)
        {
            room.AddObject(obj.Position, obj.Object);
        }

        rooms.Add(room);
    }
    public void AddDoor(Vector3 position, int linkedRoomID)
    {
        doors.Add(new DoorData(position, linkedRoomID, ++lastUsedID));
    }

    public void Initialize()
    {
        lastUsedID = 0;
        rooms = new List<RoomData>();
        doors = new List<DoorData>();
        walls = new List<Wall>();
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
#region Settings
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
        get => sensitivity;
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
#endregion
#region Save
[System.Serializable]
public class Save
{
    [SerializeField] private string version = "1.0";
    [SerializeField] private string seed;
    [SerializeField] private string filepath;
    [SerializeField] private GenerationBounds<int> roomCountBounds;
    [SerializeField] private GenerationBounds<int> doorCountBounds;
    [SerializeField] private Bounds<float> xRoomBounds;
    [SerializeField] private Bounds<float> zRoomBounds;
    [SerializeField] private Bounds<float> xMapBounds;
    [SerializeField] private Bounds<float> zMapBounds;

    public string FilePath
    {
        get => Application.persistentDataPath + "/TestDNDFile.dnd";
        set => filepath = value;
    }
    public string Version
    {
        get => version;
        set => version = value;
    }
    public string Seed
    {
        get => seed;
        set => seed = value;
    }

    public GenerationBounds<int> RoomsCountBounds
    {
        get => roomCountBounds;
        set => roomCountBounds = value;
    }
    public GenerationBounds<int> DoorCountBounds
    {
        get => doorCountBounds;
        set => doorCountBounds = value;
    }

    public Bounds<float> XRoomBounds
    {
        get => xRoomBounds;
        set => xRoomBounds = value;
    }
    public Bounds<float> ZRoomBounds
    {
        get => zRoomBounds;
        set => zRoomBounds = value;
    }
    public Bounds<float> XMapBounds
    {
        get => xMapBounds;
        set => xMapBounds = value;
    }
    public Bounds<float> ZMapBounds
    {
        get => zMapBounds;
        set => zMapBounds = value;
    }
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
        roomCountBounds = new GenerationBounds<int>(5, new Bounds<int>(5, 10));
        doorCountBounds = new GenerationBounds<int>(2, new Bounds<int>(2, 4));
        xRoomBounds = new Bounds<float>(5, 10);
        zRoomBounds = new Bounds<float>(5, 10);
        xMapBounds = new Bounds<float>(-10, +10);
        zMapBounds = new Bounds<float>(-10, +10);
    }

    public override string ToString()
    {
        return $"Save: Version = {version}, Seed = {seed}, FilePath = {filepath}, " +
               $"RoomCountBounds = {roomCountBounds.ToString()}, WidthBounds = {xRoomBounds.ToString()}";
    }
}
#endregion
#region RoomData
[System.Serializable]
public class RoomData : BaseEntityData
{
    private int lastUsedObjectID;
    private bool isStartRoom;

    [SerializeField] private Vector3 size;
    [SerializeField] private List<ObjectData> listObjects;

    public Vector3 Size => size;
    public List<ObjectData> Objects => listObjects;

    public bool IsStartRoom
    {
        get => isStartRoom;
        set => isStartRoom = value;
    }

    public RoomData(Vector3 size, Vector3 position, int id) : base(position, id)
    {
        this.size = size;
        this.listObjects = new List<ObjectData>();
        lastUsedObjectID = 0;
    }

    public void AddObject(Vector3 position, GameObject prefab)
    {
        ObjectData obj = new ObjectData(position, prefab, ++lastUsedObjectID);
        listObjects.Add(obj);
    }

    public override string ToString()
    {
        string s = $"\nRoom ID: {ID};\nSize: {Size};\nPosition: {Position};";
        s += "Objects: ";
        foreach (ObjectData obj in listObjects)
        {
            s += obj.ToString() + "\n";
        }
        return s;
    }
}
#endregion
#region DoorData
[System.Serializable]
public class DoorData : BaseEntityData
{
    private int linkedRoomID;
    private bool isOnWE;

    public int LinkedRoomID
    {
        get => linkedRoomID;
        set => linkedRoomID = value;
    }
    public bool IsOnWE
    {
        get => isOnWE;
        set => isOnWE = value;
    }

    public DoorData(Vector3 position, int linkedRoomID, int id) : base(position, id)
    {
        this.linkedRoomID = linkedRoomID;
    }
    public DoorData() : base(Vector3.zero, 0)
    {
        linkedRoomID = 0;
    }
    public override string ToString()
    {
        return base.ToString() + $"Linked Room ID: {linkedRoomID}";
    }
}
#endregion
#region ObjectData
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
#region BetterRandom
public class BetterRandom
{
    private readonly System.Random rnd;

    public BetterRandom(int seed)
    {
        rnd = new System.Random(seed);
    }

    public T Random<T>(T min, T max) where T : IComparable<T>
    {
        if (min is int)
            return (T)(object)rnd.Next((int)(object)min, (int)(object)max);
        else if (min is float)
            return (T)(object)((float)(object)min + (float)rnd.NextDouble() * ((float)(object)max - (float)(object)min));
        else if (min is double)
            return (T)(object)((double)(object)min + (double)rnd.NextDouble() * ((double)(object)max - (double)(object)min));
        else
            throw new ArgumentException("Type not supported");
    }

    public Vector3 RandomVector3<T>(Bounds<T> xBounds, Bounds<T> yBounds, Bounds<T> zBounds) where T : IComparable<T>
    {
        return new Vector3(
            (float)(object)Random(xBounds.Min, xBounds.Max),
            (float)(object)Random(yBounds.Min, yBounds.Max),
            (float)(object)Random(zBounds.Min, zBounds.Max)
        );
    }
    public Vector3 RandomVector3<T>(Bounds<T> xBounds, Bounds<T> zBounds) where T : IComparable<T>
    {
        return new Vector3(
            (float)(object)Random(xBounds.Min, xBounds.Max),
            0f,
            (float)(object)Random(zBounds.Min, zBounds.Max)
        );
    }
    public Vector3 RandomVector3(Bounds<float> xBounds, Bounds<float> zBounds)
    {
        return new Vector3(
            Random(xBounds.Min, xBounds.Max),
            0f,
            Random(zBounds.Min, zBounds.Max)
        );
    }
    public Vector3 RandomPositiveVector3(Vector3 xBounds, Vector3 zBounds)
    {
        return new Vector3(
            Random(xBounds.x, xBounds.y),
            0f,
            Random(zBounds.x, zBounds.y)
        );
    }
    public Vector3 RandomPositiveVector3(Bounds<float> xBounds, Bounds<float> zBounds)
    {
        return new Vector3(
            MathF.Abs(Random(xBounds.Min, xBounds.Max)),
            0f,
            MathF.Abs(Random(zBounds.Min, zBounds.Max))
        );
    }
    public Orientation RandomOrientation()
    {
        return (Orientation)Random(0, 4);
    }
    public Vector3 RandomPointOnWall(Vector3 start, Vector3 end)
    {
        if(end.x < start.x) return new Vector3(Random(end.x, start.x),0,Random(end.z, start.z));        
        return new Vector3(Random(start.x, end.x),0,Random(start.z, end.z));
    }
}
#endregion
#region GenerationBounds
[System.Serializable]
public class GenerationBounds<T> where T : IComparable<T>
{
    private bool shouldUseDefaultValue;
    private bool wasValueAssigned;
    private T value;
    private T defaultValue;
    private Bounds<T> extremesBounds;

    public bool ShouldUseDefaultValue
    {
        get => shouldUseDefaultValue;
        set => shouldUseDefaultValue = value;
    }
    public T Value
    {
        get => value;
        set
        {
            this.value = value;
            wasValueAssigned = true;
        }
    }
    public T DefaultValue
    {
        get => defaultValue;
        set => defaultValue = value;
    }
    public Bounds<T> ExtremesBounds
    {
        get => extremesBounds;
        set => extremesBounds = value;
    }

    public GenerationBounds(T defaultValue, Bounds<T> extremes)
    {
        wasValueAssigned = false;
        shouldUseDefaultValue = false;
        this.defaultValue = defaultValue;
        this.extremesBounds = extremes;
        this.value = defaultValue;
    }
    public GenerationBounds(T defaultValue)
    {
        wasValueAssigned = false;
        shouldUseDefaultValue = false;
        this.defaultValue = defaultValue;
        this.extremesBounds = new Bounds<T>(default, default);
        this.value = defaultValue;
    }
    public GenerationBounds()
    {
        wasValueAssigned = false;
        shouldUseDefaultValue = false;
        defaultValue = default;
        extremesBounds = new Bounds<T>(default, default);
        value = default;
    }

    public void Generate(BetterRandom rnd)
    {
        if (shouldUseDefaultValue)
        {
            value = defaultValue;
        }
        else if (!wasValueAssigned)
        {
            if (defaultValue is Vector3)
                value = (T)(object)rnd.RandomVector3(extremesBounds, extremesBounds);
            else
                value = rnd.Random(extremesBounds.Min, extremesBounds.Max);
        }
    }
    public override string ToString() =>
            $"GenerationBounds: ShouldGenerate={shouldUseDefaultValue}, Value={value}, Default={defaultValue}, Bounds={extremesBounds.ToString()}";
}
#endregion
#region Bounds
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
#region Base Entity Data
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

#region Wall
public enum
Orientation
{
    N, E, S, W
}
[System.Serializable]
public class Wall
{
    [SerializeField]
    private
    Orientation orientation;
    [SerializeField] private Vector3 start;
    [SerializeField] private Vector3 end;

    public
    Orientation Orientation
    {
        get => orientation;
        set => orientation = value;
    }
    public Vector3 Start
    {
        get => start;
        set => start = value;
    }

    public Vector3 End
    {
        get => end;
        set => end = value;
    }

    public Wall(Vector3 start, Vector3 end,
    Orientation orientation)
    {
        this.start = start;
        this.end = end;
        this.orientation = orientation;
    }

    public Wall(float startX, float startZ, float endX, float endZ,
    Orientation orientation)
    {
        start = new Vector3(startX, 0, startZ);
        end = new Vector3(endX, 0, endZ);
        this.orientation = orientation;
    }
}
#endregion
#region RectangleF
[System.Serializable]
public class RectangleF
{
    [SerializeField] private Vector2 position;
    [SerializeField] private Vector2 size;

    public Vector2 Position => position;
    public Vector2 Size => size;

    public RectangleF(Vector2 position, Vector2 size)
    {
        this.position = position;
        this.size = size;
    }
    public RectangleF(Vector3 position3D, Vector3 size3D)
    {
        position = new Vector2(position3D.x, position3D.z); // Use X/Z
        size = new Vector2(Mathf.Abs(size3D.x), Mathf.Abs(size3D.z));
    }

    public bool Overlaps(RectangleF other, float padding = 1f)
    {
        return (position.x - padding) < (other.position.x + other.size.x + padding) &&
               (position.x + size.x + padding) > (other.position.x - padding) &&
               (position.y - padding) < (other.position.y + other.size.y + padding) &&
               (position.y + size.y + padding) > (other.position.y - padding);
    }
}
#endregion
