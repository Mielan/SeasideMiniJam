using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public GameObject loadingInfo;
    public GameObject enterNameUI;
    public Button[] alfabetButton;
    public Button continueButton;
    public TextMeshProUGUI txtName;
    string playerName = "";


    void Start()
    {
        enterNameUI.SetActive(true);
        continueButton.gameObject.SetActive(false);
        loadingInfo.SetActive(false);
        txtName.text = "";

        continueButton.onClick.AddListener(() => { EnterLobby(); });

        for (int i = 0; i < alfabetButton.Length; i++)
        {
            int index = i;

            alfabetButton[i].onClick.AddListener(() => { OnClickAlfabet(index); });
        }
    }

    void OnClickAlfabet(int i)
    {
        playerName += alfabetButton[i].GetComponent<TextMeshProUGUI>().text;
        txtName.text = playerName;
        if (playerName.Length >= 3)
        {
            continueButton.gameObject.SetActive(true);
        }
    }

    void EnterLobby()
    {
        PhotonNetwork.NickName = playerName;
        enterNameUI.SetActive(false);
        loadingInfo.SetActive(true);
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        SceneManager.LoadScene("3_Lobby");
    }
}
