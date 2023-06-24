using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private string saveGamePath = "Default";

    public string SaveGamePath
    {
        get { return saveGamePath; }
        set { saveGamePath = value; }
    }

    public delegate void OnMapSceneLoaded(string saveGamePath);
    public static event OnMapSceneLoaded sendMapData;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void LaunchGame()
    {
        SaveData gameData = new SaveData();
        gameData.LoadFromFile(saveGamePath);

        StartCoroutine(LoadMapSceneAsync(1));
   
    }

    IEnumerator LoadMapSceneAsync(int sceneID)
    {
        AsyncOperation loadingScene = SceneManager.LoadSceneAsync(sceneID);

        while (!loadingScene.isDone)
        {

            yield return null;
        }

        sendMapData?.Invoke(SaveGamePath);

    }

}
