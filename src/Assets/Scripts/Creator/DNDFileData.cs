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
    [SerializeField] private int lastUsedDoorID;
    [SerializeField] private int lastUsedObjectID;
    [SerializeField] private List<RoomData> rooms;
    [SerializeField] private List<DoorConection> doors;
    [SerializeField] private List<ObjectData> objects;
    [SerializeField] private Settings settings;
    [SerializeField] private Save save;

    public List<RoomData> Rooms
    {
        get => rooms;
        set => rooms = value;
    }    public List<DoorConection> Doors => doors;
    public List<ObjectData> Objects => objects;
    public Settings Settings => settings;
    public Save Save => save;

    public void AddConnection(Door door, Door teleportDoor)
    {
        door = new Door(door.Position, ++lastUsedDoorID, teleportDoor.Position, door.Orientation);
        teleportDoor = new Door(teleportDoor.Position, ++lastUsedDoorID, door.Position, teleportDoor.Orientation);
        doors.Add(new DoorConection(door, teleportDoor));
    }
    public void AddRoom(Vector3 size, Vector3 position)
    {
        RoomData room = new RoomData(size, position, ++lastUsedID);
        rooms.Add(room);
    }
    public void AddObject(Vector3 position, string name)
    {
        ObjectData obj = new ObjectData(position, ++lastUsedObjectID, name);
        objects.Add(obj);
    }

    public void Initialize()
    {
        lastUsedID = 0;
        lastUsedDoorID = 0;
        lastUsedObjectID = 0;
        rooms = new List<RoomData>();
        doors = new List<DoorConection>();
        objects = new List<ObjectData>();
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
    [SerializeField] private GenerationBounds<int> objectCountBounds;
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
    public GenerationBounds<int> ObjectCountBounds
    {
        get => objectCountBounds;
        set => objectCountBounds = value;
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
        objectCountBounds = new GenerationBounds<int>(5, new Bounds<int>(5, 10));
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
    private bool isStartRoom;

    [SerializeField] private Vector3 size;

    public Vector3 Size => size;

    public bool IsStartRoom
    {
        get => isStartRoom;
        set => isStartRoom = value;
    }

    public RoomData(Vector3 size, Vector3 position, int id) : base(position, id)
    {
        this.size = size;
    }

    public override string ToString()
    {
        return $"\nRoom ID: {ID};\nSize: {Size};\nPosition: {Position};";
    }
}
#endregion
#region Door
[System.Serializable]
public class DoorConection
{
    private Door door;
    private Door teleportDoor;

    public Door Door => door;
    public Door TeleportDoor => teleportDoor;

    public DoorConection(Door door, Door teleportDoor)
    {
        this.door = door;
        this.teleportDoor = teleportDoor;
    }
}
[System.Serializable]
public class Door : BaseEntityData
{
    private Orientation orientation;
    private Vector3 playerTeleportLocation;

    public Vector3 PlayerTeleportLocation
    {
        get => playerTeleportLocation;
        set => playerTeleportLocation = value;
    }
    public Orientation Orientation
    {
        get => orientation;
        set => orientation = value;
    }

    public Door(Vector3 position, int id, Vector3 playerTeleportLocation, Orientation orientation) : base(position, id)
    {
        this.playerTeleportLocation = playerTeleportLocation;
        this.orientation = orientation;
    }
    public Door() : base(Vector3.zero, 0)
    {
        playerTeleportLocation = Vector3.zero;
        orientation = Orientation.N;
    }
}
#endregion
#region ObjectData
[System.Serializable]
public class ObjectData : BaseEntityData
{
    [SerializeField] private string name;
    public string Name => name;
    public enum TypesOfObjects
    {
        Light, Furniture, Rubble, Wall, Decoration
    }
    public enum LightTypes
    {
        Torch, Candle1, Candle2, Candle3, Lantern, Fireplace
    }
    public enum FurnitureTypes
    {
        Bag, Barrel, Box, Bucket, Carpet, Firewood, Stool, Table1, Table2
    }
    public enum RubbleTypes
    {
        Big, Medium1, Medium2, Small1, Small2, Small3, Small4
    }
    public enum WallTypes
    {
        Painting, Axe
    }
    public enum DecorationTypes
    {
        Book1, Book2, Bottle1, Bottle2, Bottle3, Coin1, Coin2, Coin3, Cup1, Cup2, Flask1, Flask2, Flask3,
        Food1, Food2, Food3, Food4, Food5, Food6, Gem1, Gem2, Jug1, Jug2,
        Plate1, Plate2, Plate3, Plate4, Plate5, Urn, Vase1, Vase2, Vase3, Vial1, Vial2
    }


    public ObjectData(Vector3 position, int id, string name) : base(position, id)
    {
        this.name = name;
    }

    public override string ToString()
    {
        return base.ToString() + $"Name: {name}";
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
    public Vector3 RandomPositiveVector3(Bounds<float> xBounds, Bounds<float> zBounds)
    {
        return new Vector3(
            MathF.Abs(Random(xBounds.Min, xBounds.Max)),
            0f,
            MathF.Abs(Random(zBounds.Min, zBounds.Max))
        );
    }

    public T RandomEnum<T>() where T : Enum
    {
        T[] values = (T[])Enum.GetValues(typeof(T));
        return values[Random(0, values.Length)];
    }
    public Orientation RandomOrientation()
    {
        return (Orientation)Random(0, 4);
    }
public Vector3 RandomPointOnWall(Vector3 start, Vector3 end, Orientation orientation, float padding = 2)
{
    // Ensure correct min/max ordering
    float minX = Mathf.Min(start.x, end.x);
    float maxX = Mathf.Max(start.x, end.x);
    float minY = Mathf.Min(start.y, end.y);
    float maxY = Mathf.Max(start.y, end.y);
    float minZ = Mathf.Min(start.z, end.z);
    float maxZ = Mathf.Max(start.z, end.z);

    // Ensure valid range
    if (maxX - minX < padding * 2) padding = (maxX - minX) / 2;
    if (maxY - minY < padding * 2) padding = (maxY - minY) / 2;
    if (maxZ - minZ < padding * 2) padding = (maxZ - minZ) / 2;

    // Pick a random point
    float xElement = RandomElement(minX, maxX, padding);
    float yElement = RandomElement(minY, maxY, padding);
    float zElement = RandomElement(minZ, maxZ, padding);

    // Offset placement
    float offset = 0.3f;
    switch (orientation)
    {
        case Orientation.N: zElement = maxZ - offset; break;
        case Orientation.E: xElement = maxX - offset; break;
        case Orientation.S: zElement = minZ + offset; break;
        case Orientation.W: xElement = minX + offset; break;
    }

    return new Vector3(xElement, yElement, zElement);
}
    public Vector3 RandomPointInRoom(RoomData room, Cube cube)
    {
        RoomData adjustedRoom = new RoomData(new Vector3(room.Size.x, 2, room.Size.z), room.Position, room.ID);
        Vector3 cubeHalfSize = cube.Size / 2;

        float minX = adjustedRoom.Position.x + cubeHalfSize.x;
        float maxX = adjustedRoom.Position.x + adjustedRoom.Size.x - cubeHalfSize.x;
        float minY = adjustedRoom.Position.y + cubeHalfSize.y;
        float maxY = adjustedRoom.Position.y + adjustedRoom.Size.y - cubeHalfSize.y;
        float minZ = adjustedRoom.Position.z + cubeHalfSize.z;
        float maxZ = adjustedRoom.Position.z + adjustedRoom.Size.z - cubeHalfSize.z;

        if (minX > maxX) throw new ArgumentException("Cube too wide for room X-axis");
        if (minY > maxY) throw new ArgumentException("Cube too tall for room Y-axis");
        if (minZ > maxZ) throw new ArgumentException("Cube too deep for room Z-axis");

        return new Vector3(
            Random(minX, maxX),
            Random(minY, maxY),
            Random(minZ, maxZ)
        );
    }

    private float RandomElement(float a, float b, float padding)
    {
        if (b > a) return Random(a + padding, b - padding);
        if (a > b) return Random(b + padding, a - padding);
        return a;
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
    public Vector3 Position
    {
        get => position;
        set => position = value;
    }

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
#region Orientation
public enum Orientation
{
    N, E, S, W
}
#endregion
#region RectangleF
[System.Serializable]
public class RectangleF
{
    [SerializeField] private Vector2 position;
    [SerializeField] private Vector2 size;

    public Vector2 Position
    {
        get => position;
        set => position = value;
    }
    public Vector2 Size
    {
        get => size;
        set => size = value;
    }

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
#region Cube

[System.Serializable]
public class Cube
{
    private Vector3 position;
    private Vector3 size;

    public Vector3 Position
    {
        get => position;
        set => position = value;
    }
    public Vector3 Size
    {
        get => size;
        set => size = value;
    }

    public Cube(Vector3 position, Vector3 size)
    {
        this.position = position;
        this.size = size;
    }
    public Cube(Vector3 position, float size)
    {
        this.position = position;
        this.size = new Vector3(size, size, size);
    }

    public bool Intersects(Cube other)
    {
        return (position.x - size.x) < (other.position.x + other.size.x) &&
               (position.x + size.x) > (other.position.x - other.size.x) &&
               (position.y - size.y) < (other.position.y + other.size.y) &&
               (position.y + size.y) > (other.position.y - other.size.y) &&
               (position.z - size.z) < (other.position.z + other.size.z) &&
               (position.z + size.z) > (other.position.z - other.size.z);
    }

    public override string ToString()
    {
        return $"Cube: Position = {position}, Size = {size}";
    }
}
#endregion