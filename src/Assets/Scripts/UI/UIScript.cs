using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using System.Runtime.InteropServices;
using System;

public class UIScript : MonoBehaviour
{
    [DllImport("__Internal")] private static extern void DownloadFile(string data, string filename);
    [DllImport("__Internal")] public static extern string BrowserTextUpload(string extFilter, string gameObjName, string dataSinkFn);

    [SerializeField] private DNDFileData fileData;

    private DNDFileScriptCreator dNDFileScriptCreator;
    private PlayerInputScript playerInputScript;
    private Canvas canvas;
    private PlayerInput playerInput;

    private Toggle toggleNoOfRooms;
    private GameObject GONoOfRoomsInput;
    private Toggle toggleNoOfObjects;
    private GameObject GONoOfObjectsInput;

    private Toggle roomBoundsToggle;
    private Toggle mapBoundsToggle;
    private GameObject[] GORoomsBoundsInput;
    private GameObject[] GOMapBoundsInput;
    private GameObject player;
    private GameObject canvasGO;
    private GameObject toggles;
    private GameObject creator;
    private GameObject errorGO;

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
                    GONoOfObjectsInput = GameObject.Find("NoOfObjectsInput");

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
                    go = GameObject.Find("NoOfObjectsToggle");
                    toggleNoOfObjects = go.GetComponent<Toggle>();

                    dNDFileScriptCreator = creator.GetComponent<DNDFileScriptCreator>();
                    errorGO = GameObject.Find("ErrorText");
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

                    if (!toggleNoOfObjects.isOn)
                    {
                        GONoOfObjectsInput.SetActive(true);
                    }
                    else
                    {
                        GONoOfObjectsInput.SetActive(false);
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

        try
        {
            AssignGenerationBounds(fileData.Save.RoomsCountBounds, "NoOfRooms");
            AssignGenerationBounds(fileData.Save.ObjectCountBounds, "NoOfObjects");

            AssignBounds(fileData.Save.XRoomBounds, "RoomBounds", "MinRoomX", "MaxRoomX");
            AssignBounds(fileData.Save.ZRoomBounds, "RoomBounds", "MinRoomZ", "MaxRoomZ");
            AssignBounds(fileData.Save.XMapBounds, "MapBounds", "MinMapX", "MaxMapX");
            AssignBounds(fileData.Save.ZMapBounds, "MapBounds", "MinMapZ", "MaxMapZ");
        }
        catch (FormatException e)
        {
            errorGO.SetActive(true);
            errorGO.GetComponent<TextMeshProUGUI>().text = e.Message.ToString();
            return;
        }

        dNDFileScriptCreator.PrepareSave(fileData);

        Debug.Log(fileData.Save.ToString());

        dNDFileScriptCreator.CreateFile(fileData);

        NextScene();
    }

    public void DownloadButton_Click()
    {
        string dataToSave = File.ReadAllText(fileData.Save.FilePath);

        Debug.Log(dataToSave);

#if UNITY_WEBGL && !UNITY_EDITOR
        DownloadFile(dataToSave, "Save.dnd");
#else
        Debug.Log("Download not supported in this platform");
#endif

    }

    public void UploadButton_Click()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        BrowserTextUpload(".dnd", gameObject.name, "OnFileUploaded");
#else
        Debug.Log("Upload not supported in this platform");
#endif
    }

    public void NextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(SceneManager.GetActiveScene().buildIndex));
    }

    public void PreviousScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(SceneManager.GetActiveScene().buildIndex));
    }

    public void UpdateFOV(float value)
    {
        fileData.Settings.FOV = value;
    }
    public void UpdateSensitivity(float value)
    {
        fileData.Settings.Sensitivity = value;
    }
    public void UpdateMenu(bool value)
    {
        playerInputScript.ShouldBeInMenu = value;
    }
    #endregion
    #region Helper Methods
    public void OnFileUploaded(string str)
    {
        Debug.Log(str);

        File.WriteAllText(fileData.Save.FilePath, str);

        NextScene();
    }

    private void AssignGenerationBounds(GenerationBounds<int> bounds, string name)
    {
        try
        {
            bool shouldGenerate = GetToggle(name).isOn;
            if (!shouldGenerate)
            {
                bounds.Value = int.Parse(GetInput("NoOfRooms").Trim());
            }
            bounds.ShouldUseDefaultValue = shouldGenerate;
        }
        catch (FormatException)
        {
            throw new FormatException($"{name} is not valid");
        }
    }
    private void AssignBounds(Bounds<float> bounds, string name, string min, string max)
    {
        try
        {
            bool shouldGenerate = GetToggle(name).isOn;
            if (!shouldGenerate)
            {
                bounds = new Bounds<float>(float.Parse(GetInput(min)), float.Parse(GetInput(max)));
            }
        }
        catch (FormatException)
        {
            throw new FormatException($"{name} is not valid");
        }
    }
    private string GetInput(string toFind)
    {
        var inputField = GameObject.Find(toFind + "Input").transform.GetChild(0).GetComponent<TMP_InputField>();
        return inputField.text;
    }

    private Toggle GetToggle(string ToFind)
    {
        Transform ToFindT = toggles.transform.Find(ToFind + "Toggle");
        Toggle toFindTOggle = ToFindT.GetComponent<Toggle>();
        return toFindTOggle;
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
    #endregion
}
