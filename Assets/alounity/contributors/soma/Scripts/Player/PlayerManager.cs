using UnityEngine;
using FishRumble;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using System;

public class PlayerManager: MonoBehaviour
{
    int playerID; 
    public int PlayerID
    {
        set
        {
            playerID = value;
        }
        get
        {
            return playerID;
        }
    }
    [Header("Settings")]
    [SerializeField] PlayerData playerData;
    [Header("References")]
    [SerializeField] KayakRiderHealth riderHealth;
    [SerializeField] FisherController fisherController;
    [SerializeField] BoatController boatController;
    [SerializeField] PlayerState currentState;

    public Action<int> onDeath;

    void Awake()
    {
        Init();
    }

    void Init()
    {
        riderHealth.PlayerID = playerID;
        fisherController.PlayerID = playerID;
        boatController.PlayerID = playerID;

        riderHealth.onDead += DeadPlayer;
    }

    void ManageState(PlayerState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case PlayerState.Readey:
                break;
            case PlayerState.Fighting:
                break;
            case PlayerState.Dead:
                break;
            case PlayerState.Respawning:
                break;
        }
    }

    void DeadPlayer()
    {
        riderHealth.onDead -= DeadPlayer;
        onDeath?.Invoke(playerID);
    }
}
