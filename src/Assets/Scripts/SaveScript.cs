using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SaveScript", menuName = "ScriptableObjects/SaveScript", order = 1)]
public class SaveScript : ScriptableObject
{
    public readonly string version = "1.0";
    public string seed;
    public string filePath;

    public DNDFileData DNDFileData;

    public float FOV = 60f;
}
