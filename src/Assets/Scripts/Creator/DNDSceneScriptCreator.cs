using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DNDSceneScriptCreator : MonoBehaviour
{
    [SerializeField]
    private DNDFileData fileData;

    private Dictionary<string, GameObject> Models;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == "LevelScene")
        {
            if (!string.IsNullOrEmpty(fileData.Save.FilePath))
            {
                LoadModels();
                MakeMap();
            }
            else
            {
                Debug.LogError("Creator: Badly made DND File Path");
            }
        }
    }

    private void LoadModels()
    {
        List<GameObject> resources = Resources.LoadAll<GameObject>("Models").ToList();
        Models = new Dictionary<string, GameObject>();

        foreach (GameObject resource in resources)
        {
            Models.Add(resource.name, resource);
        }

        Debug.Log("Creator: Imported: " + Models.ToSeparatedString("; "));
    }
    private void MakeMap()
    {
        fileData = ParseDNDFile(fileData.Save.FilePath);

        Debug.Log("Creator: " + fileData.ToString());

    }

    private DNDFileData ParseDNDFile(string filePath)
    {
        string fileContent = File.ReadAllText(filePath);
        XDocument document = XDocument.Parse(fileContent);

        DNDFileData file = ScriptableObject.CreateInstance<DNDFileData>();

        XElement roomstretch = document.Element("RoomStretch");

        XElement scene = roomstretch.Element("Scene");

        List<XElement> rooms = scene.Elements("Room").ToList();

        foreach (XElement room in rooms)
        {
            float height = float.Parse( room.Element("Height").Value.Trim());
            float width = float.Parse( room.Element("Width").Value.Trim());
            float depth = float.Parse( room.Element("Depth").Value.Trim());

            Vector3 size = new Vector3(depth, width, height);

            Vector3 position = Vector3.zero;

            List<XElement> doors = scene.Elements("Door").ToList();
            List<DoorData> doorsData = new List<DoorData>();

            foreach (XElement door in doors)
            {

            }

            List<XElement> objects = scene.Elements("Object").ToList();
            List<ObjectData> objectDatas = new List<ObjectData>();

            foreach (XElement prefab in objects)
            {

            }

            file.AddRoom(size, position, doorsData, objectDatas);
        }

        return file;
    }

}
