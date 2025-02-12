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

    [SerializeField]
    private DNDFileData fileData;

    #region PrepareSave
    public void PrepareSave(DNDFileData save)
    {
        BetterRandom random = save.Save.Random;

        save.Save.RoomsCountBounds.Generate(random);
        save.Save.DepthBounds.Generate(random);
        save.Save.WidthBounds.Generate(random);


        List<Rectangle> existingRooms = new List<Rectangle>();

        for (int i = 0; i < save.Save.RoomsCountBounds.Value; i++)
        {
            Vector3 size;
            Vector3 position;
            Rectangle newRoom;
            int attempts = 0;
            const int maxAttempts = 100;

            // Try to place the room without overlapping
            do
            {
                size = random.RandomVector3(save.Save.WidthBounds.ExtremesBounds, save.Save.WidthBounds.ExtremesBounds);
                position = random.RandomVector3(save.Save.WidthBounds.ExtremesBounds, save.Save.WidthBounds.ExtremesBounds);
                newRoom = new Rectangle(size.x, size.z, position.x, position.z);
                attempts++;
            } while (IsOverlapping(existingRooms, newRoom) && attempts < maxAttempts);

            // Log a warning if the room couldn't be placed
            if (attempts >= maxAttempts)
            {
                Debug.LogWarning($"Failed to place room {i + 1} after {maxAttempts} attempts.");
                continue;
            }

            // Add the new room to the list of existing rooms
            existingRooms.Add(newRoom);

            // Add the room to the save data
            save.AddRoom(size, position, new List<DoorData>(), new List<ObjectData>());
        }
    }
    #endregion
    private bool IsOverlapping(List<Rectangle> existingRooms, Rectangle newRoom)
    {
        foreach (Rectangle room in existingRooms)
        {
            if (room.AreOverlapping(newRoom))
            {
                return true;
            }
        }
        return false;
    }
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
            WriteGenerationBounds(writer, fileData.Save.WidthBounds, "WidthBounds");
            WriteGenerationBounds(writer, fileData.Save.DepthBounds, "DepthBounds");

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
