using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UtilityScript : MonoBehaviour
{
    public static bool isPrime(int x)
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

    private static string Indentation(int degreeOfDepth)
    {
        return new string(' ', degreeOfDepth * 4);
    }

    #region Write Into Files
    /// <summary>
    /// Writes a tag that starts and ends e.g. <Seed> 123 </Seed>
    /// </summary>
    /// <param name="tag">Name of the Tag to write e.g. </param>
    /// <param name="contents">Contents of the tag e.g. 123</param>
    public static void WriteStartingEndingTag(string tag, string contents, int degreeOfDepth, SaveScript save)
    {
        string toWrite = Indentation(degreeOfDepth) + "<" + tag + "> " + contents + " </" + tag + ">";
        File.AppendAllText(save.filePath, toWrite);
    }

    /// <summary>
    /// Writes a tag that starts e.g. <Head>
    /// </summary>
    /// <param name="tag">Name of the Tag to write</param>
    public static void WriteStartingTag(string tag, int degreeOfDepth, SaveScript save)
    {
        string toWrite = Indentation(degreeOfDepth) + "<" + tag + ">";
        File.AppendAllText(save.filePath, toWrite);
    }

    /// <summary>
    /// Writes a tag that ends </Head>
    /// </summary>
    /// <param name="tag">Name of the Tag to write</param>
    public static void WriteEndingTag(string tag, int degreeOfDepth, SaveScript save)
    {
        string toWrite = Indentation(degreeOfDepth) + "</" + tag + ">";
        File.AppendAllText(save.filePath, toWrite);
    }

    /// <summary>
    /// Writes a New Line to the file
    /// </summary>
    public static void WriteNewline(SaveScript save)
    {
        string toWrite = "\n";
        File.AppendAllText(save.filePath, toWrite);
    }
    #endregion

    #region Read From Files
    public static string[] ReadIndentation(int targetIndentation, SaveScript save)
    {
        string[] lines = File.ReadAllLines(save.filePath);
        string indentation = new string(' ', targetIndentation * 4);
        List<string> matchingLines = new List<string>();

        foreach (string line in lines)
        {
            if (line.StartsWith(indentation))
            {
                matchingLines.Add(line.Trim()); // Add the trimmed line content to the list
            }
        }

        return matchingLines.ToArray();
    }
    #endregion
}
