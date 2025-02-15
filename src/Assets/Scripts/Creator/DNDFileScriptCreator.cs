using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor.Playables;
using UnityEngine;

public class DNDFileScriptCreator : MonoBehaviour
{
    #region PrepareSave
    public void PrepareSave(DNDFileData save)
    {
        BetterRandom random = save.Save.Random;

        save.Save.ZBounds = new GenerationBounds<float>(14, new Bounds<float>(10, 20));

        save.Save.RoomsCountBounds.Generate(random);
        save.Save.YBounds.Generate(random);
        save.Save.XBounds.Generate(random);
        save.Save.ZBounds.Generate(random);

        List<Cube> rooms = new List<Cube>();

        for (int i = 0; i < save.Save.RoomsCountBounds.Value; i++)
        {
            Vector3 roomPosition, roomSize;
            Cube room;
            int attempts = 0, maxAttempts = 100;
            bool placed = true;


            do
            {
                if(!placed) Debug.LogWarning($"Could not place room {i} without overlapping after {attempts} attempts.");
                roomPosition = random.RandomVector3(save.Save.XBounds.ExtremesBounds, save.Save.ZBounds.ExtremesBounds);
                roomSize = random.RandomVector3(save.Save.XBounds.ExtremesBounds, save.Save.YBounds.ExtremesBounds, save.Save.ZBounds.ExtremesBounds);

                roomSize.x = Mathf.Abs(roomSize.x);
                roomSize.y = 0;
                roomSize.z = Mathf.Abs(roomSize.z);

                Debug.Log("Room: " + roomPosition.ToString() + " " + roomSize.ToString());

                room = new Cube(roomPosition, roomSize);
                attempts++;

                placed = !rooms.Any(r => room.Overlaps(r));

            } while (!placed && attempts < maxAttempts);

            if (!placed)
            {
                Debug.LogWarning($"Could not place room {i} without overlapping after {maxAttempts} attempts.");
                continue; // Skip this room, or handle differently.
            }

            rooms.Add(room);

            RoomData roomData = new RoomData(roomSize, roomPosition, i);

            for (int j = 0; j <= 1; j++)//jedny dvere zatim
            {
                Vector3 doorPosition = roomData.Position;
                int linkedRoomID = random.Random(0, save.Save.RoomsCountBounds.Value);
                DoorData doorData = new DoorData(doorPosition, linkedRoomID, j);

                roomData.Doors.Add(doorData);
            }

            for (int j = 0; j <= 1; j++)//jeden objekt zatim
            {
                Vector3 objectPosition = roomData.Position;
                GameObject prefab = new GameObject("Vial_1");//TEMP
                ObjectData objectData = new ObjectData(objectPosition, prefab, j);

                roomData.Objects.Add(objectData);
            }

            save.Rooms.Add(roomData);
        }
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
            WriteGenerationBounds(writer, fileData.Save.XBounds, "XBounds");
            WriteGenerationBounds(writer, fileData.Save.YBounds, "YBounds");
            WriteGenerationBounds(writer, fileData.Save.ZBounds, "ZBounds");

            writer.WriteEndElement();
            writer.WriteStartElement("Settings");

            writer.WriteElementString("FOV", fileData.Settings.FOV.ToString());
            writer.WriteElementString("Sensitivity", fileData.Settings.Sensitivity.ToString());

            writer.WriteEndElement();

            writer.WriteEndElement();
            #endregion
            #region Body
            writer.WriteStartElement("Body");

            foreach (RoomData roomData in fileData.Rooms)
            {
                writer.WriteStartElement("Room");

                writer.WriteElementString("ID", roomData.ID.ToString());

                WriteVector3(writer, roomData.Position, "Position");
                WriteVector3(writer, roomData.Size, "Size");

                foreach (DoorData doorData in roomData.Doors)
                {
                    writer.WriteStartElement("Door");

                    writer.WriteElementString("ID", doorData.ID.ToString());
                    WriteVector3(writer, doorData.Position, "Position");

                    writer.WriteElementString("LinkedRoomID", doorData.LinkedRoomID.ToString());

                    writer.WriteEndElement();
                }


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

        writer.WriteElementString("ShouldGenerate", bounds.ShouldGenerate.ToString());
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
#region Cube
[System.Serializable]
public class Cube
{
    [SerializeField] private float x;
    [SerializeField] private float y;
    [SerializeField] private float z;
    [SerializeField] private float width;
    [SerializeField] private float height;
    [SerializeField] private float depth;

    public Vector3 Position => new Vector3(x, y, z);
    public Vector3 Size => new Vector3(width, height, depth);

    public Cube(Vector3 position, Vector3 size)
    {
        x = position.x;
        y = position.y;
        z = position.z;
        width = Mathf.Abs(size.x);
        height = Mathf.Abs(size.y);
        depth = Mathf.Abs(size.z);
    }

    public bool Overlaps(Cube other)
    {
        Bounds thisBounds = new Bounds(Position + Size / 2f, Size);
        Bounds otherBounds = new Bounds(other.Position + other.Size / 2f, other.Size);

        return thisBounds.Intersects(otherBounds);
    }

    public override string ToString()
    {
        return $"Box: Pos=({x:F2},{y:F2},{z:F2}), Size=({width:F2},{height:F2},{depth:F2})";
    }
}

#endregion