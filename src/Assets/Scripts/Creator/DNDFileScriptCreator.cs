using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor.Playables;
using UnityEngine;

public class DNDFileScriptCreator : MonoBehaviour
{
    private string filePath;
    
    [SerializeField]
    private SaveScript save;

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

    public void CreateFile(SaveScript save)
    {
        XmlWriterSettings settings = new()
        {
            Indent = true, 
            NewLineOnAttributes = false

        };

        DNDFileData data = save.DNDFileData;

        using (XmlWriter writer = XmlWriter.Create(filePath, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("RoomStretch");
            #region Head
            writer.WriteStartElement("Head");

            writer.WriteElementString("Version", data.Version);
            writer.WriteElementString("Seed", data.Seed);

            writer.WriteEndElement();
            #endregion
            #region Body
            writer.WriteStartElement("Body");

            foreach (RoomData roomData in data.Rooms)
            {
                writer.WriteStartElement("Room");

                writer.WriteElementString("ID", roomData.Id.ToString());

                writer.WriteElementString("Height", roomData.Size.z.ToString());
                writer.WriteElementString("Width", roomData.Size.y.ToString());
                writer.WriteElementString("Depth", roomData.Size.x.ToString());


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
