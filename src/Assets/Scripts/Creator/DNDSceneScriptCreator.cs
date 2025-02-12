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

    private GameObject Map;

    private Dictionary<string, GameObject> Models;
    private Dictionary<string, GameObject> Prefabs;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == "LevelScene")
        {
            Map = GameObject.Find("Map");
            LoadModels();
            LoadPrefabs();
            MakeMap();
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
    private void LoadPrefabs()
    {
        List<GameObject> resources = Resources.LoadAll<GameObject>("Prefabs").ToList();
        Prefabs = new Dictionary<string, GameObject>();

        foreach (GameObject resource in resources)
        {
            Prefabs.Add(resource.name, resource);
        }

        Debug.Log("Creator: Imported: " + Prefabs.ToSeparatedString("; "));
    }
    private void MakeMap()
    {
        fileData = ParseDNDFile(fileData.Save.FilePath);

        foreach (RoomData room in fileData.Rooms)
        {
            Vector3 size = room.Size;
            Vector3 position = room.Position;

            GameObject toInstantiate = Instantiate(Prefabs["Room"], position, Quaternion.identity, Map.transform);
            toInstantiate.transform.localScale = new Vector3(size.x, 1, size.z); // Adjust size, keeping Y scale fixed

            Instantiate(toInstantiate, Map.transform, true);
        }

        Debug.Log("Creator: " + fileData.ToString());

    }

    private DNDFileData ParseDNDFile(string filePath)
    {
        string fileContent = File.ReadAllText(filePath);
        XDocument document = XDocument.Parse(fileContent);

        DNDFileData file = ScriptableObject.CreateInstance<DNDFileData>();

        XElement roomstretch = document.Element("RoomStretch");

        XElement body = roomstretch.Element("Body");

        List<XElement> rooms = body.Elements("Room").ToList();

        foreach (XElement room in rooms)
        {
            XElement sizeE = room.Element("Size");
            Vector3 size = Vector3.zero;
            size.z = float.Parse(sizeE.Element("Height").Value.Trim());
            size.y = float.Parse(sizeE.Element("Width").Value.Trim());
            size.x = float.Parse(sizeE.Element("Depth").Value.Trim());

            XElement postionE = room.Element("Position");
            Vector3 position = Vector3.zero;
            position.z = float.Parse(postionE.Element("Height").Value.Trim());
            position.y = float.Parse(postionE.Element("Width").Value.Trim());
            position.x = float.Parse(postionE.Element("Depth").Value.Trim());

            List<XElement> doors = body.Elements("Door").ToList();
            List<DoorData> doorsData = new List<DoorData>();

            foreach (XElement door in doors)
            {
                int id = int.Parse(door.Element("ID").Value.Trim());
                int linkedRoomID = int.Parse(door.Element("LinkedRoomID").Value.Trim());

                Vector3 doorPosition = Vector3.zero;
                doorPosition.z = float.Parse(door.Element("Height").Value.Trim());
                doorPosition.y = float.Parse(door.Element("Width").Value.Trim());
                doorPosition.x = float.Parse(door.Element("Depth").Value.Trim());

                doorsData.Add(new DoorData(doorPosition, linkedRoomID, id));
            }

            List<XElement> objects = body.Elements("Object").ToList();
            List<ObjectData> objectDatas = new List<ObjectData>();

            foreach (XElement prefab in objects)
            {
                int id = int.Parse(prefab.Element("ID").Value.Trim());
                string objectName = prefab.Element("ObjectName").Value.Trim();

                Vector3 objectPosition = Vector3.zero;
                objectPosition.z = float.Parse(prefab.Element("Height").Value.Trim());
                objectPosition.y = float.Parse(prefab.Element("Width").Value.Trim());
                objectPosition.x = float.Parse(prefab.Element("Depth").Value.Trim());

                objectDatas.Add(new ObjectData(objectPosition, Models[objectName], id));
            }

            file.AddRoom(
                size,
                 position,
                  doorsData,
                   objectDatas);
        }

        return file;
    }

}
