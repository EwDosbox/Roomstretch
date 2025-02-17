using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class UIScript : MonoBehaviour
{
    [SerializeField]
    private DNDFileData fileData;

    private DNDFileScriptCreator dNDFileScriptCreator;
    private PlayerInputScript playerInputScript;
    private Canvas canvas;
    private PlayerInput playerInput;

    private Toggle toggleNoOfRooms;
    private Toggle toggleNoOfDoors;
    private GameObject GONoOfRoomsInput;
    private GameObject GONoOfDoorsInput;

    private Toggle roomBoundsToggle;
    private Toggle mapBoundsToggle;
    private GameObject[] GORoomsBoundsInput;
    private GameObject[] GOMapBoundsInput;
    private GameObject player;
    private GameObject canvasGO;
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
                    toggles = GameObject.Find("Toggles");
                    GONoOfRoomsInput = GameObject.Find("NoOfRoomsInput");
                    GONoOfDoorsInput = GameObject.Find("NoOfDoorsInput");

                    GameObject go = GameObject.Find("RoomBoundsToggle");
                    roomBoundsToggle = go.GetComponent<Toggle>();
                    GameObject maxXInput = GameObject.Find("MaxRoomXInput");
                    GameObject minXInput = GameObject.Find("MinRoomXInput");
                    GameObject maxZInput = GameObject.Find("MaxRoomZInput");
                    GameObject minZInput = GameObject.Find("MinRoomZInput");
                    GORoomsBoundsInput = new GameObject[] { maxXInput, maxZInput, minXInput, minZInput };

                    go = GameObject.Find("MapBoundsToggle");
                    mapBoundsToggle = go.GetComponent<Toggle>();
                    maxXInput = GameObject.Find("MaxMapXInput");
                    minXInput = GameObject.Find("MinMapXInput");
                    maxZInput = GameObject.Find("MaxMapZInput");
                    minZInput = GameObject.Find("MinMapZInput");
                    GOMapBoundsInput = new GameObject[] { maxXInput, maxZInput, minXInput, minZInput };

                    go = GameObject.Find("NoOfRoomsToggle");
                    toggleNoOfRooms = go.GetComponent<Toggle>();

                    go = GameObject.Find("NoOfDoorsToggle");
                    toggleNoOfDoors = go.GetComponent<Toggle>();


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
                    if(!toggleNoOfDoors.isOn)
                    {
                        GONoOfDoorsInput.SetActive(true);
                    }
                    else
                    {
                        GONoOfDoorsInput.SetActive(false);
                    }
                    if (roomBoundsToggle.isOn)
                    {
                        foreach (GameObject go in GORoomsBoundsInput)
                        {
                            go.SetActive(false);
                        }
                    }
                    else
                    {
                        foreach (GameObject go in GORoomsBoundsInput)
                        {
                            go.SetActive(true);
                        }
                    }
                    if (mapBoundsToggle.isOn)
                    {
                        foreach (GameObject go in GOMapBoundsInput)
                        {
                            go.SetActive(false);
                        }
                    }
                    else
                    {
                        foreach (GameObject go in GOMapBoundsInput)
                        {
                            go.SetActive(true);
                        }
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
        fileData.Initialize();

        fileData.Save.Seed = GetInput("Seed");


        bool shouldGenerate = GetToggle("NoOfRooms").isOn;
        if (!shouldGenerate)
        {
            fileData.Save.RoomsCountBounds.Value = int.Parse(GetInput("NoOfRooms").Trim());
        }
        fileData.Save.RoomsCountBounds.ShouldUseDefaultValue = shouldGenerate;

        shouldGenerate = GetToggle("NoOfDoors").isOn;
        if (!shouldGenerate)
        {
            fileData.Save.DoorCountBounds.Value = int.Parse(GetInput("NoOfDoors").Trim());
        }
        fileData.Save.DoorCountBounds.ShouldUseDefaultValue = shouldGenerate;

        shouldGenerate = GetToggle("RoomBounds").isOn;
        if (!shouldGenerate)
        {
            fileData.Save.XRoomBounds = new Bounds<float>(float.Parse(GetInput("MinRoomX")), float.Parse(GetInput("MaxRoomX")));
            fileData.Save.ZRoomBounds = new Bounds<float>(float.Parse(GetInput("MinRoomZ")), float.Parse(GetInput("MaxRoomZ")));
        }

        shouldGenerate = GetToggle("MapBounds").isOn;
        if (!shouldGenerate)
        {
            fileData.Save.XMapBounds = new Bounds<float>(float.Parse(GetInput("MinMapX")), float.Parse(GetInput("MaxMapX")));
            fileData.Save.ZMapBounds = new Bounds<float>(float.Parse(GetInput("MinMapZ")), float.Parse(GetInput("MaxMapZ")));
        }

        dNDFileScriptCreator.PrepareSave(fileData);

        Debug.Log(fileData.Save.ToString());

        dNDFileScriptCreator.CreateFile(fileData);

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
        var inputField = GameObject.Find(toFind + "Input").transform.GetChild(0).GetComponent<TMP_InputField>();
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
