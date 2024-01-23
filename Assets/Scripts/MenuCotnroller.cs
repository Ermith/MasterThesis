using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuCotnroller : MonoBehaviour
{
    public void Resume()
    {
        GameController.Resume();
    }

    public void NewGame()
    {
        GameController.NewGame();
    }

    public void MainMenu()
    {
        GameController.MainMenu();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
