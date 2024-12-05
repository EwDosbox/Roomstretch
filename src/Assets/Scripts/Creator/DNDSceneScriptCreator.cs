using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DNDSceneScriptCreator : MonoBehaviour
{
    [SerializeField]
    public UtilityScript utilityScript;
    private string dNDFilePath = "";

    private void Awake()
    {

        dNDFilePath = utilityScript.filePath;

        if(string.IsNullOrEmpty(dNDFilePath))
        {
            Debug.Log("Creator: Badly made DND File Path");
        }
        else
        {
            
        }
    }
    #region Reading Lines
    private string ReadLine()
    {
        return " ";
    }
    #endregion
    
}
