using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DNDFileScriptCreator : MonoBehaviour
{
    private string path;
    private string filePath;
    private int degreeOfDepth;

    private void Awake()
    {
        path = Application.persistentDataPath + "/Files/";
        //path = UtilityScript.filePathPC;
        Directory.CreateDirectory(path);
        degreeOfDepth = 0;
    }
    public void CreateFile(string seed, string fileName)
    {
        filePath = path + fileName;
        File.Create(filePath).Dispose();

        WriteHead(seed);
        Debug.Log(".dnd File called: " + fileName + " Created at: " + filePath);
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

    private void WriteStartingEndingTag(string tag, string contents)
    {

        string toWrite = Indentation() + "<" + tag + "> " + contents + " </" + tag + ">";
        File.AppendAllText(filePath, toWrite);

    }
    private void WriteStartingTag(string tag)
    {
        string toWrite = Indentation() + "<" + tag + ">";
        IncreaseIndentation();
        File.AppendAllText(filePath, toWrite);

    }
    private void WriteEndingTag(string tag)
    {
        DecreaseIndentation();
        string toWrite = Indentation() + "</" + tag + ">";
        File.AppendAllText(filePath, toWrite);
    }
    private void WriteNewline()
    {
        string toWrite = "\n";
        File.AppendAllText(filePath, toWrite);
    }
    private void IncreaseIndentation()
    {
        degreeOfDepth++;
    }
    private void DecreaseIndentation()
    {
        degreeOfDepth--;
    }

    private string Indentation()
    {
        string indentation = "";

        for (int i = 0; i < degreeOfDepth; i++)
        {
            indentation += "    ";
        }

        return indentation;
    }
}
