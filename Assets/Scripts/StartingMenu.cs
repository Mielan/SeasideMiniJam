using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartingMenu : MonoBehaviour
{
    [SerializeField] private Button[] menuButton;
    public GameObject[] menuUI;

    private void Start()
    {
        menuUI[0].SetActive(true);
        menuUI[1].SetActive(false);
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

            case 1:
                menuUI[0].SetActive(false);
                menuUI[1].SetActive(true);
                break;

            case 2:
                menuUI[0].SetActive(true);
                menuUI[1].SetActive(false);
                break;

            default:
                break;
        }
    }
}
