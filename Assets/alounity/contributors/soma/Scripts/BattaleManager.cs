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
        [SerializeField] int countdownTimeMs;
        [SerializeField] int battleTimeMs;
        [SerializeField] int DeadTimeMs;
        int[] playersLives;
        int currentTime;
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

            await Countdown();

            //バトル開始
            await StartCountdown();

            //バトル終了
            await TimeUp();

        }

        void PlayersSpawn()
        {
            for(int i = 0; i < AppManager.Instance.playerCount; i++)
            {
                Instantiate (playerPrefab, playerLocations[i].transform.position, playerLocations[i].transform.rotation);
            }
        }

        async UniTask DeadCoolTime(PlayerState currentState,int teamID)
        {
            for(int i = 0; i < players.Length; i++)
            {
                 if (PlayerState.Dead == currentState&&teamID==i)
                {
                    await Timer(DeadTimeMs);
                }
            }
        }

        async UniTask TimeUp()
        {
            FinishGame();
        }

        void FinishGame()
        {
            
        }

        async UniTask Countdown()
        {
            await Timer(countdownTimeMs);
        }

        async UniTask StartCountdown()
        {
            await Timer(battleTimeMs);
        }

        

        async UniTask Timer(int Ms)
        {
            currentTime = Ms;
            for(int i = 0; i < Ms; i++)
            {
                await UniTask.Delay(1);
                currentTime -= 1;
                Debug.Log(currentTime);
            }
            
            return;
        }
    }
}