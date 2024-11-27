using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Playables;
using UnityEngine;

public class DNDFileScriptCreator : MonoBehaviour
{
    private string filePath;
    private int degreeOfDepth;

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
        degreeOfDepth = 0;
    }
    /// <summary>
    /// Creates .dnd file with specified paramters
    /// </summary>
    /// <param name="seed">Seed of the file</param>
    /// <param name="fileName">Name of the File</param>
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
        WriteStartingTag("Head");
        WriteNewline();
        WriteNewline();
        WriteStartingEndingTag("Seed", seed);
        WriteNewline();
        WriteEndingTag("Head");
    }
    /// <summary>
    /// Writes a tag that starts and ends e.g. <Seed> 123 </Seed>
    /// </summary>
    /// <param name="tag">Name of the Tag to write e.g. </param>
    /// <param name="contents">Contents of the tag e.g. 123</param>
    private void WriteStartingEndingTag(string tag, string contents)
    {
        string toWrite = Indentation() + "<" + tag + "> " + contents + " </" + tag + ">";
        File.AppendAllText(filePath, toWrite);
    }
    /// <summary>
    /// Writes a tag that starts e.g. <Head>
    /// </summary>
    /// <param name="tag">Name of the Tag to write</param>
    private void WriteStartingTag(string tag)
    {
        string toWrite = Indentation() + "<" + tag + ">";
        IncreaseIndentation();
        File.AppendAllText(filePath, toWrite);
    }
    /// <summary>
    /// Writes a tag that ends </Head>
    /// </summary>
    /// <param name="tag">Name of the Tag to write</param>
    private void WriteEndingTag(string tag)
    {
        DecreaseIndentation();
        string toWrite = Indentation() + "</" + tag + ">";
        File.AppendAllText(filePath, toWrite);
    }
    /// <summary>
    /// Writes a New Line to the file
    /// </summary>
    private void WriteNewline()
    {
        string toWrite = "\n";
        File.AppendAllText(filePath, toWrite);
    }
    /// <summary>
    /// Increases Indetation for the next tag
    /// </summary>
    private void IncreaseIndentation()
    {
        degreeOfDepth++;
    }
    /// <summary>
    /// Decreases Indentation for the next tag
    /// </summary>
    private void DecreaseIndentation()
    {
        degreeOfDepth--;
    }
    /// <summary>
    /// Makes the next indentation
    /// </summary>
    /// <returns>Indentation String</returns>
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
