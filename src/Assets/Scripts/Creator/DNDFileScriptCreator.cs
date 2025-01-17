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

    public void PrepareSave(DNDFileData save, bool shouldGenRanNoOfRooms, int noOfRooms)
    {
        System.Random ran = save.Save.Random;

        save.Save.ShouldGenRanNoOfRooms = shouldGenRanNoOfRooms;
        if (save.Save.ShouldGenRanNoOfRooms)
        {
            save.Save.NoOfRooms = ran.Next(save.Save.LowerBoundNoOfRooms, save.Save.UpperBoundNoOfRooms + 1);
        }
        else
        {
            save.Save.NoOfRooms = noOfRooms;
        }

        foreach(int i in Enumerable.Range(0, noOfRooms))
        {
            Vector3 size = Vector3.zero;
            Vector3 position = Vector3.zero;

            size.x = ran.Next(2, 10);
            size.y = ran.Next(2, 10);

            position.x = ran.Next(2, 10);
            position.y = ran.Next(2, 10);

            List<DoorData> doors = new List<DoorData>();
            List<ObjectData> objects = new List<ObjectData>();

            save.AddRoom(size, position, doors, objects);
        }
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

            writer.WriteElementString("Version", fileData.Save.Version);
            writer.WriteElementString("Seed", fileData.Save.Seed);

            writer.WriteEndElement();
            #endregion
            #region Body
            writer.WriteStartElement("Body");

            foreach (RoomData roomData in fileData.Rooms)
            {
                writer.WriteStartElement("Room");

                writer.WriteElementString("ID", roomData.Id.ToString());

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

                    writer.WriteElementString("ID", doorData.DoorID.ToString());

                    writer.WriteElementString("LinkedRoomID", doorData.LinkedRoom.Id.ToString());

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
}
