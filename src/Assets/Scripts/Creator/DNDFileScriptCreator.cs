using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEditor.Playables;
using UnityEngine;

public class DNDFileScriptCreator : MonoBehaviour
{
    private List<RectangleF> rooms;
    private List<Cube> objects;
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

        door.Orientation = random.RandomOrientation();
        PointOfWalls(room, out Vector3 upperLeft, out Vector3 upperRight, out Vector3 lowerLeft, out Vector3 lowerRight);

        switch (door.Orientation)
        {
            case Orientation.N:
                door.Position = random.RandomPointOnWall(upperLeft, upperRight);
                break;
            case Orientation.S:
                door.Position = random.RandomPointOnWall(lowerLeft, lowerRight);
                break;
            case Orientation.E:
                door.Position = random.RandomPointOnWall(upperRight, lowerRight);
                break;
            case Orientation.W:
                door.Position = random.RandomPointOnWall(upperLeft, lowerLeft);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        int randomRoomIndex = random.Random(0, save.Rooms.Count);
        RoomData teleportRoom = save.Rooms[randomRoomIndex];
        teleportDoor.Orientation = random.RandomOrientation();
        PointOfWalls(teleportRoom, out Vector3 upperLeftTeleport, out Vector3 upperRightTeleport, out Vector3 lowerLeftTeleport, out Vector3 lowerRightTeleport);

        switch (teleportDoor.Orientation)
        {
            case Orientation.N:
                teleportDoor.Position = random.RandomPointOnWall(upperLeftTeleport, upperRightTeleport);
                break;
            case Orientation.S:
                teleportDoor.Position = random.RandomPointOnWall(lowerLeftTeleport, lowerRightTeleport);
                break;
            case Orientation.E:
                teleportDoor.Position = random.RandomPointOnWall(upperRightTeleport, lowerRightTeleport);
                break;
            case Orientation.W:
                teleportDoor.Position = random.RandomPointOnWall(upperLeftTeleport, lowerLeftTeleport);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    public void PlaceObjects(DNDFileData save, BetterRandom random, RoomData room)
    {
        for (int i = 0; i < save.Save.ObjectCountBounds.Value; i++)
        {
            ObjectData objectData;
            try
            {
                objectData = PlaceObject(save, random, room);
                save.AddObject(objectData.Position, objectData.Name);
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
                case ObjectData.TypesOfObjects.Light:
                    objectData = PlaceLight(save, random, room);
                    break;
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
        
        Vector3 size;
        switch (name)
        {
            case "Torch":
                size = new Vector3(0.15f, 0.7f, 0.11f);
                break;
            case "Vial":
                size = new Vector3(0.5f, 0.5f, 0.5f);
                break;
            case "Chest":
                size = new Vector3(1, 1, 1);
                break;
            case "Enemy":
                size = new Vector3(1, 1, 1);
                break;
            case "Wall":
                size = new Vector3(1, 1, 1);
                break;
            default:
                size = new Vector3(1, 1, 1);
                break;
        }

        return new Cube(Vector3.zero, size);
    }
    #endregion
    #region PlaceObjectTypes
    private ObjectData PlaceLight(DNDFileData save, BetterRandom random, RoomData room)
    {
        ObjectData.LightTypes lightType = random.RandomEnum<ObjectData.LightTypes>();
        Vector3 position;

        switch (lightType)
        {
            case ObjectData.LightTypes.Candle1:
                position = random.RandomPointInRoom(room, GetSize("Candle1"));
                break;
            case ObjectData.LightTypes.Candle2:
                position = random.RandomPointInRoom(room, GetSize("Candle2"));
                break;
            case ObjectData.LightTypes.Candle3:
                position = random.RandomPointInRoom(room, GetSize("Candle3"));
                break;
            case ObjectData.LightTypes.Torch:
                position = random.RandomPointInRoom(room, GetSize("Torch"));
                break;
            case ObjectData.LightTypes.Lantern:
                position = random.RandomPointInRoom(room, GetSize("Lantern"));
                break;
            case ObjectData.LightTypes.Fireplace:
                position = random.RandomPointInRoom(room, GetSize("Fireplace"));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        string name = lightType.ToString();
        return new ObjectData(position, -1, name);
    }
    private ObjectData PlaceFurniture(DNDFileData save, BetterRandom random, RoomData room)
    {
        ObjectData.FurnitureTypes furnitureType = random.RandomEnum<ObjectData.FurnitureTypes>();
        Vector3 position;

        switch (furnitureType)
        {
            case ObjectData.FurnitureTypes.Bag:
                position = random.RandomPointInRoom(room, GetSize("Bag"));
                break;
            case ObjectData.FurnitureTypes.Barrel:
                position = random.RandomPointInRoom(room, GetSize("Barrel"));
                break;
            case ObjectData.FurnitureTypes.Box:
                position = random.RandomPointInRoom(room, GetSize("Box"));
                break;
            case ObjectData.FurnitureTypes.Bucket:
                position = random.RandomPointInRoom(room, GetSize("Bucket"));
                break;
            case ObjectData.FurnitureTypes.Carpet:
                position = random.RandomPointInRoom(room, GetSize("Carpet"));
                break;
            case ObjectData.FurnitureTypes.Firewood:
                position = random.RandomPointInRoom(room, GetSize("Firewood"));
                break;
            case ObjectData.FurnitureTypes.Stool:
                position = random.RandomPointInRoom(room, GetSize("Stool"));
                break;
            case ObjectData.FurnitureTypes.Table1:
                position = random.RandomPointInRoom(room, GetSize("Table1"));
                break;
            case ObjectData.FurnitureTypes.Table2:
                position = random.RandomPointInRoom(room, GetSize("Table2"));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        string name = furnitureType.ToString();
        return new ObjectData(position, -1, name);
    }
    private ObjectData PlaceRubble(DNDFileData save, BetterRandom random, RoomData room)
    {
        ObjectData.RubbleTypes rubbleType = random.RandomEnum<ObjectData.RubbleTypes>();
        Vector3 position;

        switch (rubbleType)
        {
            case ObjectData.RubbleTypes.Big:
                position = random.RandomPointInRoom(room, GetSize("Big"));
                break;;
            case ObjectData.RubbleTypes.Medium1:
                position = random.RandomPointInRoom(room, GetSize("Medium1"));
                break;
            case ObjectData.RubbleTypes.Medium2:
                position = random.RandomPointInRoom(room, GetSize("Medium2"));
                break;
            case ObjectData.RubbleTypes.Small1:
                position = random.RandomPointInRoom(room, GetSize("Small1"));
                break;
            case ObjectData.RubbleTypes.Small2:
                position = random.RandomPointInRoom(room, GetSize("Small2"));
                break;
            case ObjectData.RubbleTypes.Small3:
                position = random.RandomPointInRoom(room, GetSize("Small3"));
                break;
            case ObjectData.RubbleTypes.Small4:
                position = random.RandomPointInRoom(room, GetSize("Small4"));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        string name = rubbleType.ToString();
        return new ObjectData(position, -1, name);
    }
    private ObjectData PlaceWall(DNDFileData save, BetterRandom random, RoomData room)
    {
        ObjectData.WallTypes wallType = random.RandomEnum<ObjectData.WallTypes>();
        Vector3 position;

        switch (wallType)
        {
            case ObjectData.WallTypes.Axe:
                position = random.RandomPointInRoom(room, GetSize("Axe"));
                break;
            case ObjectData.WallTypes.Painting:
                position = random.RandomPointInRoom(room, GetSize("Painting"));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        string name = wallType.ToString();
        return new ObjectData(position, -1, name);
    }
    private ObjectData PlaceDecoration(DNDFileData save, BetterRandom random, RoomData room)
    {
        ObjectData.DecorationTypes decorationType = random.RandomEnum<ObjectData.DecorationTypes>();
        Vector3 position;

        switch (decorationType)
        {
            case ObjectData.DecorationTypes.Book:
                position = random.RandomPointInRoom(room, GetSize("Book"));
                break;
            case ObjectData.DecorationTypes.Bottle1:
                position = random.RandomPointInRoom(room, GetSize("Bottle1"));
                break;
            case ObjectData.DecorationTypes.Bottle2:
                position = random.RandomPointInRoom(room, GetSize("Bottle2"));
                break;
            case ObjectData.DecorationTypes.Bottle3:
                position = random.RandomPointInRoom(room, GetSize("Bottle3"));
                break;
            case ObjectData.DecorationTypes.Coin1:
                position = random.RandomPointInRoom(room, GetSize("Coin1"));
                break;
            case ObjectData.DecorationTypes.Coin2:
                position = random.RandomPointInRoom(room, GetSize("Coin2"));
                break;
            case ObjectData.DecorationTypes.Coin3:
                position = random.RandomPointInRoom(room, GetSize("Coin3"));
                break;
            case ObjectData.DecorationTypes.Cup1:
                position = random.RandomPointInRoom(room, GetSize("Cup1"));
                break;
            case ObjectData.DecorationTypes.Cup2:
                position = random.RandomPointInRoom(room, GetSize("Cup2"));
                break;
            case ObjectData.DecorationTypes.Flask1:
                position = random.RandomPointInRoom(room, GetSize("Flask1"));
                break;
            case ObjectData.DecorationTypes.Flask2:
                position = random.RandomPointInRoom(room, GetSize("Flask2"));
                break;
            case ObjectData.DecorationTypes.Flask3:
                position = random.RandomPointInRoom(room, GetSize("Flask3"));
                break;
            case ObjectData.DecorationTypes.Food1:
                position = random.RandomPointInRoom(room, GetSize("Food1"));
                break;
            case ObjectData.DecorationTypes.Food2:
                position = random.RandomPointInRoom(room, GetSize("Food2"));
                break;
            case ObjectData.DecorationTypes.Food3:
                position = random.RandomPointInRoom(room, GetSize("Food3"));
                break;
            case ObjectData.DecorationTypes.Food4:
                position = random.RandomPointInRoom(room, GetSize("Food4"));
                break;
            case ObjectData.DecorationTypes.Food5:
                position = random.RandomPointInRoom(room, GetSize("Food5"));
                break;
            case ObjectData.DecorationTypes.Food6:
                position = random.RandomPointInRoom(room, GetSize("Food6"));
                break;
            case ObjectData.DecorationTypes.Gem1:
                position = random.RandomPointInRoom(room, GetSize("Gem1"));
                break;
            case ObjectData.DecorationTypes.Gem2:
                position = random.RandomPointInRoom(room, GetSize("Gem2"));
                break;
            case ObjectData.DecorationTypes.Jug1:
                position = random.RandomPointInRoom(room, GetSize("Jug1"));
                break;
            case ObjectData.DecorationTypes.Jug2:
                position = random.RandomPointInRoom(room, GetSize("Jug2"));
                break;
            case ObjectData.DecorationTypes.Plate1:
                position = random.RandomPointInRoom(room, GetSize("Plate1"));
                break;
            case ObjectData.DecorationTypes.Plate2:
                position = random.RandomPointInRoom(room, GetSize("Plate2"));
                break;
            case ObjectData.DecorationTypes.Plate3:
                position = random.RandomPointInRoom(room, GetSize("Plate3"));
                break;
            case ObjectData.DecorationTypes.Plate4:
                position = random.RandomPointInRoom(room, GetSize("Plate4"));
                break;
            case ObjectData.DecorationTypes.Plate5:
                position = random.RandomPointInRoom(room, GetSize("Plate5"));
                break;
            case ObjectData.DecorationTypes.Urn:
                position = random.RandomPointInRoom(room, GetSize("Urn"));
                break;
            case ObjectData.DecorationTypes.Vase1:
                position = random.RandomPointInRoom(room, GetSize("Vase1"));
                break;
            case ObjectData.DecorationTypes.Vase2:
                position = random.RandomPointInRoom(room, GetSize("Vase2"));
                break;
            case ObjectData.DecorationTypes.Vase3:
                position = random.RandomPointInRoom(room, GetSize("Vase3"));
                break;
            case ObjectData.DecorationTypes.Vial1:
                position = random.RandomPointInRoom(room, GetSize("Vial1"));
                break;
            case ObjectData.DecorationTypes.Vial2:
                position = random.RandomPointInRoom(room, GetSize("Vial2"));
                break;
            default:
                throw new ArgumentOutOfRangeException();

        }

        string name = decorationType.ToString();
        return new ObjectData(position, -1, name);
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

            writer.WriteStartElement("Walls");
            foreach (Wall wall in fileData.Walls)
            {
                writer.WriteStartElement("Wall");

                WriteVector3(writer, wall.Start, "Start");
                WriteVector3(writer, wall.End, "End");
                writer.WriteElementString("Orientation", wall.Orientation.ToString());

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
