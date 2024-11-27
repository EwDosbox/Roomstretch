using System.Collections;
using System.Collections.Generic;
using TMPro;
using SFB;
using UnityEngine;
using System.IO;

public class CreatingUIScript : MonoBehaviour
{
    private DNDFileScriptCreator dNDFileScriptCreator;
    private GameObject inputs;
    private void Awake()
    {
        GameObject creator = GameObject.Find("Creator");
        inputs = GameObject.Find("Inputs");

        //Gets the instnace of the file script
        dNDFileScriptCreator = creator.GetComponent<DNDFileScriptCreator>();
    }
    /// <summary>
    /// Submits the contensts of the Inputs and gives them to the creator
    /// </summary>
    public void SubmitButton_Click()
    {

        string seed = GetInput("Seed") ;
        //Creates the DND File
        dNDFileScriptCreator.CreateFile(seed);

        // Open save file dialog
        string dNDFilePath = StandaloneFileBrowser.SaveFilePanel("Save Your .DND File", "", "", "dnd");

        if (!string.IsNullOrEmpty(dNDFilePath))
        {
            File.Copy(dNDFileScriptCreator.FilePath, dNDFilePath, true);
            File.Delete(dNDFileScriptCreator.FilePath);
            Debug.Log("File: .dnd was created at " + dNDFilePath);
        }
    }

    private string GetInput(string ToFind)
    {
        GameObject ToFindGO = inputs.transform.Find(ToFind + "Input").gameObject;
        GameObject ToFindInputGO = ToFindGO.transform.Find(ToFind +"InputField").gameObject;
        TMP_InputField ToFindTMPInputField = ToFindInputGO.GetComponent<TMP_InputField>();
        return ToFindTMPInputField.text;
    }
}
