using UnityEngine;
using FishRumble;
using JetBrains.Annotations;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

namespace FishRumble
{
    public class BattleManager : MonoBehaviour
    {
        [Header("準備パラメータ")]
        [SerializeField] GameObject[] playerLocations;
        [SerializeField] GameObject playerPrefab;
        [SerializeField] int DeadCoolTimeMs;
        [SerializeField]
        int[] playersLives;
        int winningTeamID;
        PlayerManager[] players;

        public void Start()
        {
            ManageBattle();
        }

        async void ManageBattle()
        {
            //バトル準備
            PlayersSpawn();

            //バトル開始

        }

        void PlayersSpawn()
        {
            for(int i = 0; i < AppManager.Instance.playerCount; i++)
            {
                GameObject obj = Instantiate(playerPrefab, playerLocations[i].transform.position, playerLocations[i].transform.rotation);
                players[i] = obj.GetComponent<PlayerManager>();
                players[i].PlayerID = i;
                players[i].onDeath += DeathPlayer;
            }
        }

        async void DeathPlayer(int i)
        {
            players[i].onDeath -= DeathPlayer;
            playersLives[i] -= 1;
            if(playersLives[i] <= 0)
            {

                return;
            }
            await DeadCoolTime();
        }

        async UniTask DeadCoolTime()
        {
            await UniTask.Delay(DeadCoolTimeMs);
        }

        void DefetePlayer(int playerID)
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

            if(livingCount <= 1)
            {
                FinishGame(winnerID);
            }
            //UI真っ黒にする処理
        }
        void FinishGame(int winnerPlayerID)
        {
            
        }

        
    }
}