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
}
