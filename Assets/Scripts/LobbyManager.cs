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
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("4_Main");
        }
    }
}
