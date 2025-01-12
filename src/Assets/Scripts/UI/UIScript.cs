using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using TMPro;
using SFB;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class UIScript : MonoBehaviour
{
    [SerializeField]
    private SaveScript save;

    private DNDFileScriptCreator dNDFileScriptCreator;
    private PlayerInputScript playerInputScript;
    private Canvas canvas;
    private PlayerInput playerInput;

    private GameObject player;
    private GameObject canvasGO;
    private GameObject inputs;
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
        if (SceneManager.GetActiveScene().buildIndex == 2)
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
        }
    }

    #region UI

    public void SubmitButton_Click()
    {
        string seed = GetInput("Seed");
        save.seed = seed;
        
        PrepareSave(save);
        dNDFileScriptCreator.CreateFile(save);

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

    private void PrepareSave(SaveScript save)
    {
        DNDFileData data = new DNDFileData(save.version, save.seed);


        Vector3 size = new Vector3(1, 2, 3);
        Vector3 position = new Vector3(3, 2, 1);

        List<DoorData> doors = new List<DoorData>();
        List<ObjectData> objects = new List<ObjectData>();

        data.AddRoom(size, position, doors, objects);

        save.DNDFileData = data;
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
        save.FOV = value;
    }
    #endregion

    private string GetInput(string ToFind)
    {
        GameObject ToFindGO = inputs.transform.Find(ToFind + "Input").gameObject;
        GameObject ToFindInputGO = ToFindGO.transform.Find(ToFind + "InputField").gameObject;
        TMP_InputField ToFindTMPInputField = ToFindInputGO.GetComponent<TMP_InputField>();
        return ToFindTMPInputField.text;
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
