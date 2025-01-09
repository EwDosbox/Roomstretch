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
    private SaveScript save;

    private Dictionary<string, GameObject> Models;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == "LevelScene")
        {
            if (!string.IsNullOrEmpty(save.filePath))
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
        DNDFileData file = ParseDNDFile(save.filePath);

        Debug.Log("Creator: " + file.Rooms[0].Height);

    }

    private DNDFileData ParseDNDFile(string filePath)
    {
        var fileContent = File.ReadAllText(filePath);
        var document = XDocument.Parse(fileContent);

        var head = document.Element("RoomStretch")?.Element("Head");
        if (head == null) throw new InvalidDataException("Missing Head element in DND file.");

        var roomElement = document.Element("RoomStretch")?.Element("Room");
        if (roomElement == null) throw new InvalidDataException("Missing Room element in DND file.");

        Vector3 size = new Vector3(
            int.Parse(roomElement.Element("Depth")?.Value.Trim() ?? "0"),
            int.Parse(roomElement.Element("Width")?.Value.Trim() ?? "0"),
            int.Parse(roomElement.Element("Height")?.Value.Trim() ?? "0"));

        // Parse the room data
        var roomData = new RoomData
        (
            size, new List<DoorData>(), new List<ObjectData>(), this
        );

        // Return the parsed data
        return new DNDFileData(
            head.Element("Version")?.Value.Trim() ?? "1.0",
            head.Element("Seed")?.Value.Trim() ?? "default",
            new List<RoomData> { roomData });
    }

}
