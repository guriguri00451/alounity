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
    [SerializeField] PlayerEffectMaker effectMaker;
    public Action<int, int> onDamage; 
    public Action<int> onDeath;

    SensorDataReceiver boundReceiver;

    public void Init()
    {
        riderHealth.PlayerID = playerID;
        fisherController.PlayerID = playerID;
        boatController.PlayerID = playerID;

        riderHealth.onDead += DeadPlayer;
        riderHealth.onDamaged += Damaged;

        riderHealth.Respawn();
    }

    void DeadPlayer()
    {
        fisherController.DropFish();
        boatController.FixBoat();
        effectMaker.makeDeadEffect();

        riderHealth.onDead -= DeadPlayer;
        onDeath?.Invoke(playerID);
    }

    void Damaged(int damage)
    {
        effectMaker.makeHitEffect();
        onDamage?.Invoke(damage,playerID);
    }

    /// <summary>
    /// スポーン・リスポーン後にBattleManagerから呼ぶ。PlayerIDに基づくチームのイベントを登録する。
    /// </summary>
    public void BindSensorInput(SensorDataReceiver receiver)
    {
        UnbindSensorInput();
        boundReceiver = receiver;

        bool isTeamA = playerID == 0;
        if (isTeamA)
        {
            receiver.onPaddleRightInput_A.AddListener(boatController.OnPaddleRightInput);
            receiver.onPaddleLeftInput_A.AddListener(boatController.OnPaddleLeftInput);
            receiver.onFisherInput_A.AddListener(fisherController.OnSensorInput);
        }
        else
        {
            receiver.onPaddleRightInput_B.AddListener(boatController.OnPaddleRightInput);
            receiver.onPaddleLeftInput_B.AddListener(boatController.OnPaddleLeftInput);
            receiver.onFisherInput_B.AddListener(fisherController.OnSensorInput);
        }
    }

    void UnbindSensorInput()
    {
        if (boundReceiver == null) return;
        boundReceiver.onPaddleRightInput_A.RemoveListener(boatController.OnPaddleRightInput);
        boundReceiver.onPaddleLeftInput_A.RemoveListener(boatController.OnPaddleLeftInput);
        boundReceiver.onFisherInput_A.RemoveListener(fisherController.OnSensorInput);
        boundReceiver.onPaddleRightInput_B.RemoveListener(boatController.OnPaddleRightInput);
        boundReceiver.onPaddleLeftInput_B.RemoveListener(boatController.OnPaddleLeftInput);
        boundReceiver.onFisherInput_B.RemoveListener(fisherController.OnSensorInput);
        boundReceiver = null;
    }

    void OnDestroy()
    {
        UnbindSensorInput();
    }
}
