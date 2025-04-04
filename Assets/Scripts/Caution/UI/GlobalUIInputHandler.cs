using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalUIInputHandler : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;

    private void Awake()
    {
        uiManager = FindObjectOfType<UIManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            uiManager?.TogglePauseMenu();
        }
    }
}
