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

    #region LoadResources
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
    #endregion
    #region MakeMap
    private void MakeMap()
    {
        fileData = ParseDNDFile(fileData.Save.FilePath);

        foreach (RoomData room in fileData.Rooms)
        {
            InstantiateRoom(room);
        }

        Debug.Log("Creator: " + fileData.ToString());
    }

    #endregion
    #region InstantiateRoom
    private void InstantiateRoom(RoomData room)
    {
        Vector3 size = room.Size;
        Vector3 position = room.Position;
        position.y = 1;

        GameObject roomObject = Instantiate(Prefabs["Room"], position, Quaternion.identity, Map.transform);

        // Find child walls
        Transform wallN = roomObject.transform.Find("WallN");
        Transform wallS = roomObject.transform.Find("WallS");
        Transform wallE = roomObject.transform.Find("WallE");
        Transform wallW = roomObject.transform.Find("WallW");

        // Scale walls
        wallN.localScale = new Vector3(1, 1, size.x);
        wallS.localScale = new Vector3(1, 1, size.x);
        wallE.localScale = new Vector3(1, 1, size.z);
        wallW.localScale = new Vector3(1, 1, size.z);

        wallN.localPosition = new Vector3(size.z, 0, 0);
        wallS.localPosition = new Vector3(-size.z, 0, 0);
        wallE.localPosition = new Vector3(0, 0, size.x);
        wallW.localPosition = new Vector3(0, 0, -size.x);

        Debug.Log($"Room created at {position} with size {size}");
    }
    #endregion
    #region ParseDNDFile
    private DNDFileData ParseDNDFile(string filePath)
    {
        string fileContent = File.ReadAllText(filePath);
        XDocument document = XDocument.Parse(fileContent);

        DNDFileData file = ScriptableObject.CreateInstance<DNDFileData>();
        file.Initialize();

        XElement roomstretch = document.Element("RoomStretch");

        XElement head = roomstretch.Element("Head");
        XElement save = head.Element("Save");

        file.Save.Version = save.Element("Version").Value.Trim();
        file.Save.Seed = save.Element("Seed").Value.Trim();
        file.Save.FilePath = save.Element("FilePath").Value.Trim();

        file.Save.RoomsCountBounds = ParseGenerationBounds<int>(save.Element("RoomsCountBounds"));
        file.Save.XRoomBounds = ParseGenerationBounds<float>(save.Element("XRoomBounds"));
        file.Save.ZRoomBounds = ParseGenerationBounds<float>(save.Element("ZRoomBounds"));
        file.Save.XMapBounds = ParseGenerationBounds<float>(save.Element("XMapBounds"));
        file.Save.ZMapBounds = ParseGenerationBounds<float>(save.Element("ZMapBounds"));

        XElement settings = head.Element("Settings");

        file.Settings.FOV = float.Parse(settings.Element("FOV").Value.Trim());
        file.Settings.Sensitivity = float.Parse(settings.Element("Sensitivity").Value.Trim());

        XElement body = roomstretch.Element("Body");

        List<XElement> rooms = body.Elements("Room").ToList();

        foreach (XElement room in rooms)
        {
            int id = int.Parse(room.Element("ID").Value.Trim());

            Vector3 position = ParseVector3(room.Element("Position"));
            Vector3 size = ParseVector3(room.Element("Size"));

            List<XElement> doors = room.Elements("Door").ToList();
            List<DoorData> doorsData = new List<DoorData>();

            foreach (XElement door in doors)
            {
                int doorId = int.Parse(door.Element("ID").Value.Trim());
                int linkedRoomID = int.Parse(door.Element("LinkedRoomID").Value.Trim());
                Vector3 doorPosition = ParseVector3(door.Element("Position"));

                doorsData.Add(new DoorData(doorPosition, linkedRoomID, doorId));
            }

            List<XElement> objects = room.Elements("Object").ToList();
            List<ObjectData> objectDatas = new List<ObjectData>();

            foreach (XElement prefab in objects)
            {
                int objectId = int.Parse(prefab.Element("ID").Value.Trim());
                Vector3 objectPosition = ParseVector3(prefab.Element("Position"));
                string objectName = prefab.Element("ObjectName").Value.Trim();

                objectDatas.Add(new ObjectData(objectPosition, Models[objectName], objectId));
            }

            file.AddRoom(size, position, doorsData, objectDatas);
        }

        return file;
    }
    #endregion
    #region Helping Parse Methods
    private GenerationBounds<T> ParseGenerationBounds<T>(XElement element) where T : IComparable<T>
    {
        bool shouldGenerate = element.Element("ShouldGenerate").Value.Trim() == "True";
        T value = (T)Convert.ChangeType(element.Element("Value").Value.Trim(), typeof(T));
        T defaultValue = (T)Convert.ChangeType(element.Element("DefaultValue").Value.Trim(), typeof(T));
        Bounds<T> extremesBounds = ParseBounds<T>(element.Element("ExtremesBounds"));

        GenerationBounds<T> generationBounds = new GenerationBounds<T>(defaultValue, extremesBounds);
        generationBounds.ShouldGenerate = shouldGenerate;
        generationBounds.Value = value;

        return generationBounds;
    }
    private Bounds<T> ParseBounds<T>(XElement element) where T : IComparable<T>
    {
        T min = (T)Convert.ChangeType(element.Element("Min").Value.Trim(), typeof(T));
        T max = (T)Convert.ChangeType(element.Element("Max").Value.Trim(), typeof(T));

        return new Bounds<T>(min, max);
    }
    private Vector3 ParseVector3(XElement element)
    {
        Vector3 vector = Vector3.zero;

        vector.x = float.Parse(element.Element("X").Value.Trim());
        vector.y = float.Parse(element.Element("Y").Value.Trim());
        vector.z = float.Parse(element.Element("Z").Value.Trim());

        return vector;
    }
    #endregion
}
