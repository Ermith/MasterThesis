using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuCotnroller : MonoBehaviour
{
    public void Resume()
    {
        GameController.Instance.Resume();
    }

    public void NewGame()
    {
        SceneManager.LoadSceneAsync("GameScene");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
