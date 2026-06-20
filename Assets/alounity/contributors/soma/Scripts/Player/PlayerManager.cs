using UnityEngine;
using FishRumble;
using System.Diagnostics;
using Cysharp.Threading.Tasks;

public class PlayerManager: MonoBehaviour
{
    [SerializeField] KayakRiderHealth riderHealth;
    [SerializeField] FisherController fisherController;
    [SerializeField] BoatController boatController;
    [SerializeField] PlayerState currentState;

    void Awake()
    {
        
    }

    void Init()
    {
        
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

    public async UniTask DeadPlayer()
    {
        await UniTask.WaitUntil(() => currentState == PlayerState.Respawning);
    }
}
