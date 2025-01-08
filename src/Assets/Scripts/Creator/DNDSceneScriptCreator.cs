using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DNDSceneScriptCreator : MonoBehaviour
{
    [SerializeField]
    private UtilityScript utilityScript;

    private Dictionary<string, GameObject> Models;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == "LevelScene")
        {
            if (!string.IsNullOrEmpty(utilityScript.filePath))
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
        string[] linesRead = utilityScript.ReadIndentation(0);

        foreach (string line in linesRead)
        {
            Debug.Log("Creator: Read0:" + line);
        }

        linesRead = utilityScript.ReadIndentation(1); foreach (string line in linesRead)
        {
            Debug.Log("Creator: Read1:" + line);
        }

        Instantiate(Models["Book_1"]);
    }

    private bool isBody()
    {
        throw new NotImplementedException();
    }
}
