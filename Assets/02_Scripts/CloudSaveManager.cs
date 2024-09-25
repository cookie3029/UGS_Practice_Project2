using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct PlayerData
{
    public string name;
    public int level;
    public int xp;
    public int gold;

    public List<ItemData> items;
}

[Serializable]
public struct ItemData
{
    public string name;
    public int value;
    public int count;
    public string icon;
}

public class CloudSaveManager : MonoBehaviour
{
    [Header("Ui Button")]
    [SerializeField] private Button loginButton;
    [SerializeField] private Button saveSingleDataButton;
    [SerializeField] private Button saveMultiDataButton;
    [SerializeField] private Button loadSingleDataButton, loadMultiDataButton;

    [Header("Player Data")]
    public PlayerData playerData;

    private async void Awake()
    {
        // UGS가 초기화 성공했을 때 호출되는 콜백
        UnityServices.Initialized += () =>
        {
            Debug.Log("UGS 초기화 성공");
        };

        UnityServices.InitializeFailed += (ex) =>
        {
            Debug.Log($"UGS 초기화 실패 : {ex.Message}");
        };

        // UGS 초기화
        await UnityServices.InitializeAsync();

        // 익명 사용자로 로그인 성공했을 때 호출되는 콜백
        AuthenticationService.Instance.SignedIn += () =>
        {
            string playerId = AuthenticationService.Instance.PlayerId;
            Debug.Log($"익명로그인 성공\nPlayerId: <color=#00ff00>{playerId}</color>");
        };

        // 로그인 버튼 클릭 이벤트 연결
        loginButton.onClick.AddListener(async () =>
        {
            await SignIn();
        });


        // 싱글 데이터 버튼 클릭 이벤트 연결
        saveSingleDataButton.onClick.AddListener(async () =>
        {
            await SaveSingleData();
        });

        // 멀티 데이터 버튼 클릭 이벤트 연결
        saveMultiDataButton.onClick.AddListener(async () =>
        {
            await SaveMultiData<PlayerData>("PlayerData", playerData);
        });

        loadSingleDataButton.onClick.AddListener(async () =>
        {
            await LoadSingleData();
        });

        // 멀티 데이터 로드 버튼 클릭 이벤트 연결
        loadMultiDataButton.onClick.AddListener(async () =>
        {
            await LoadMultiData<PlayerData>("PlayerData");
        });
    }

    // 로그인 함수
    private async Task SignIn()
    {
        if (AuthenticationService.Instance.IsSignedIn) return;

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    // 단일 데이터 저장
    private async Task SaveSingleData()
    {
        // 저장할 데이터 선언
        var data = new Dictionary<string, object>
        {
            {"player_name", "Cookie3029"},
            {"level", 30},
            {"xp", 2000},
            {"gold", 100}
        };

        // Cloud 저장
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);

        Debug.Log("싱글 데이터 저장 완료");
    }

    // 복수 데이터 저장
    private async Task SaveMultiData<T>(string key, T saveData)
    {
        // 딕셔너리 파일
        var data = new Dictionary<string, object>
        {
            {key, saveData}
        };

        // 저장 메소드
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
        Debug.Log("복수 데이터 저장 완료");

        // playerData 삭제
        playerData = new PlayerData();
    }

    // 싱글 데이터 로드
    private async Task LoadSingleData()
    {
        /*
            var fields = new HashSet<string>();

            fields.Add("player_name");
            fields.Add("level");
            fields.Add("xp");
            fields.Add("gold");
        */

        // [1]
        // var fields = new HashSet<string>
        // {
        //     "player_name", "level", "xp", "gold"
        // };
        // var data = await CloudSaveService.Instance.Data.Player.LoadAsync(fields);

        // [2]
        var data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>
        {
            "player_name", "level", "xp", "gold"
        });

        if (data.TryGetValue("player_name", out var playerName))
        {
            Debug.Log($"Player Name : {playerName.Value.GetAs<string>()}");
        }

        if (data.TryGetValue("level", out var level))
        {
            Debug.Log($"Player LV : {level.Value.GetAs<string>()}");
        }

        if (data.TryGetValue("gold", out var gold))
        {
            Debug.Log($"Gold : {gold.Value.GetAs<string>()}");
        }
    }

    // 복수 데이터 로드
    private async Task LoadMultiData<T>(string key)
    {
        var loadData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });

        if (loadData.TryGetValue(key, out var data))
        {
            playerData = data.Value.GetAs<PlayerData>();
        }
    }

    /*
        HashSet<T> 자료형
        - 중복값을 허용하지 않는다.
        - 검색속도가 빠르다. O(1)
        - 인덱스 기반으로 값을 추출할 수 없다. TryGetValue 사용
    */
}
