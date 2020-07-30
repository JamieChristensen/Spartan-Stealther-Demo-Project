using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public KeyCode ReloadSceneKey, LoadNextLevelKey;
    // Update is called once per frame
    void Update()
    {
        //For testing purposes only:
        if (Input.GetKeyDown(ReloadSceneKey))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        if (Input.GetKeyDown(LoadNextLevelKey))
        {
            SceneManager.LoadScene((SceneManager.GetActiveScene().buildIndex + 1) % (SceneManager.sceneCountInBuildSettings));
        }
    }
}
