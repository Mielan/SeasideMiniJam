using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections.ObjectModel;

public class OnlineScoreBoard : MonoBehaviour
{
    [SerializeField] private string URL = "https://sittingduckgames.com/Home/score/get_score.php";
    [SerializeField] private float refreshRate = 5f;
    [SerializeField] private List<Transform> scoreTexts = new List<Transform>();
    private PlayerScore playerScore;
    private string baseUtcOffset;
    bool internetIsConnected = true;

    private void OnEnable()
    {
        //ReadOnlyCollection<System.TimeZoneInfo> zones = System.TimeZoneInfo.GetSystemTimeZones();
        //Debug.Log(zones.Count);
        //foreach (System.TimeZoneInfo zone in zones) 
        //    Debug.Log("time zone id : " + zone.Id + " display name : " + zone.DisplayName + " base UTC Offset : " + zone.BaseUtcOffset);

        System.TimeZoneInfo localZone = System.TimeZoneInfo.Local;
        baseUtcOffset = localZone.BaseUtcOffset.ToString();

        //Debug.Log("time zone id : " + localZone.Id + " display name : " + localZone.DisplayName + " base UTC Offset : " + baseUtcOffset);
        //Debug.Log(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments));
        //Debug.Log(System.Environment.UserName);

        GenerateRequest();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            string name = "new player " + playerScore.score.Length;
            int score = Random.Range(1, 300);
            StartCoroutine(AddNewScore(name, score));
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
            internetIsConnected = false;
            Debug.Log(request.error);
        }
        else
        {
            playerScore = JsonUtility.FromJson<PlayerScore>(request.downloadHandler.text);
            DisplayScore();
        }
    }

    public void GenerateRequest()
    {
        StartCoroutine(LoadPlayerScore());
    }

    IEnumerator LoadPlayerScore()
    {
        while (internetIsConnected)
        {
            StartCoroutine(ProcessRequest(URL));
            yield return new WaitForSeconds(refreshRate);
        }
    }


    private IEnumerator ProcessRequest(string uri)
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("baseUtcOffset", baseUtcOffset));

        UnityWebRequest request = UnityWebRequest.Post(URL, formData);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            internetIsConnected = false;
            Debug.Log("Error: " + request.error);
        }
        else
        {
            playerScore = JsonUtility.FromJson<PlayerScore>(request.downloadHandler.text);
            DisplayScore();
        }
    }

    void DisplayScore()
    {
        for (int i = 0; i < scoreTexts.Count; i++)
        {
            if (i < playerScore.score.Length)
            {
                scoreTexts[i].gameObject.SetActive(true);
                scoreTexts[i].GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = (i + 1).ToString() + ".";
                scoreTexts[i].GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = playerScore.score[i].name;
                scoreTexts[i].GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text = playerScore.score[i].score.ToString();

                //string dateTimeString = playerScore.score[i].datetime;
                //System.DateTime dateTime = System.DateTime.Parse(dateTimeString);
                //scoreTexts[i].GetChild(3).gameObject.GetComponent<TextMeshProUGUI>().text = dateTime.ToString("yyyy MMM dd") + "\n" + dateTime.ToString("hh:mm:ss tt");
            }
            else
            {
                scoreTexts[i].gameObject.SetActive(false);
            }
        }
    }
}


[System.Serializable]
public class PlayerScore
{
    public string message;
    public Score[] score;
}

[System.Serializable]
public class Score
{
    public int id;
    public string name;
    public int score;
    public string datetime;
}