using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using TMPro;
using SFB;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    [SerializeField]
    private DNDFileData fileData;

    private DNDFileScriptCreator dNDFileScriptCreator;
    private PlayerInputScript playerInputScript;
    private Canvas canvas;
    private PlayerInput playerInput;

    private Toggle toggleNoOfRooms;
    private GameObject GONoOfRoomsInput;

    private GameObject player;
    private GameObject canvasGO;
    private GameObject inputs;
    private GameObject toggles;
    private GameObject creator;
    private void Awake()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        switch (activeScene.buildIndex)
        {
            case 0:
                {//Main Menu
                    break;
                }
            case 1:
                {//Generating Scene
                    creator = GameObject.Find("Creator");
                    inputs = GameObject.Find("Inputs");
                    toggles = GameObject.Find("Toggles");
                    GONoOfRoomsInput = GameObject.Find("NoOfRoomsInput");

                    GameObject go = GameObject.Find("NoOfRoomsToggle");
                    Debug.Log(go.name);
                    toggleNoOfRooms = go.GetComponent<Toggle>();
                    Debug.Log(toggleNoOfRooms.name);

                    dNDFileScriptCreator = creator.GetComponent<DNDFileScriptCreator>();
                    break;
                }
            case 2:
                {//Level scene
                    creator = GameObject.Find("Creator");
                    player = GameObject.Find("Player");
                    canvasGO = GameObject.Find("Canvas");

                    canvas = canvasGO.GetComponent<Canvas>();
                    dNDFileScriptCreator = creator.GetComponent<DNDFileScriptCreator>();
                    playerInputScript = player.GetComponent<PlayerInputScript>();
                    playerInput = player.GetComponent<PlayerInput>();
                    break;
                }
        }

    }

    private void Update()
    {
        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 1:
                {
                    if (!toggleNoOfRooms.isOn)
                    {
                        GONoOfRoomsInput.SetActive(true);
                    }
                    else
                    {
                        GONoOfRoomsInput.SetActive(false);
                    }
                    break;
                }
            case 2:
                {//Level Scene
                    if (playerInputScript.ShouldBeInMenu)
                    {
                        canvas.enabled = true;
                        LockCursor(false);

                        playerInput.actions.FindActionMap("Movement").Disable();
                    }
                    else
                    {
                        canvas.enabled = false;
                        LockCursor(true);

                        playerInput.actions.FindActionMap("Movement").Enable();
                    }
                    break;
                }

        }
    }

    #region UI

    public void SubmitButton_Click()
    {
        string seed = GetInput("Seed");
        fileData.Save.Seed = seed;

        bool shouldGenRanNoOfRooms = GetToggle("NoOfRooms").isOn;
        int noOfRooms = int.Parse(GetInput("NoOfRooms").Trim());
        dNDFileScriptCreator.PrepareSave(fileData, shouldGenRanNoOfRooms, noOfRooms);

        Debug.Log(fileData.Save.ToString());

        dNDFileScriptCreator.CreateFile(fileData);

        string dNDFilePath = StandaloneFileBrowser.SaveFilePanel("Save Your .DND File", "", "", "dnd");

        dNDFilePath = EndsWithDND(dNDFilePath);

        if (!string.IsNullOrEmpty(dNDFilePath))
        {
            File.Copy(dNDFileScriptCreator.FilePath, dNDFilePath, true);
            File.Delete(dNDFileScriptCreator.FilePath);
            Debug.Log("File: .dnd was created at " + dNDFilePath);
        }

        NextScene();
    }

    public void NextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void PreviousScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void UpdateFOV(float value)
    {
        fileData.Settings.FOV = value;
    }
    #endregion

    private string GetInput(string ToFind)
    {
        GameObject ToFindGO = inputs.transform.Find(ToFind + "Input").gameObject;
        GameObject ToFindInputGO = ToFindGO.transform.Find(ToFind + "InputField").gameObject;
        TMP_InputField ToFindTMPInputField = ToFindInputGO.GetComponent<TMP_InputField>();
        return ToFindTMPInputField.text;
    }

    private Toggle GetToggle(string ToFind)
    {
        Transform ToFindT = toggles.transform.Find("NoOfRoomsToggle");
        Debug.Log(ToFindT.name);
        GameObject toFindGO = ToFindT.gameObject;
        Toggle toFindTOggle = ToFindT.GetComponent<Toggle>();
        return toFindTOggle;
    }

    private string EndsWithDND(string file)
    {
        if (file.EndsWith(".dnd") || file.EndsWith(".DND")) return file;
        return file + ".dnd";
    }

    public void LockCursor(bool shouldBeLocked)
    {
        if (shouldBeLocked)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
