using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button saveScoreButton;
    [SerializeField] private Button allScoreButton;
    [SerializeField] private TMP_InputField scoreIf;

    private const string leaderboardId = "Ranking";

    // 점수를 저장할 리스트
    public List<LeaderboardEntry> entries = new();

    private async void Awake()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += async () =>
        {
            Debug.Log("로그인 완료");

            // 기존 점수 불러오기
            await GetPlayerScore();
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        // Add Score 버튼 클릭 이벤트 연결
        saveScoreButton.onClick.AddListener(async () => await AddScore(int.Parse(scoreIf.text)));

        allScoreButton.onClick.AddListener(async () => await GetAllScores());
    }

    private async Task AddScore(int score)
    {
        var response = await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score);
        Debug.Log(JsonConvert.SerializeObject(response));
    }

    private async Task GetPlayerScore()
    {
        var response = await LeaderboardsService.Instance.GetPlayerScoreAsync(leaderboardId);

        scoreIf.text = response.Score.ToString();

        Debug.Log(JsonConvert.SerializeObject(response));
    }

    private async Task GetAllScores()
    {
        var response = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId);
        Debug.Log(JsonConvert.SerializeObject(response));

        entries = response.Results;

        string ranking = "";

        foreach (var entry in entries)
        {
            ranking += $"[{entry.Rank + 1}] {entry.PlayerName} : {entry.Score}\n";
        }

        Debug.Log(ranking);

        await Task.Delay(2000);

        await GetScoreByPage();
    }

    // 페이징 처리
    private async Task GetScoreByPage()
    {
        var options = new GetScoresOptions
        {
            Offset = 1,
            Limit = 20
        };

        // 5 + 자신 + 5
        var options2 = new GetPlayerRangeOptions
        {
            RangeLimit = 5
        };

        var response = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId, options);

        Debug.Log(JsonConvert.SerializeObject(response));
    }
}
