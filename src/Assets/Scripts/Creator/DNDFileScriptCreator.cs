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
        }

        int NoOfObjectsInScene = save.Save.ObjectCountBounds.Value * save.Rooms.Count;
        for (int i = 0; i < NoOfObjectsInScene; i++)
        {
            ObjectData objectData = PlaceObject(save, random);
            save.Objects.Add(objectData);
        }
    }
    #endregion
    #region Helping Placement Methods
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
    public ObjectData PlaceObject(DNDFileData save, BetterRandom random)
    {
        ObjectData objectData;
        bool isPlaced;
        int attempts = 0, maxAttempts = 100;
        do
        {
            ObjectData.TypesOfObjects typesOfObject = random.RandomTypeOfObject();

            switch (typesOfObject)
            {
                case ObjectData.TypesOfObjects.Light:
                    objectData = PlaceLight(save, random);
                    break;
                case ObjectData.TypesOfObjects.Furniture:
                    objectData = PlaceFurniture(save, random);
                    break;
                case ObjectData.TypesOfObjects.Rubble:
                    objectData = PlaceRubble(save, random);
                    break;
                case ObjectData.TypesOfObjects.Wall:
                    objectData = PlaceWall(save, random);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            attempts++;
            isPlaced = IsValid(objectData, objects);
        } while (!isPlaced && attempts < maxAttempts);

        if (attempts >= maxAttempts) throw new Exception($"Failed to place object after {attempts} attempts");

        if(isPlaced) objects.Add(GetSize(objectData));

        return objectData;
    }
    #region Helping PlaceObjectMethods

    private bool IsValid(ObjectData objectData, List<Cube> cubes)
    {
        Cube cube = GetSize(objectData);
        return !cubes.Any(c => c.Intersects(cube));
    }
    private Cube GetSize(ObjectData objectData)
    {
        switch (objectData.Name)
        {
            case "Vial":
                return new Cube(new Vector3(0.5f, 0.5f, 0.5f), objectData.Position);
            case "Chest":
                return new Cube(new Vector3(1, 1, 1), objectData.Position);
            case "Enemy":
                return new Cube(new Vector3(1, 1, 1), objectData.Position);
            case "Wall":
                return new Cube(new Vector3(1, 1, 1), objectData.Position);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    #endregion
    #region PlaceObjectTypes
    private ObjectData PlaceLight(DNDFileData save, BetterRandom random)
    {
        Vector3 position = random.RandomVector3(save.Save.XMapBounds, save.Save.ZMapBounds);
        return new ObjectData(position, "Light", ObjectData.TypesOfObjects.Light);
    }
    private ObjectData PlaceFurniture(DNDFileData save, BetterRandom random)
    {
        Vector3 position = random.RandomVector3(save.Save.XMapBounds, save.Save.ZMapBounds);
        return new ObjectData(position, "Chest", ObjectData.TypesOfObjects.Furniture);
    }
    private ObjectData PlaceRubble(DNDFileData save, BetterRandom random)
    {
        Vector3 position = random.RandomVector3(save.Save.XMapBounds, save.Save.ZMapBounds);
        return new ObjectData(position, "Enemy", ObjectData.TypesOfObjects.Rubble);
    }
    private ObjectData PlaceWall(DNDFileData save, BetterRandom random)
    {
        Vector3 position = random.RandomVector3(save.Save.XMapBounds, save.Save.ZMapBounds);
        return new ObjectData(position, "Wall", ObjectData.TypesOfObjects.Wall);
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
