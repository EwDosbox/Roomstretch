using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;

public class DNDFileScriptCreator : MonoBehaviour
{
    private List<RectangleF> rooms;
    private List<Cube> objects;
    private Dictionary<string, GameObject> Prefabs;

    #region LoadResources
    private void LoadPrefabs()
    {
        List<GameObject> resources = Resources.LoadAll<GameObject>("Prefabs").ToList();
        Prefabs = new Dictionary<string, GameObject>();

        foreach (GameObject resource in resources)
        {
            Prefabs.Add(resource.name, resource);
        }

        Debug.Log("Creator: Imported: " + Prefabs.ToSeparatedString("; "));
    }
    #endregion
    #region PrepareSave
    public void PrepareSave(DNDFileData save)
    {
        BetterRandom random = save.Save.Random;

        save.Save.RoomsCountBounds.Generate(random);
        save.Save.ObjectCountBounds.Generate(random);

        rooms = new List<RectangleF>();
        objects = new List<Cube>();

        for (int i = 0; i < save.Save.RoomsCountBounds.Value; i++)
        {
            RectangleF room;
            try
            {
                room = PlaceRoom(rooms, save, random);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                continue;
            }
            rooms.Add(room);
            Vector3 roomPosition = new Vector3(room.Position.x, 0, room.Position.y);
            Vector3 roomSize = new Vector3(room.Size.x, 0, room.Size.y);

            RoomData roomData = new RoomData(roomSize, roomPosition, i);
            save.Rooms.Add(roomData);
        }

        save.Save.RoomsCountBounds.Value = save.Rooms.Count;
        save.Rooms[random.Random(0, save.Rooms.Count)].IsStartRoom = true;

        foreach (RoomData room in save.Rooms)
        {
            PlaceDoor(room, save, random, out Door door, out Door doorTeleport);
            save.AddConnection(door, doorTeleport);
            LoadDoorIntoObjects(door, doorTeleport);
            PlaceObjects(save, random, room);
        }
    }
    #endregion
    #region Helping Placement Methods
    #region Place Big
    private RectangleF PlaceRoom(List<RectangleF> rooms, DNDFileData save, BetterRandom random)
    {
        Vector3 roomPosition, roomSize;
        RectangleF room;
        int attempts = 0, maxAttempts = 100;
        bool placed;

        do
        {
            placed = false;

            roomPosition = random.RandomVector3(save.Save.XMapBounds, save.Save.ZMapBounds);
            roomSize = random.RandomPositiveVector3(save.Save.XRoomBounds, save.Save.ZRoomBounds);

            room = new RectangleF(roomPosition, roomSize);
            attempts++;

            placed = !rooms.Any(r => room.Overlaps(r));

        } while (!placed && attempts < maxAttempts);

        if (attempts >= maxAttempts) throw new Exception($"Failed to place room after {attempts} attempts");

        return room;
    }
    public void PlaceDoor(RoomData room, DNDFileData save, BetterRandom random, out Door door, out Door teleportDoor)
    {
        door = new Door();
        teleportDoor = new Door();

        int attempts = 0, maxAttempts = 100;
        bool isPlaced = false;

        do
        {
            door.Orientation = random.RandomOrientation();
            PointOfWalls(room, out Vector3 upperLeft, out Vector3 upperRight, out Vector3 lowerLeft, out Vector3 lowerRight);

            switch (door.Orientation)
            {
                case Orientation.N:
                    door.Position = random.RandomPointOnWall(upperLeft, upperRight, door.Orientation);
                    break;
                case Orientation.S:
                    door.Position = random.RandomPointOnWall(lowerLeft, lowerRight, door.Orientation);
                    break;
                case Orientation.E:
                    door.Position = random.RandomPointOnWall(upperRight, lowerRight, door.Orientation);
                    break;
                case Orientation.W:
                    door.Position = random.RandomPointOnWall(upperLeft, lowerLeft, door.Orientation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            isPlaced = IsValidDoor(door, save.Doors);
            attempts++;
        } while (!isPlaced && attempts < maxAttempts);

        if (attempts >= maxAttempts) throw new Exception($"Failed to place door after {attempts} attempts");

        attempts = 0;
        isPlaced = false;
        do
        {
            int randomRoomIndex;
            do
            {
                randomRoomIndex = random.Random(0, save.Rooms.Count);
            } while (randomRoomIndex == room.ID);

            RoomData teleportRoom = save.Rooms[randomRoomIndex];
            teleportDoor.Orientation = random.RandomOrientation();
            PointOfWalls(teleportRoom, out Vector3 upperLeftTeleport, out Vector3 upperRightTeleport, out Vector3 lowerLeftTeleport, out Vector3 lowerRightTeleport);

            switch (teleportDoor.Orientation)
            {
                case Orientation.N:
                    teleportDoor.Position = random.RandomPointOnWall(upperLeftTeleport, upperRightTeleport, teleportDoor.Orientation);
                    break;
                case Orientation.S:
                    teleportDoor.Position = random.RandomPointOnWall(lowerLeftTeleport, lowerRightTeleport, teleportDoor.Orientation);
                    break;
                case Orientation.E:
                    teleportDoor.Position = random.RandomPointOnWall(upperRightTeleport, lowerRightTeleport, teleportDoor.Orientation);
                    break;
                case Orientation.W:
                    teleportDoor.Position = random.RandomPointOnWall(upperLeftTeleport, lowerLeftTeleport, teleportDoor.Orientation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            isPlaced = IsValidDoor(teleportDoor, save.Doors);
            attempts++;
        } while (!isPlaced && attempts < maxAttempts);

        if (attempts >= maxAttempts) throw new Exception($"Failed to place teleport door after {attempts} attempts");
    }

    private bool IsValidDoor(Door newDoor, List<DoorConection> existingDoors)
    {
        foreach (DoorConection doorConection in existingDoors)
        {
            foreach (Door door in new Door[] { doorConection.Door, doorConection.TeleportDoor })
            {

                if (Vector3.Distance(newDoor.Position, door.Position) < 2.0f)
                {
                    return false;
                }
            }
        }
        return true;
    }
    public void PlaceObjects(DNDFileData save, BetterRandom random, RoomData room)
    {
        for (int i = 0; i < save.Save.ObjectCountBounds.Value; i++)
        {
            ObjectData objectData;
            try
            {
                objectData = PlaceObject(save, random, room);
                save.AddObject(objectData.Position, objectData.Name, objectData.Type, objectData.Orientation);
                objects.Add(GetCube(objectData));
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                continue;
            }
        }
    }
    #endregion
    #region LoadDoorIntoObjects
    private void LoadDoorIntoObjects(Door door, Door teleportDoor)
    {
        Cube doorCube = GetSize("Door");
        objects.Add(new Cube(door.Position, doorCube.Size));
        objects.Add(new Cube(teleportDoor.Position, doorCube.Size));
    }
    #endregion
    private ObjectData PlaceObject(DNDFileData save, BetterRandom random, RoomData room)
    {
        ObjectData objectData;
        bool isPlaced;
        int attempts = 0, maxAttempts = 100;
        do
        {
            ObjectData.TypesOfObjects typesOfObject = random.RandomEnum<ObjectData.TypesOfObjects>();

            switch (typesOfObject)
            {
                case ObjectData.TypesOfObjects.Furniture:
                    objectData = PlaceFurniture(save, random, room);
                    break;
                case ObjectData.TypesOfObjects.Rubble:
                    objectData = PlaceRubble(save, random, room);
                    break;
                case ObjectData.TypesOfObjects.Wall:
                    objectData = PlaceWall(save, random, room);
                    break;
                case ObjectData.TypesOfObjects.Decoration:
                    objectData = PlaceDecoration(save, random, room);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            attempts++;
            isPlaced = IsValid(objectData, objects);
        } while (!isPlaced && attempts < maxAttempts);

        if (attempts >= maxAttempts) throw new Exception($"Failed to place object after {attempts} attempts");

        return objectData;
    }
    #region Helping PlaceObjectMethods

    private bool IsValid(ObjectData objectData, List<Cube> cubes)
    {
        Cube cube = GetCube(objectData);
        return !cubes.Any(c => c.Intersects(cube));
    }
    private Cube GetCube(ObjectData objectData)
    {
        Cube size = GetSize(objectData.Name);
        return new Cube(objectData.Position, size.Size);
    }
    private Cube GetSize(string name)
    {
        if (Prefabs == null) LoadPrefabs();

        if (Prefabs.TryGetValue(name, out GameObject prefab) && prefab.TryGetComponent(out BoxCollider collider))
        {
            return new Cube(Vector3.zero, collider.size);
        }

        Debug.LogWarning($"Prefab '{name}' not found or missing BoxCollider.");
        return new Cube(Vector3.zero, Vector3.one);
    }
    #endregion
    #region PlaceObjectTypes
    private ObjectData PlaceFurniture(DNDFileData save, BetterRandom random, RoomData room)
    {
        ObjectData.FurnitureTypes furnitureType = random.RandomEnum<ObjectData.FurnitureTypes>();

        Vector3 position = random.RandomPointInRoom(room, GetSize(furnitureType.ToString()));
        string name = furnitureType.ToString();
        Orientation orientation = random.RandomOrientation();

        return new ObjectData(position, ObjectData.TypesOfObjects.Furniture, orientation, -1, name);
    }
    private ObjectData PlaceRubble(DNDFileData save, BetterRandom random, RoomData room)
    {
        ObjectData.RubbleTypes rubbleType = random.RandomEnum<ObjectData.RubbleTypes>();

        Vector3 position = random.RandomPointInRoom(room, GetSize(rubbleType.ToString()));
        string name = rubbleType.ToString();
        Orientation orientation = random.RandomOrientation();

        return new ObjectData(position, ObjectData.TypesOfObjects.Rubble, orientation, -1, name);
    }
    private ObjectData PlaceWall(DNDFileData save, BetterRandom random, RoomData room)
    {
        ObjectData.WallTypes wallType = random.RandomEnum<ObjectData.WallTypes>();

        Vector3 position = random.RandomPointOnWall(room, GetSize(wallType.ToString()), out Orientation orientation);
        string name = wallType.ToString();

        return new ObjectData(position, ObjectData.TypesOfObjects.Wall, orientation, -1, name);
    }
    private ObjectData PlaceDecoration(DNDFileData save, BetterRandom random, RoomData room)
    {
        ObjectData.DecorationTypes decorationType = random.RandomEnum<ObjectData.DecorationTypes>();

        Vector3 position = random.RandomPointInRoom(room, GetSize(decorationType.ToString()));
        string name = decorationType.ToString();
        Orientation orientation = random.RandomOrientation();

        return new ObjectData(position, ObjectData.TypesOfObjects.Decoration, orientation, -1, name);
    }
    #endregion
    #endregion
    #region Walls
    private static void PointOfWalls(RoomData room, out Vector3 upperLeft, out Vector3 upperRight, out Vector3 lowerLeft, out Vector3 lowerRight)
    {
        upperLeft = new(room.Position.x, 0, room.Position.z + room.Size.z);
        upperRight = new(room.Position.x + room.Size.x, 0, room.Position.z + room.Size.z);
        lowerLeft = new(room.Position.x, 0, room.Position.z);
        lowerRight = new(room.Position.x + room.Size.x, 0, room.Position.z);
    }
    #endregion
    #region CreateFile
    public void CreateFile(DNDFileData fileData)
    {
        XmlWriterSettings settings = new()
        {
            Indent = true,
            NewLineOnAttributes = false

        };

        using (XmlWriter writer = XmlWriter.Create(fileData.Save.FilePath, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("RoomStretch");
            #region Head
            writer.WriteStartElement("Head");

            writer.WriteStartElement("Save");

            writer.WriteElementString("Version", fileData.Save.Version);
            writer.WriteElementString("Seed", fileData.Save.Seed);
            writer.WriteElementString("FilePath", fileData.Save.FilePath);
            WriteGenerationBounds(writer, fileData.Save.RoomsCountBounds, "RoomsCountBounds");
            WriteGenerationBounds(writer, fileData.Save.ObjectCountBounds, "ObjectCountBounds");

            WriteBounds(writer, fileData.Save.XRoomBounds, "XRoomBounds");
            WriteBounds(writer, fileData.Save.ZRoomBounds, "ZRoomBounds");
            WriteBounds(writer, fileData.Save.XMapBounds, "XMapBounds");
            WriteBounds(writer, fileData.Save.ZMapBounds, "ZMapBounds");

            writer.WriteEndElement();
            writer.WriteStartElement("Settings");

            writer.WriteElementString("FOV", fileData.Settings.FOV.ToString());
            writer.WriteElementString("Sensitivity", fileData.Settings.Sensitivity.ToString());

            writer.WriteEndElement();

            writer.WriteEndElement();
            #endregion
            #region Body
            writer.WriteStartElement("Body");

            writer.WriteStartElement("Rooms");

            foreach (RoomData roomData in fileData.Rooms)
            {
                writer.WriteStartElement("Room");

                writer.WriteElementString("ID", roomData.ID.ToString());
                writer.WriteElementString("IsStartRoom", roomData.IsStartRoom.ToString());

                WriteVector3(writer, roomData.Position, "Position");
                WriteVector3(writer, roomData.Size, "Size");

                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteStartElement("Doors");
            foreach (DoorConection door in fileData.Doors)
            {
                writer.WriteStartElement("DoorConection");
                writer.WriteStartElement("Door");

                writer.WriteElementString("ID", door.Door.ID.ToString());
                WriteVector3(writer, door.Door.Position, "Position");
                WriteVector3(writer, door.Door.PlayerTeleportLocation, "PlayerTeleportLocation");
                writer.WriteElementString("Orientation", door.Door.Orientation.ToString());

                writer.WriteEndElement();
                writer.WriteStartElement("TeleportDoor");

                writer.WriteElementString("ID", door.TeleportDoor.ID.ToString());
                WriteVector3(writer, door.TeleportDoor.Position, "Position");
                WriteVector3(writer, door.TeleportDoor.PlayerTeleportLocation, "PlayerTeleportLocation");
                writer.WriteElementString("Orientation", door.TeleportDoor.Orientation.ToString());

                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Objects");
            foreach (ObjectData objectData in fileData.Objects)
            {
                writer.WriteStartElement("Object");

                writer.WriteElementString("ID", objectData.ID.ToString());
                WriteVector3(writer, objectData.Position, "Position");
                writer.WriteElementString("ObjectName", objectData.Name);
                writer.WriteElementString("Type", objectData.Type.ToString());
                writer.WriteElementString("Orientation", objectData.Orientation.ToString());

                writer.WriteEndElement();
            }
            #endregion
            writer.WriteEndElement();

            writer.WriteEndDocument();
        }
        Debug.Log("File: Temp.dnd created at: " + fileData.Save.FilePath);
    }
    #endregion
    #region WriteGenerationBounds
    private void WriteGenerationBounds<T>(XmlWriter writer, GenerationBounds<T> bounds, string name) where T : IComparable<T>
    {
        writer.WriteStartElement(name);

        writer.WriteElementString("ShouldUseDefaultValue", bounds.ShouldUseDefaultValue.ToString());
        writer.WriteElementString("Value", bounds.Value.ToString());
        writer.WriteElementString("DefaultValue", bounds.DefaultValue.ToString());

        WriteBounds(writer, bounds.ExtremesBounds, "ExtremesBounds");

        writer.WriteEndElement();
    }
    #endregion
    #region WriteBound
    private void WriteBounds<T>(XmlWriter writer, Bounds<T> bound, string name) where T : IComparable<T>
    {
        writer.WriteStartElement(name);

        writer.WriteElementString("Min", bound.Min.ToString());
        writer.WriteElementString("Max", bound.Max.ToString());

        writer.WriteEndElement();
    }
    #endregion
    #region WritePosition
    private void WriteVector3(XmlWriter writer, Vector3 position, string name)
    {
        writer.WriteStartElement(name);

        writer.WriteElementString("X", position.x.ToString());
        writer.WriteElementString("Y", position.y.ToString());
        writer.WriteElementString("Z", position.z.ToString());

        writer.WriteEndElement();
    }
    #endregion
}
