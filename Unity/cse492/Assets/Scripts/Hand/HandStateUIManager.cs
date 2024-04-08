using System;
using UnityEngine;
using UnityEngine.UI; // Make sure to include this for UI elements
using TMPro;

public class HandStateUIManager : MonoBehaviour
{
    [Header("References")]
    public InputController inputController;

    [Header("UI Elements")]
    public TextMeshProUGUI stateNameText;

    public void SetCurrentStateName(string stateName)
    {
        stateNameText.text = stateName;
    }
}
