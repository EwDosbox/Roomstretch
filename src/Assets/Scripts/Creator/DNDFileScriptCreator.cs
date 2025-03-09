using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Unity.VisualScripting;
using UnityEditor.Playables;
using UnityEngine;

public class DNDFileScriptCreator : MonoBehaviour
{
    private List<RectangleF> rooms;


    #region PrepareSave
    public void PrepareSave(DNDFileData save)
    {
        BetterRandom random = save.Save.Random;

        save.Save.RoomsCountBounds.Generate(random);
        save.Save.DoorCountBounds.Generate(random);

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

            for (int j = 0; j <= 1; j++)//jeden objekt zatim
            {
                Vector3 objectPosition = roomData.Position;
                GameObject prefab = new GameObject("Vial_1");//TEMP
                ObjectData objectData = new ObjectData(objectPosition, prefab, j);

                roomData.Objects.Add(objectData);
            }

            save.Rooms.Add(roomData);
        }

        save.Save.RoomsCountBounds.Value = save.Rooms.Count;
        save.Rooms[random.Random(0, save.Rooms.Count)].IsStartRoom = true;

        List<RoomData> roomsCopy = new List<RoomData>(save.Rooms);
        foreach (RoomData room in roomsCopy)
        {
            Vector3 doorPosition = PlaceDoor(rooms, save, room, random, out bool isOnWE, out RectangleF hall);

            Vector3 hallPosition = new Vector3(hall.Position.x, 0, hall.Position.y);
            Vector3 hallSize = new Vector3(hall.Size.x, 0, hall.Size.y);

            RoomData roomData = new RoomData(hallSize, hallPosition, -1);

            save.Rooms.Add(roomData);

            DoorData doorData = new DoorData(doorPosition, -1, 1); // -1 for no linked room yet
            doorData.IsOnWE = isOnWE;

            save.Doors.Add(doorData);
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

    public static Vector3 PlaceDoor(List<RectangleF> rooms, DNDFileData save, RoomData room, BetterRandom random, out bool isOnWE, out RectangleF hall)
    {
        Vector3 doorPosition = Vector3.zero;

        Vector3 upperLeft = new(room.Position.x, 0, room.Position.z + room.Size.z);
        Vector3 upperRight = new(room.Position.x + room.Size.x, 0, room.Position.z + room.Size.z);
        Vector3 lowerLeft = new(room.Position.x, 0, room.Position.z);
        Vector3 lowerRight = new(room.Position.x + room.Size.x, 0, room.Position.z);

        Wall wallN = new Wall(upperLeft, upperRight, Orientation.N);
        Wall wallE = new Wall(lowerLeft, upperLeft, Orientation.E);
        Wall wallS = new Wall(lowerRight, lowerLeft, Orientation.S);
        Wall wallW = new Wall(lowerRight, upperRight, Orientation.W);

        save.Walls.Add(wallN);
        save.Walls.Add(wallE);
        save.Walls.Add(wallS);
        save.Walls.Add(wallW);

        bool isDoorPlaced = false;
        int attempts = 0, maxAttempts = 100;
        Orientation orientation;
        Bounds<float> hallBounds = new Bounds<float>(2f, 10f);
        float hallWidth = 1f;
        RectangleF possibleHall;

        List<RectangleF> roomsWithoutThis = new List<RectangleF>(rooms);
        roomsWithoutThis.RemoveAll(r => r.Position.x == room.Position.x && r.Position.y == room.Position.z);

        do
        {
            orientation = random.RandomOrientation();

            switch (orientation)
            {
                case Orientation.N:
                    doorPosition = random.RandomPointOnWall(upperLeft, upperRight);
                    possibleHall = new RectangleF(doorPosition, new Vector3(hallWidth, 0, hallBounds.Max)); // Hall extends north
                    break;
                case Orientation.S:
                    doorPosition = random.RandomPointOnWall(lowerLeft, lowerRight);
                    // Offset the hall so it extends south (subtract from z)
                    possibleHall = new RectangleF(
                        new Vector3(doorPosition.x, doorPosition.y, doorPosition.z - hallBounds.Max),
                        new Vector3(hallWidth, 0, hallBounds.Max));
                    break;
                case Orientation.E:
                    doorPosition = random.RandomPointOnWall(upperRight, lowerRight);
                    possibleHall = new RectangleF(doorPosition, new Vector3(hallBounds.Max, 0, hallWidth)); // Hall extends east
                    break;
                case Orientation.W:
                    doorPosition = random.RandomPointOnWall(upperLeft, lowerLeft);
                    // Offset the hall so it extends west (subtract from x)
                    possibleHall = new RectangleF(
                        new Vector3(doorPosition.x - hallBounds.Max, doorPosition.y, doorPosition.z),
                        new Vector3(hallBounds.Max, 0, hallWidth));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            isDoorPlaced = roomsWithoutThis.Any(r => r.Overlaps(possibleHall));
            attempts++;

        } while (!isDoorPlaced && attempts < maxAttempts);

        if (isDoorPlaced)
        {
            isOnWE = orientation == Orientation.W || orientation == Orientation.E;

            RectangleF targetRoom = roomsWithoutThis.First(r => r.Overlaps(possibleHall));
            float requiredLength = Vector3.Distance(doorPosition, targetRoom.Position);
            possibleHall = ResizeHall(possibleHall, requiredLength);

            hall = possibleHall;
            return doorPosition;
        }
        else
        {
            Debug.LogError("Failed to place door after " + attempts + " attempts");
            isOnWE = false;
            hall = new RectangleF(Vector3.zero, Vector3.zero);
            return Vector3.zero;
        }
    }

    private static RectangleF ResizeHall(RectangleF hall, float requiredLength)
    {
        Vector2 newSize = hall.Size;
        if (hall.Size.x > hall.Size.y) // Horizontal hall
            newSize.x = requiredLength;
        else // Vertical hall
            newSize.y = requiredLength;

        return new RectangleF(hall.Position, newSize);
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
            WriteGenerationBounds(writer, fileData.Save.DoorCountBounds, "DoorsCountBounds");

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

                foreach (ObjectData objectData in roomData.Objects)
                {
                    writer.WriteStartElement("Object");

                    writer.WriteElementString("ID", objectData.ID.ToString());
                    WriteVector3(writer, objectData.Position, "Position");
                    writer.WriteElementString("ObjectName", objectData.Object.name);

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteStartElement("Doors");
            foreach (DoorData doorData in fileData.Doors)
            {
                writer.WriteStartElement("Door");

                writer.WriteElementString("ID", doorData.ID.ToString());
                WriteVector3(writer, doorData.Position, "Position");
                writer.WriteElementString("LinkedRoomID", doorData.LinkedRoomID.ToString());
                writer.WriteElementString("IsOnWE", doorData.IsOnWE.ToString());

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
