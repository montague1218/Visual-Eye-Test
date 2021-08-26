using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuFunctions : MonoBehaviour {

    //public int[] sceneNumbers;
    //public string[] sceneNames;

    public void customizedLoadScene(int sceneNumbersForLoad)
    {
		SceneManager.LoadScene(sceneNumbersForLoad);
    }
     
    public void customizedLoadScene(string sceneNameForLoad)
    {
        SceneManager.LoadScene(sceneNameForLoad);
    }

    public void reloadScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void loadNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
