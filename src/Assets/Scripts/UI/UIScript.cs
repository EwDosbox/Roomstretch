using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using TMPro;
using SFB;

public class UIScript : MonoBehaviour
{
    private DNDFileScriptCreator dNDFileScriptCreator;
    private GameObject inputs;
    private GameObject creator;
    private void Awake()
    {
        try
        {
            creator = GameObject.Find("Creator");
            inputs = GameObject.Find("Inputs");

            //Gets the instnace of the file script
            dNDFileScriptCreator = creator.GetComponent<DNDFileScriptCreator>();
        }
        catch
        {
            Debug.Log("UI: Didnt find all the objects");
        }
    }

    #region Buttons
    /// <summary>
    /// Submits the contensts of the Inputs and gives them to the creator
    /// </summary>
    public void SubmitButton_Click()
    {
        string seed = GetInput("Seed");
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

        NextScene();
    }

    /// <summary>
    /// Plays the next scene; MainMenu => GeneratingScene => LevelScene
    /// </summary>
    public void NextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    /// <summary>
    /// Plays the previous scene; MainMenu => GeneratingScene => LevelScene
    /// </summary>
    public void PreviousScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
    #endregion

    /// <summary>
    /// Gets the input from the input field
    /// </summary>
    /// <param name="ToFind">Name of the InputField to Find</param>
    /// <returns>Input from the InputField</returns>
    private string GetInput(string ToFind)
    {
        GameObject ToFindGO = inputs.transform.Find(ToFind + "Input").gameObject;
        GameObject ToFindInputGO = ToFindGO.transform.Find(ToFind + "InputField").gameObject;
        TMP_InputField ToFindTMPInputField = ToFindInputGO.GetComponent<TMP_InputField>();
        return ToFindTMPInputField.text;
    }
}
