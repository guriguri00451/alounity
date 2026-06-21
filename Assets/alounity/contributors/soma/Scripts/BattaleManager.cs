using UnityEngine;
using FishRumble;
using JetBrains.Annotations;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

namespace FishRumble
{
    public class BattleManager : MonoBehaviour
    {
        [Header("準備パラメータ")]
        [SerializeField] GameObject QRPanel;
        [SerializeField] GameObject[] playerLocations;
        [SerializeField] GameObject playerPrefab;
        [SerializeField] int DeadCoolTimeMs;
        [SerializeField] SensorDataReceiver sensorDataReceiver;
        [SerializeField] int[] playersLives;
        int winningTeamID;
        [SerializeField] PlayerManager[] players;
        PlayerData[] playersData;

        void Awake()
        {
            AppManager.Instance.battleManager = this;
        }
        void Start()
        {
            ManageBattle();
        }

        async void ManageBattle()
        {
            await EnterSettingsScreen();

            //バトル開始
            PlayersSubscribe();
        }

        async UniTask EnterSettingsScreen()
        {
            await UniTask.WaitUntil(() => Input.GetKey(KeyCode.Space));
            QRPanel.SetActive(false);
        }

        void PlayersSubscribe()
        {
            playersLives = new int[] {3,3};
            for(int i = 0; i < players.Length; i++)
            {
                players[i].PlayerID = i;
                players[i].onDeath += DeathPlayer;
                players[i].BindSensorInput(sensorDataReceiver);
                players[i].Init();
            }
        }

        async void DeathPlayer(int playerID)
        {
            playersLives[playerID] -= 1;
            if(playersLives[playerID] <= 0)
            {
                CheckGameMatch(playerID);
                return;
            }
            await DeadCoolTime();
            RespawnPlayer(playerID);
        }
        void RespawnPlayer(int playerID)
        {
            players[playerID].Init();
            players[playerID].transform.position = GetRespawnPoint();
        }

        Vector3 GetRespawnPoint()
        {
            int i = Random.Range(0, playerLocations.Length - 1); 
            return playerLocations[i].transform.position;
        }

        async UniTask DeadCoolTime()
        {
            await UniTask.Delay(DeadCoolTimeMs);
        }

        void CheckGameMatch(int playerID)
        {
            int winnerID = 0;
            int livingCount = 0;
            //もう決着か判断
            for (int i = 0; i < playersLives.Length; i++)
            {
                if(playersLives[i] <= 0) 
                {
                    livingCount += 1;
                    winnerID = i;
                }
            }

            Debug.Log("finishgamecheck:"+livingCount);
            // もし残機がなかったら時にゲーム終了処理に行く
            if(livingCount <= 1)
            {
                FinishGame(winnerID);
            }

            //　リスポーン前にUI真っ黒にする処理

        }
        void FinishGame(int winnerID)
        {
            AppManager.Instance.winnerPlayerID = 0;
            AppManager.Instance.resultPlayersData = null;
            AppManager.Instance.winnerPlayerID = winnerID;
            AppManager.Instance.resultPlayersData = playersData;

            SceneManager.LoadScene(SceneName.Result.ToString());
        }

        
    }
}