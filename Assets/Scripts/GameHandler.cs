using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using DG.Tweening;
using UnityEngine.SceneManagement;

using UnityEngine.Networking;

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
    public GameObject seaShell;

    bool timeIsRunning;
    public TextMeshProUGUI txtTime;
    public int timeLimit = 120;
    int time;

    public PhotonView view;
    public GameObject[] player = new GameObject[4];
    public List<GameObject> ball;
    public TextMeshProUGUI winnerText;
    int playerWinner = 0;



    [SerializeField] private string URL = "https://sittingduckgames.com/Home/score/get_score.php";
    private PlayerScore scores;
    private string baseUtcOffset;


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
        System.TimeZoneInfo localZone = System.TimeZoneInfo.Local;
        baseUtcOffset = localZone.BaseUtcOffset.ToString();

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
        while (time > 0)
        {
            txtTime.text = time.ToString();
            yield return new WaitForSeconds(1f);
            time--;
            AddSeaShell();
            AddSeaShell();
            AddSeaShell();
            AddVolleyBall();
        }

        view.RPC("CheckWinner", RpcTarget.All);
        yield return new WaitForSeconds(1.5f);
        Camera.main.transform.LookAt(player[playerWinner].transform.position);
        yield return new WaitForSeconds(0.5f);
        view.RPC("LeaveRoom", RpcTarget.All);
    }

    [PunRPC]
    public void CheckWinner()
    {
        int max = 0;
        for (int i = 0; i < playerScore.Length; i++)
        {
            if (i < playerCount)
            {
                if(PhotonNetwork.IsMasterClient)
                {
                    if(playerScore[i] > 0)
                    {
                        StartCoroutine(AddNewScore(playerName[i], playerScore[i]));
                    }
                }
                player[i].GetComponent<PlayerController>().canMove = false;
                player[i].GetComponent<Rigidbody>().isKinematic = true;
                if (playerScore[i] >= max)
                {
                    max = playerScore[i];
                    playerWinner = i;
                }
            }
        }

        winnerText.text = "THE WINNER IS " + playerName[playerWinner];
        winnerText.color = playerColor[playerWinner];
        winnerText.gameObject.SetActive(true);
        Camera.main.transform.DOMove(player[playerWinner].GetComponent<PlayerController>().camPos.position, 1.5f).OnUpdate(()=> {
            Camera.main.transform.LookAt(player[playerWinner].transform.position);
        });
    }

    [PunRPC]
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom(true);
    }


    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("3_Lobby");
    }

    public void AddVolleyBall()
    {
        GameObject volleyObj = PhotonNetwork.Instantiate(
            volleyBall.name,
            new Vector3(Random.Range(topLeftLimit.position.x, bottomRightLimit.position.x), spawnY, Random.Range(topLeftLimit.position.z, bottomRightLimit.position.z)),
            Quaternion.identity
        );
        volleyObj.GetComponent<VolleyBallCollider>().index = ball.Count;
        ball.Add(volleyObj);
    }

    public void AddSeaShell()
    {
        PhotonNetwork.Instantiate(
            seaShell.name,
            new Vector3(Random.Range(topLeftLimit.position.x, bottomRightLimit.position.x), spawnY, Random.Range(topLeftLimit.position.z, bottomRightLimit.position.z)),
            Quaternion.identity
        );
    }

    public void ShowPlayerScore()
    {
        for (int i = 0; i < playerScoreUI.Length; i++)
        {
            if(i < playerCount)
            {
                txtScore[i].text = playerScore[i].ToString();
                txtName[i].text = playerName[i];
                txtName[i].color = playerColor[i];
                txtScore[i].color = playerColor[i];
            }
            playerScoreUI[i].SetActive(i < playerCount);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                stream.SendNext(time);
            }
        }
        else
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                time = (int)stream.ReceiveNext();
                txtTime.text = time.ToString();
            }
        }
    }


    IEnumerator AddNewScore(string name, int score)
    {
        // Legacy approach :
        //WWWForm form = new WWWForm();
        //form.AddField("name", name);
        //form.AddField("score", score);

        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("name", name));
        formData.Add(new MultipartFormDataSection("score", score.ToString()));
        formData.Add(new MultipartFormDataSection("baseUtcOffset", baseUtcOffset));

        UnityWebRequest request = UnityWebRequest.Post(URL, formData);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            scores = JsonUtility.FromJson<PlayerScore>(request.downloadHandler.text);
        }
    }
}
