﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class MainMenuSpaceShooter : MonoBehaviour
{
    
    public void PlayGame()
    {
        SceneManager.LoadScene("EasyLevel");
    }
 
}
