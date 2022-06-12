using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class VolleyBallCollider : MonoBehaviourPunCallbacks, IPunObservable
{
    public int index;
    public bool isActive = false;
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
        thrower = GameHandler.GM.player[i];
        isActive = true;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(index);
        }
        else
        {
            index = (int)stream.ReceiveNext();
        }
    }
}
