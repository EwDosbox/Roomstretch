using System.Collections;
using System.Collections.Generic;
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
}