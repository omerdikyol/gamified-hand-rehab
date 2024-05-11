using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;


public class HandCanvasCullingMask : MonoBehaviour
{

    int[] canvasNeededScenes = {1,2,3,4,5,6,7,8,9,10,11,12};

    // When entering a new scene, if current scene index is not the given scenes, disable canvas, else enable
    private void OnEnable() {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1) {
        int index = SceneManager.GetActiveScene().buildIndex;
        if (!canvasNeededScenes.Contains(index)) {
            if (GetComponent<Canvas>().enabled) {
                Debug.Log("Disabling Canvas");
                GetComponent<Canvas>().enabled = false;
            }
        } else {
            if (!GetComponent<Canvas>().enabled) {
                Debug.Log("Enabling Canvas");
                GetComponent<Canvas>().enabled = true;
            }
        }
    }
}
