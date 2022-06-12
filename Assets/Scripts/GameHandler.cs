using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class GameHandler : MonoBehaviourPunCallbacks, IPunObservable
{
    public int playerCount;
    public Color[] playerColor;
    [SerializeField] private GameObject[] playerScoreUI;
    [SerializeField] private TextMeshProUGUI[] txtName;
    [SerializeField] private Image[] playerImage;
    [SerializeField] private TextMeshProUGUI[] txtScore;

    public int[] playerScore = new int[4];
    public string[] playerName = new string[4];

    public float spawnY = 10f;
    public Transform topLeftLimit, bottomRightLimit;

    public static GameHandler GM;
    public GameObject volleyBall;

    bool timeIsRunning;
    public TextMeshProUGUI txtTime;
    public int timeLimit = 120;
    int time;

    public PhotonView view;
    public GameObject[] player = new GameObject[4];
    public List<GameObject> ball;

    private void Awake()
    {
        if (GM == null)
        {
            GM = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            time = timeLimit;
            StartGameTime();
        }
    }

    void StartGameTime()
    {
        if (!timeIsRunning)
        {
            timeIsRunning = true;
            StartCoroutine(StartGameIE());
        }
    }
    IEnumerator StartGameIE()
    {
        //AddVolleyBall();
        //AddVolleyBall();
        while (time > 0)
        {
            txtTime.text = time.ToString();
            yield return new WaitForSeconds(1f);
            time--;
            AddVolleyBall();
        }
    }

    public void AddVolleyBall()
    {
        GameObject volleyObj = PhotonNetwork.Instantiate(
            volleyBall.name,
            new Vector3(Random.Range(topLeftLimit.position.x, bottomRightLimit.position.x), spawnY, Random.Range(topLeftLimit.position.z, bottomRightLimit.position.z)),
            Quaternion.identity
        );
        ball.Add(volleyObj);
    }

    public void ShowPlayerScore()
    {
        for (int i = 0; i < playerScoreUI.Length; i++)
        {
            if(i < playerCount)
            {
                txtScore[i].text = playerScore[i].ToString();
            }
            playerScoreUI[i].SetActive(i < playerCount);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            // send stats
            stream.SendNext(time);
        }
        else
        {
            time = (int)stream.ReceiveNext();
            txtTime.text = time.ToString();
        }
    }

}
