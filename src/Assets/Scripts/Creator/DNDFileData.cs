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
    public Save Save => save;

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

    public void Initialize()
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
    private int lastUsedDoorID;
    private int lastUsedObjectID;

    [SerializeField] private Vector3 size;
    [SerializeField] private DoorData door;
    [SerializeField] private List<ObjectData> listObjects;

    public Vector3 Size => size;
    public DoorData Door
    {
        get => door;
        set => door = value;
    }
    public List<ObjectData> Objects => listObjects;

    public RoomData(Vector3 size, Vector3 position, int id) : base(position, id)
    {
        this.size = size;
        this.door = new DoorData();
        this.listObjects = new List<ObjectData>();
        lastUsedDoorID = 0;
        lastUsedObjectID = 0;
    }

    public void AddDoor(Vector3 position, int linkedRoomID)
    {
        this.door = new DoorData(position, linkedRoomID, ++lastUsedDoorID);
    }

    public void AddObject(Vector3 position, GameObject prefab)
    {
        ObjectData obj = new ObjectData(position, prefab, ++lastUsedObjectID);
        listObjects.Add(obj);
    }

    public override string ToString()
    {
        string s = $"\nRoom ID: {ID};\nSize: {Size};\nPosition: {Position};\nDoors: {Door}\nObjects: ";
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

    public int LinkedRoomID => linkedRoomID;

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

    private void ValidateRange(float min, float max)
    {
        if (min > max) throw new ArgumentException("Min must be <= Max");
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
