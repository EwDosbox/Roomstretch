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
    private string filePath;

    [SerializeField]
    private DNDFileData fileData;

    public string FilePath
    {
        get
        {
            return filePath;
        }
    }

    private void Awake()
    {
        filePath = Application.persistentDataPath + "/Files/temp";
        Directory.CreateDirectory(Application.persistentDataPath + "/Files");
    }

    public void PrepareSave(DNDFileData save)
    {
        BetterRandom random = save.Save.Random;

        save.Save.RoomsCountBounds.Generate(random);
        save.Save.DepthBounds.Generate(random);
        save.Save.WidthBounds.Generate(random);
        

        List<Rectangle> existingRooms = new List<Rectangle>();

        for (int i = 0; i < save.Save.RoomsCountBounds.NoOfGenerations; i++)
        {
            Vector3 size;
            Vector3 position;
            Rectangle newRoom;
            int attempts = 0;
            const int maxAttempts = 100;

            // Try to place the room without overlapping
            do
            {
                size = random.RandomVector3(save.Save.WidthBounds,save.Save.WidthBounds);
                position = random.RandomVector3(save.Save.WidthBounds,save.Save.WidthBounds);
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
    // Helper method to check if a new room overlaps with any existing rooms
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

    public void CreateFile(DNDFileData fileData)
    {
        XmlWriterSettings settings = new()
        {
            Indent = true,
            NewLineOnAttributes = false

        };

        using (XmlWriter writer = XmlWriter.Create(filePath, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("RoomStretch");
            #region Head
            writer.WriteStartElement("Head");

            writer.WriteStartElement("Save");

            writer.WriteElementString("Version", fileData.Save.Version);
            writer.WriteElementString("Seed", fileData.Save.Seed);
            writer.WriteElementString("FilePath", fileData.Save.FilePath);
            WriteGenerationBounds(writer, fileData.Save.RoomsCountBounds);
            WriteGenerationBounds(writer, fileData.Save.WidthBounds);
            WriteGenerationBounds(writer, fileData.Save.DepthBounds);

            writer.WriteEndElement();
            writer.WriteStartElement("Settings");

            writer.WriteElementString("FOV", fileData.Settings.FOV);
            writer.WriteElementString("Sensitivity", fileData.Settings.Sensitivity);

            writer.WriteEndElement();

            writer.WriteEndElement();
            #endregion
            #region Body
            writer.WriteStartElement("Body");

            foreach (RoomData roomData in fileData.Rooms)
            {
                writer.WriteStartElement("Room");

                writer.WriteElementString("ID", roomData.ID.ToString());

                writer.WriteStartElement("Size");
                writer.WriteElementString("Height", roomData.Size.z.ToString());
                writer.WriteElementString("Width", roomData.Size.y.ToString());
                writer.WriteElementString("Depth", roomData.Size.x.ToString());
                writer.WriteEndElement();

                writer.WriteStartElement("Position");
                writer.WriteElementString("Height", roomData.Position.z.ToString());
                writer.WriteElementString("Width", roomData.Position.y.ToString());
                writer.WriteElementString("Depth", roomData.Position.x.ToString());
                writer.WriteEndElement();


                foreach (DoorData doorData in roomData.Doors)
                {
                    writer.WriteStartElement("Door");

                    writer.WriteElementString("ID", doorData.ID.ToString());

                    writer.WriteElementString("LinkedRoomID", doorData.LinkedRoomID.ToString());

                    writer.WriteElementString("Height", doorData.Position.z.ToString());
                    writer.WriteElementString("Width", doorData.Position.y.ToString());
                    writer.WriteElementString("Depth", doorData.Position.x.ToString());

                    writer.WriteEndElement();
                }


                foreach (ObjectData objectData in roomData.Objects)
                {
                    writer.WriteStartElement("Object");

                    writer.WriteElementString("ObjectName", objectData.Object.name);

                    writer.WriteElementString("Height", objectData.Position.z.ToString());
                    writer.WriteElementString("Width", objectData.Position.y.ToString());
                    writer.WriteElementString("Depth", objectData.Position.x.ToString());

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            #endregion

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }
        Debug.Log("File: Temp.dnd created at: " + filePath);
    }

    private void WriteGenerationBounds(XmlWriter writer, GenerationBounds bounds)
    {
        writer.WriteStartElement("GenerationBounds");

        writer.WriteElementString("ShouldGenerate", bounds.ShouldGenerate);
        writer.WriteElementString("Value", bounds.Value);
        writer.WriteElementString("DefaultValue", bounds.DefaultValue);

        WriteBound(writer, bounds.ExtremesBounds);

        writer.WriteEndElement();
    }
    private void WriteBound(XmlWriter writer,Bound bound){
        writer.WriteStartElement("Bound");

        writer.WriteElementString("Min", bound.Min);
        writer.WriteElementString("Max", bound.Max);

        writer.WriteEndElement();
    }
}
