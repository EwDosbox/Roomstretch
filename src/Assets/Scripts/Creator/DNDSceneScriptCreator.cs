using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DNDSceneScriptCreator : MonoBehaviour
{
    [SerializeField] private DNDFileData fileData;
    [SerializeField] private GameObject Player;

    private GameObject Map;
    private Dictionary<string, GameObject> Prefabs;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == "LevelScene")
        {
            Map = GameObject.Find("Map");
            LoadPrefabs();
            MakeMap();
        }
    }    
    #region LoadResources
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
        foreach (DoorData door in fileData.Doors)
        {
            InstantiateDoor(door);
        }

        Debug.Log("Creator: " + fileData.ToString());
    }

    #endregion
    #region Instantiate Methods
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
        // Correct Scaling
        wallN.localScale = new Vector3(1, 1, size.x / 2);
        wallS.localScale = new Vector3(1, 1, size.x / 2);
        wallE.localScale = new Vector3(1, 1, size.z / 2);
        wallW.localScale = new Vector3(1, 1, size.z / 2);

        wallN.rotation = Quaternion.Euler(new Vector3(0, 90f, 0));
        wallS.rotation = Quaternion.Euler(new Vector3(0, 90f, 0));
        wallE.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        wallW.rotation = Quaternion.Euler(new Vector3(0, 0, 0));

        wallN.localPosition = new Vector3(size.x / 2, 0, size.z);
        wallS.localPosition = new Vector3(size.x / 2, 0, 0);
        wallE.localPosition = new Vector3(size.x, 0, size.z / 2);
        wallW.localPosition = new Vector3(0, 0, size.z / 2);

        if (room.IsStartRoom)
        {
            Player.transform.position = room.Position + room.Size / 2 + new Vector3(0, 1, 0);
        }

        Debug.Log($"Room created at {position} with size {size}");
    }
    private void InstantiateDoor(DoorData door)
    {
        Vector3 position = door.Position;
        position.y = Prefabs["Arch"].transform.position.y;

        GameObject doorObject = Instantiate(Prefabs["Arch"], position, Quaternion.identity, Map.transform);

        if (door.IsOnWE)
        {
            doorObject.transform.rotation = Quaternion.Euler(new Vector3(0, 90f, 0));
        }

        Debug.Log($"Door created at {position}");
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
        file.Save.DoorCountBounds = ParseGenerationBounds<int>(save.Element("DoorsCountBounds"));
        file.Save.XRoomBounds = ParseBounds<float>(save.Element("XRoomBounds"));
        file.Save.ZRoomBounds = ParseBounds<float>(save.Element("ZRoomBounds"));
        file.Save.XMapBounds = ParseBounds<float>(save.Element("XMapBounds"));
        file.Save.ZMapBounds = ParseBounds<float>(save.Element("ZMapBounds"));

        XElement settings = head.Element("Settings");

        file.Settings.FOV = float.Parse(settings.Element("FOV").Value.Trim());
        file.Settings.Sensitivity = float.Parse(settings.Element("Sensitivity").Value.Trim());

        XElement body = roomstretch.Element("Body");
        XElement roomsElement = body.Element("Rooms");

        List<XElement> rooms = roomsElement.Elements("Room").ToList();

        foreach (XElement room in rooms)
        {
            int id = int.Parse(room.Element("ID").Value.Trim());
            bool isStartRoom = room.Element("IsStartRoom").Value.Trim() == "True";

            Vector3 position = ParseVector3(room.Element("Position"));
            Vector3 size = ParseVector3(room.Element("Size"));

            List<XElement> objects = room.Elements("Object").ToList();
            List<ObjectData> objectDatas = new List<ObjectData>();

            foreach (XElement prefab in objects)
            {
                int objectId = int.Parse(prefab.Element("ID").Value.Trim());
                Vector3 objectPosition = ParseVector3(prefab.Element("Position"));
                string objectName = prefab.Element("ObjectName").Value.Trim();

                objectDatas.Add(new ObjectData(objectPosition, Prefabs[objectName], objectId));
            }

            file.AddRoom(size, position, objectDatas);
            file.Rooms.Where(r => r.Position == position).First().IsStartRoom = isStartRoom;
        }

        XElement doorsElement = body.Element("Doors");
        List<XElement> doors = doorsElement.Elements("Door").ToList();
        foreach (XElement door in doors)
        {
            int doorID = int.Parse(door.Element("ID").Value.Trim());
            Vector3 doorPosition = ParseVector3(door.Element("Position"));
            int linkedRoomID = int.Parse(door.Element("LinkedRoomID").Value.Trim());
            bool isOnWE = door.Element("IsOnWE").Value.Trim() == "True";

            file.AddDoor(doorPosition, linkedRoomID);
            file.Doors.Where(d => d.Position == doorPosition).First().IsOnWE = isOnWE;
        }

        XElement wallsElement = body.Element("Walls");
        List<XElement> walls = wallsElement.Elements("Wall").ToList();
        foreach (XElement wall in walls)
        {
            Vector3 wallStart = ParseVector3(wall.Element("Start"));
            Vector3 wallEnd = ParseVector3(wall.Element("End"));
            char wallOrientationValue = wall.Element("Orientation").Value.Trim()[0];

            Orientation wallOrientation = Orientation.N;
            if(wallOrientationValue == 'N') wallOrientation = Orientation.N;
            else if(wallOrientationValue == 'S') wallOrientation = Orientation.S;
            else if(wallOrientationValue == 'E') wallOrientation = Orientation.E;
            else if(wallOrientationValue == 'W') wallOrientation = Orientation.W;

            Wall wallData = new Wall(wallStart, wallEnd, wallOrientation);
            file.Walls.Add(wallData);
        }

        return file;
    }
    #endregion
    #region Helping Parse Methods
    private GenerationBounds<T> ParseGenerationBounds<T>(XElement element) where T : IComparable<T>
    {
        bool ShouldUseDefaultValue = element.Element("ShouldUseDefaultValue").Value.Trim() == "True";
        T value = (T)Convert.ChangeType(element.Element("Value").Value.Trim(), typeof(T));
        T defaultValue = (T)Convert.ChangeType(element.Element("DefaultValue").Value.Trim(), typeof(T));
        Bounds<T> extremesBounds = ParseBounds<T>(element.Element("ExtremesBounds"));

        GenerationBounds<T> generationBounds = new GenerationBounds<T>(defaultValue, extremesBounds);
        generationBounds.ShouldUseDefaultValue = ShouldUseDefaultValue;
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
