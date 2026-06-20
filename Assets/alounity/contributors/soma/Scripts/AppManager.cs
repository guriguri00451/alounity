using UnityEngine;
using FishRumble;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class AppManager : MonoBehaviour
{
    public static AppManager Instance;
    
    // [SerializeField] TitleManager titleManager;
    [SerializeField] BattleManager battleManager;
    public int playerCount;
    public PlayerData[] resultPlayersData;
    public int winnerPlayerID;
    
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    public void StartBattle()
    {
        SceneManager.LoadScene(SceneName.Field.ToString());
    } 
}