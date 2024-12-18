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

    private string file;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == "LevelScene")
        {
            if (!string.IsNullOrEmpty(utilityScript.filePath))
            {
                LoadModels();
                PrepareFile();
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
        throw new NotImplementedException();
    }

    private bool isBody()
    {
        throw new NotImplementedException();
    }

    private void PrepareFile()
    {
        file = File.ReadAllText(utilityScript.filePath);
    }
}
