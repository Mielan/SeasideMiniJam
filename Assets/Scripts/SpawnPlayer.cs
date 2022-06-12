using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class SpawnPlayer : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public PhotonView view;

    public float spawnY = 10f;
    public Transform topLeftLimit, bottomRightLimit;

    void Start()
    {
        InstantiatePlayer();
    }

    public void InstantiatePlayer()
    {
        GameObject playerObj = PhotonNetwork.Instantiate(
            playerPrefab.name,
            new Vector3(Random.Range(topLeftLimit.position.x, bottomRightLimit.position.x), spawnY, Random.Range(topLeftLimit.position.z, bottomRightLimit.position.z)),
            Quaternion.identity
        );
    }

}
