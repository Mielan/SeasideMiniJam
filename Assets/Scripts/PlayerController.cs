using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    Rigidbody rb;
    [SerializeField] private float speed = 200f;
    [SerializeField] private float turnSmoothTime = 0.1f;
    [SerializeField] private float jumpForce;
    private float turnSmoothVelocity;
    float x, z;
    Vector3 direction;

    public bool canGrabVolleyBall = false;
    public bool holdingVolleyBall = false;
    [SerializeField] private Transform volleyBallPos;
    GameObject volleyBall;

    public GameHandler handler;
    public int playerIndex;

    PhotonView view;
    public GameObject ballInHand;
    public GameObject volleyPrefab;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        view = GetComponent<PhotonView>();

        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        playerIndex = playerCount - 1;
        handler = GameHandler.GM;
        GameHandler.GM.playerCount = playerCount;
        GameHandler.GM.ShowPlayerScore();
    }

    private void Update()
    {
        if (view.IsMine)
        {
            x = Input.GetAxisRaw("Horizontal");
            z = Input.GetAxisRaw("Vertical");
            direction = new Vector3(x, 0, z).normalized;
            if (Input.GetButtonDown("Jump"))
            {
                rb.velocity = Vector3.up * jumpForce;
            }

            if (Input.GetButtonDown("Fire1"))
            {
                if (canGrabVolleyBall)
                {
                    StartCoroutine(GrabBallIE());
                }

                if (holdingVolleyBall)
                {
                    StartCoroutine(ThrowBallIE());
                }
            }

        }

        if (holdingVolleyBall && volleyBall != null)
        {
            volleyBall.transform.position = volleyBallPos.position;
        }

        ballInHand.SetActive(holdingVolleyBall);
    }

    IEnumerator GrabBallIE()
    {
        if(volleyBall.GetComponent<PhotonView>().IsMine)
        {
            PhotonNetwork.Destroy(volleyBall);
        }
        else
        {
            view.RPC("DestroyBall", RpcTarget.All, volleyBall.GetComponent<VolleyBallCollider>().index);
        }
        yield return null;
        canGrabVolleyBall = false;
        holdingVolleyBall = true;
    }

    [PunRPC]
    public void DestroyBall(int ballIndex)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameHandler.GM.ball.RemoveAt(ballIndex);
            PhotonNetwork.Destroy(GameHandler.GM.ball[ballIndex]);
        }
    }

    IEnumerator ThrowBallIE()
    {
        holdingVolleyBall = false;
        GameObject ball = PhotonNetwork.Instantiate(volleyPrefab.name, volleyBallPos.position, Quaternion.identity);
        ball.GetComponent<VolleyBallCollider>().BallOnPlayerHand(playerIndex);
        yield return 0.05f;
        ball.GetComponent<Rigidbody>().AddForce((transform.forward + new Vector3(0, 0.25f, 0)) * 800);
        volleyBall = null;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    void MovePlayer()
    {
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0, angle, 0);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward * speed * Time.deltaTime;
            rb.velocity = new Vector3(moveDir.x, rb.velocity.y, moveDir.z);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball") && !holdingVolleyBall)
        {
            canGrabVolleyBall = true;
            volleyBall = other.gameObject;
        }

        if (other.CompareTag("SeaShell"))
        {
            handler.playerScore[playerIndex]++;
            handler.ShowPlayerScore();
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ball") && !holdingVolleyBall)
        {
            canGrabVolleyBall = false;
            volleyBall = null;
        }
    }


    public void RespawnPlayer()
    {
        view.RPC("ResetPlayer", RpcTarget.All);
    }


    // get called on  all instance of the viewID
    [PunRPC]
    public void ResetPlayer()
    {
        if (handler == null)
        {
            handler = GameHandler.GM;
        }
        transform.position = new Vector3(Random.Range(handler.topLeftLimit.position.x, handler.bottomRightLimit.position.x),
            handler.spawnY, Random.Range(handler.topLeftLimit.position.z, handler.bottomRightLimit.position.z));
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            // sync data
            stream.SendNext(playerIndex);
            stream.SendNext(holdingVolleyBall);
            stream.SendNext(canGrabVolleyBall);
            if (GetComponent<Renderer>().material.color != GameHandler.GM.playerColor[playerIndex])
            {
                GetComponent<Renderer>().material.color = GameHandler.GM.playerColor[playerIndex];
                GameHandler.GM.player[playerIndex] = gameObject;
                Debug.Log("Run");
            }
        }
        else
        {
            //we are reading
            playerIndex = (int)stream.ReceiveNext();
            holdingVolleyBall = (bool)stream.ReceiveNext();
            canGrabVolleyBall = (bool)stream.ReceiveNext();
            if (GetComponent<Renderer>().material.color != GameHandler.GM.playerColor[playerIndex])
            {
                GetComponent<Renderer>().material.color = GameHandler.GM.playerColor[playerIndex];
                GameHandler.GM.player[playerIndex] = gameObject;
                Debug.Log("Run");
            }
        }
    }
}
