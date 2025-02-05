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

    private Toggle toggleBounds;
    private GameObject GOBoundsInput;

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
                    GOBoundsInput = GameObject.Find("BoundsInput");

                    GameObject go = GameObject.Find("NoOfRoomsToggle");
                    toggleNoOfRooms = go.GetComponent<Toggle>();

                    go = GameObject.Find("BoundsToggle");
                    toggleBounds = go.GetComponent<Toggle>();

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
                    if (toggleBounds.isOn)
                    {
                        GOBoundsInput.SetActive(false);
                    }
                    else
                    {
                        GOBoundsInput.SetActive(true);
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
        fileData.Save = new Save();

        string seed = GetInput("Seed");
        fileData.Save.Seed = seed;


        fileData.Save.RoomsBounds.ShouldGenerate = GetToggle("NoOfRooms").isOn;
        if (!fileData.Save.RoomsBounds.ShouldGenerate)
        {
            fileData.Save.RoomsBounds.NoOfGenerations = int.Parse(GetInput("NoOfRooms").Trim());
        }

        fileData.Save.MapDepthBounds.ShouldGenerate = GetToggle("Bounds").isOn;
        if (!fileData.Save.MapDepthBounds.ShouldGenerate)
        {
            fileData.Save.MapDepthBounds.MaxBounds = (float.Parse(GetInput("MaxDepth")), float.Parse(GetInput("MaxWidth")));
            fileData.Save.MapDepthBounds.MinBounds = (float.Parse(GetInput("MinDepth")), float.Parse(GetInput("MinWidth")));
        }

        fileData.Save.MapWidthBounds.ShouldGenerate = GetToggle("Bounds").isOn;
        if (!fileData.Save.MapWidthBounds.ShouldGenerate)
        {
            fileData.Save.MapWidthBounds.MaxBounds = (float.Parse(GetInput("MaxDepth")), float.Parse(GetInput("MaxWidth")));
            fileData.Save.MapWidthBounds.MinBounds = (float.Parse(GetInput("MinDepth")), float.Parse(GetInput("MinWidth")));
        }

        dNDFileScriptCreator.PrepareSave(fileData);

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

    private string GetInput(string toFind)
    {
        var inputField = GameObject.Find(toFind + "InputField").GetComponent<TMP_InputField>();
        return inputField.text;
    }


    private Toggle GetToggle(string ToFind)
    {
        Transform ToFindT = toggles.transform.Find(ToFind + "Toggle");
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
