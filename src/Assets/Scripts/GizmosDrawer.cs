using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GizmosDrawer : MonoBehaviour
{
    [SerializeField] private DNDFileData fileData;

    private void Awake()
    {
        fileData = AssetDatabase.LoadAssetAtPath<DNDFileData>("Assets/Scripts/Data.asset");
    }

    private void OnDrawGizmos()
    {

    }

}
