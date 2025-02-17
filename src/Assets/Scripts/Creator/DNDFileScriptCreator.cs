using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
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
            Vector3 roomPosition, roomSize;
            RectangleF room;
            int attempts = 0, maxAttempts = 100;
            bool placed;

            do
            {
                placed = false;

                roomPosition = random.RandomVector3(save.Save.XMapBounds, save.Save.ZMapBounds);
                roomSize = random.RandomVector3(save.Save.XRoomBounds, save.Save.ZRoomBounds);

                Debug.Log("Room: Pos" + roomPosition.ToString() + "\nSize " + roomSize.ToString());

                room = new RectangleF(roomPosition, roomSize);
                attempts++;

                placed = !rooms.Any(r => room.Overlaps(r));

            } while (!placed && attempts < maxAttempts);

            if (attempts >= maxAttempts)
            {
                Debug.LogError($"Failed to place room {i} after {attempts} attempts");
                continue;
            }
            else
            {
                Debug.Log($"Placed room {i} after {attempts} attempts");
            }

            rooms.Add(room);
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

        foreach (RoomData room in save.Rooms)
        {
            Vector3 doorPosition;
            bool isOnWE;
            int wall = random.Random(0, 4);
            switch (wall)
            {
                case 0: // North
                    doorPosition = new Vector3(room.Position.x + room.Size.x / 2, 0, room.Position.z + room.Size.z);
                    isOnWE = false;
                    break;
                case 1: // East
                    doorPosition = new Vector3(room.Position.x + room.Size.x, 0, room.Position.z + room.Size.z / 2);
                    isOnWE = true;
                    break;
                case 2: // South
                    doorPosition = new Vector3(room.Position.x + room.Size.x / 2, 0, room.Position.z);
                    isOnWE = false;
                    break;
                default: // West
                    doorPosition = new Vector3(room.Position.x, 0, room.Position.z + room.Size.z / 2);
                    isOnWE = true  ;
                    break;
            }

            DoorData doorData = new DoorData(doorPosition, -1, 1); // -1 for no linked room yet
            doorData.IsOnWE = isOnWE;

            save.Doors.Add(doorData);
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

            foreach (RoomData roomData in fileData.Rooms)
            {
                writer.WriteStartElement("Room");

                writer.WriteElementString("ID", roomData.ID.ToString());

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