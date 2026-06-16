using UnityEngine;
using FishRumble;

/// <summary>
/// FisherStateごとのSpringJoint設定を一括管理するScriptableObject。
/// </summary>
[CreateAssetMenu(fileName = "FisherSpringJointConfig", menuName = "Fisher/SpringJoint Config")]
public class FisherSpringJointConfig : ScriptableObject
{
    [Header("Idle: 竿先にぶら下がってぷらぷらする状態")]
    public SpringJointSettings idle;

    [Header("Waiting: 魚がかかるのを待つ状態")]
    public SpringJointSettings waiting;

    [Header("Catching: 魚がかかった状態")]
    public SpringJointSettings catching;

    [Header("Swinging: 魚をぶん回している状態")]
    public SpringJointSettings swinging;

    /// <summary>
    /// 指定したStateに対応する設定を返す。
    /// </summary>
    public SpringJointSettings Get(FisherState state) => state switch
    {
        FisherState.Idle => idle,
        FisherState.Waiting => waiting,
        FisherState.Catching => catching,
        FisherState.Swinging => swinging,
        _ => idle,
    };
}
