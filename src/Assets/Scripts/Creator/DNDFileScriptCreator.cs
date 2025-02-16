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
    [Header("Debug Visualization")]
    [SerializeField] private bool showRooms = true;
    [SerializeField] private Color roomColor = Color.green;
    private List<RectangleF> rooms; 
    
    #region PrepareSave
    public void PrepareSave(DNDFileData save)
    {
        BetterRandom random = save.Save.Random;

        save.Save.RoomsCountBounds.Generate(random);

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
            Vector3 doorPosition = roomData.Position;
            int linkedRoomID = random.Random(0, save.Save.RoomsCountBounds.Value);
            DoorData doorData = new DoorData(doorPosition, linkedRoomID, 0);

            roomData.Door = doorData;

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
    #region TEMP
    public void OnDrawGizmos()
    {
        if (!showRooms || rooms == null)
            return;

        Gizmos.color = roomColor;

        foreach (var room in rooms)
        {
            Vector2 pos = room.Position;
            Vector2 size = room.Size;

            // Calculate center and size for the wire cube
            Vector3 center = new Vector3(pos.x + size.x / 2, 0.1f, pos.y + size.y / 2);
            Vector3 cubeSize = new Vector3(size.x, 0, size.y);

            Gizmos.DrawWireCube(center, cubeSize);
        }
    }
    #endregion
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

                writer.WriteStartElement("Door");

                writer.WriteElementString("ID", roomData.Door.ID.ToString());
                WriteVector3(writer, roomData.Door.Position, "Position");

                writer.WriteElementString("LinkedRoomID", roomData.Door.LinkedRoomID.ToString());

                writer.WriteEndElement();



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