using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Playables;
using UnityEngine;

public class DNDFileScriptCreator : MonoBehaviour
{
    private string filePath;
    
    [SerializeField]
    public UtilityScript utilityScript;

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
    /// <summary>
    /// Creates .dnd file with specified paramters
    /// </summary>
    /// <param name="seed">Seed of the file</param>
    public void CreateFile(string seed)
    {
        File.Create(filePath).Dispose();

        WriteHead(seed);
        Debug.Log("File: Temp.dnd created at: " + filePath);
    }
    /// <summary>
    /// Writes the <Head> part of the .dnd file
    /// </summary>
    /// <param name="seed">Seed of the file</param>
    private void WriteHead(string seed)
    {
        utilityScript.WriteStartingTag("Head", 0);
        utilityScript.WriteNewline();
        utilityScript.WriteStartingEndingTag("Version", utilityScript.version, 1);
        utilityScript.WriteNewline();
        utilityScript.WriteStartingEndingTag("Seed", seed, 1);
        utilityScript.WriteNewline();
        utilityScript.WriteEndingTag("Head", 0);
    }
}
