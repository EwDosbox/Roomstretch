using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "UtilityScript", menuName = "ScriptableObjects/UtilityScript", order = 1)]
public class UtilityScript : ScriptableObject
{
    public string version = "1.0";
    public string filePath = "d:\\_GIT\\Roomstretch\\DND\\";

    public bool isPrime(int x)
    {
        if (x <= 1) return false;
        if (x % 2 == 0 && x != 2) return false;
        int sqrtX = (int)Mathf.Sqrt(x);
        for (int i = 3; i < sqrtX; i += 2)
        {
            if (x % i == 0) return false;
        }
        return true;
    }
    #region Write Into Files
    /// <summary>
    /// Writes a tag that starts and ends e.g. <Seed> 123 </Seed>
    /// </summary>
    /// <param name="tag">Name of the Tag to write e.g. </param>
    /// <param name="contents">Contents of the tag e.g. 123</param>
    public void WriteStartingEndingTag(string tag, string contents, int degreeOfDepth)
    {
        string toWrite = Indentation(degreeOfDepth) + "<" + tag + "> " + contents + " </" + tag + ">";
        File.AppendAllText(filePath, toWrite);
    }
    /// <summary>
    /// Writes a tag that starts e.g. <Head>
    /// </summary>
    /// <param name="tag">Name of the Tag to write</param>
    public void WriteStartingTag(string tag, int degreeOfDepth)
    {
        string toWrite = Indentation(degreeOfDepth) + "<" + tag + ">";
        File.AppendAllText(filePath, toWrite);
    }
    /// <summary>
    /// Writes a tag that ends </Head>
    /// </summary>
    /// <param name="tag">Name of the Tag to write</param>
    public void WriteEndingTag(string tag, int degreeOfDepth)
    {
        string toWrite = Indentation(degreeOfDepth) + "</" + tag + ">";
        File.AppendAllText(filePath, toWrite);
    }
    /// <summary>
    /// Writes a New Line to the file
    /// </summary>
    public void WriteNewline()
    {
        string toWrite = "\n";
        File.AppendAllText(filePath, toWrite);
    }
    /// <summary>
    /// Makes the next indentation
    /// </summary>
    /// <returns>Indentation String</returns>
    private string Indentation(int degreeOfDepth)
    {
        string indentation = "";

        for (int i = 0; i < degreeOfDepth; i++)
        {
            indentation += "    ";
        }

        return indentation;
    }
    #endregion
}