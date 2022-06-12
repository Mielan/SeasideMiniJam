using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class VolleyBallCollider : MonoBehaviourPunCallbacks, IPunObservable
{
    public int index;
    public bool isActive = false;
    public int playerIndex;
    public GameObject thrower;
    public PhotonView view;


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (isActive)
            {
                if (collision.gameObject != thrower)
                {
                    view.RPC("AddScore", RpcTarget.All);
                    collision.gameObject.GetComponent<PlayerController>().RespawnPlayer();
                }
            }
        }
    }

    public void BallOnPlayerHand(int i)
    {
        view.RPC("BallOnHand", RpcTarget.All, i);
    }

    [PunRPC]
    public void BallOnHand(int i)
    {
        playerIndex = i;
        thrower = GameHandler.GM.player[i];
    }

    [PunRPC]
    public void AddScore()
    {
        GameHandler.GM.playerScore[playerIndex] += 5;
        GameHandler.GM.ShowPlayerScore();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(index);
            stream.SendNext(isActive);
        }
        else
        {
            index = (int)stream.ReceiveNext();
            isActive = (bool)stream.ReceiveNext();
        }
    }
}
