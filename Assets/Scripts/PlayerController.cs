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
    string playerName;

    PhotonView view;
    public int playerScore;
    public GameObject ballInHand;
    public GameObject volleyPrefab;

    float distToGround = 0;
    int jumpCount = 0;
    public int maxJump = 2;


    private void Start()
    {
        distToGround = GetComponent<Collider>().bounds.extents.y;
        rb = GetComponent<Rigidbody>();
        view = GetComponent<PhotonView>();

        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        playerIndex = (int)PhotonNetwork.LocalPlayer.CustomProperties["charID"];
        playerName = PhotonNetwork.LocalPlayer.CustomProperties["name"].ToString();
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
            if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Q))
            {
                if (IsGrounded())
                {
                    rb.velocity = Vector3.up * jumpForce;
                    jumpCount++;
                }
                else
                {
                    if(jumpCount < maxJump - 1)
                    {
                        rb.velocity = Vector3.up * jumpForce;
                        jumpCount++;
                    }
                }
            }

            if (IsGrounded())
            {
                jumpCount = 0;
            }

            if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
            {
                if (canGrabVolleyBall)
                {
                    StartCoroutine(GrabBallIE());
                }

                if (holdingVolleyBall)
                {
                    holdingVolleyBall = false;
                    StartCoroutine(ThrowBallIE());
                }
            }
        }
        ballInHand.SetActive(holdingVolleyBall);
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
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
            PhotonNetwork.Destroy(GameHandler.GM.ball[ballIndex]);
        }
    }

    IEnumerator ThrowBallIE()
    {
        GameObject ball = PhotonNetwork.Instantiate(volleyPrefab.name, volleyBallPos.position, Quaternion.identity);
        ball.GetComponent<VolleyBallCollider>().BallOnPlayerHand(playerIndex);
        ball.GetComponent<VolleyBallCollider>().thrower = gameObject;
        ball.GetComponent<Rigidbody>().AddForce((transform.forward + new Vector3(0, 0.25f, 0)) * 800);
        yield return null;
        ball.GetComponent<VolleyBallCollider>().isActive = true;
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
        view.RPC("ResetPlayer", RpcTarget.All, playerIndex);
    }


    // get called on  all instance of the viewID
    [PunRPC]
    public void ResetPlayer(int otherIndex)
    {
        if (handler == null)
        {
            handler = GameHandler.GM;
        }
        GameHandler.GM.playerScore[otherIndex] -= 3;
        if (GameHandler.GM.playerScore[otherIndex] < 0)
            GameHandler.GM.playerScore[otherIndex] = 0;
        GameHandler.GM.ShowPlayerScore();

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
            stream.SendNext(playerName);
            if (GetComponent<Renderer>().material.color != GameHandler.GM.playerColor[playerIndex])
            {
                GetComponent<Renderer>().material.color = GameHandler.GM.playerColor[playerIndex];
                GameHandler.GM.player[playerIndex] = gameObject;
                GameHandler.GM.playerName[playerIndex] = playerName;
                GameHandler.GM.ShowPlayerScore();
            }
        }
        else
        {
            //we are reading
            playerIndex = (int)stream.ReceiveNext();
            holdingVolleyBall = (bool)stream.ReceiveNext();
            playerName = (string)stream.ReceiveNext();
            if (GetComponent<Renderer>().material.color != GameHandler.GM.playerColor[playerIndex])
            {
                GetComponent<Renderer>().material.color = GameHandler.GM.playerColor[playerIndex];
                GameHandler.GM.player[playerIndex] = gameObject;
                GameHandler.GM.playerName[playerIndex] = playerName;
                GameHandler.GM.ShowPlayerScore();
            }
        }
    }
}
