using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartingMenu : MonoBehaviour
{
    [SerializeField] private Button[] menuButton;

    private void Start()
    {
        AssignClickableObjects();
    }

    private void AssignClickableObjects()
    {
        for (int i = 0; i < menuButton.Length; i++)
        {
            int index = i;
            menuButton[i].onClick.RemoveAllListeners();
            menuButton[i].onClick.AddListener(() => { OnMenuSelection(index); });
        }
    }

    private void OnMenuSelection(int index)
    {
        switch (index)
        {
            case 0:
                SceneManager.LoadScene("2_Loading");
                break;

            default:
                break;
        }
    }
}
