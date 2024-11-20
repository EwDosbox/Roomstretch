using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class DNDFileScriptCreator : MonoBehaviour
{
    private string path;
    private string filePath;

    private void Awake()
    {
        //path = Application.persistentDataPath + "/Files/";
        path = UtilityScript.filePathPC;
        Directory.CreateDirectory(path);
    }
    public void CreateFile(string seed, string fileName)
    {
        filePath = path + fileName;
        File.Create(filePath).Dispose();

        WriteHead(seed);
    }

    private void WriteStartingEndingTag(string tag, string contents)
    {

        string toWrite = "<" + tag + "> " + contents + " </" + tag + ">";
        File.AppendAllText(filePath, toWrite);

    }
    private void WriteStartingTag(string tag)
    {

        string toWrite = "<" + tag + ">";
        File.AppendAllText(filePath, toWrite);

    }
    private void WriteEndingTag(string tag)
    {
        string toWrite = "</" + tag + ">";
        File.AppendAllText(filePath, toWrite);
    }
    private void WriteNewline()
    {
        string toWrite = "\n";
        File.AppendAllText(filePath, toWrite);
    }

    private void WriteHead(string seed)
    {
        WriteStartingTag("Head");
        WriteNewline();
        WriteStartingEndingTag("Version", UtilityScript.version);
        WriteNewline();
        WriteStartingEndingTag("Seed", seed);
        WriteNewline();
        WriteEndingTag("Head");
    }
}
