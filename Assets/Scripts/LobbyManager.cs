using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public string[] roomName = new string[] {
        "BattleGround1",
        "BattleGround2",
        "BattleGround3",
        "BattleGround4",
    };

    public string battleInProgressInfo = "Battle in Progress";
    public string numberPlayerText = "Total Player : ";
    public string noPlayerInfo = "No Player";

    public TextMeshProUGUI[] txtRoomInfo;
    public Button[] roomButton;

    public Button playButton;
    public TextMeshProUGUI txtWaiting;

    ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();

    public GameObject roomSelectionUI;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        AssignMenuClick();
    }

    void AssignMenuClick()
    {
        for (int i = 0; i < roomButton.Length; i++)
        {
            int index = 1;
            roomButton[i].onClick.RemoveAllListeners();
            roomButton[i].onClick.AddListener(()=> { EnterBattleGround(index); });
        }

        playButton.onClick.AddListener(() => { OnPlayButton(); });
    }

    public void EnterBattleGround(int index)
    {
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 4
        };
        PhotonNetwork.JoinOrCreateRoom(roomName[index], roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        playerProperties["charID"] = PhotonNetwork.CurrentRoom.PlayerCount - 1;
        playerProperties["name"] = PhotonNetwork.NickName;
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
        roomSelectionUI.SetActive(false);
        txtWaiting.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >= 2)
        {
            txtWaiting.gameObject.SetActive(false);
            playButton.gameObject.SetActive(true);
        }
        else
        {
            playButton.gameObject.SetActive(false);
        }
    }

    void OnPlayButton()
    {
        PhotonNetwork.LoadLevel("4_Main");
    }
}
