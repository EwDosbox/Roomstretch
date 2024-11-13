using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUIScript : MonoBehaviour
{
    /// <summary>
    /// Quits the game
    /// </summary>
    public void QuitButton_Click()
    {
        Debug.Log("Quitting aplication");
        Application.Quit();
    }

    /// <summary>
    /// Plays the next scene; MainMenu => GeneratingScene => LevelScene
    /// </summary>
    public void PlayButton_Click()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
