using UnityEngine;
using FishRumble;
using Unity.VisualScripting;

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

    [Header("SmallAttack: 小攻撃")]
    public SpringJointSettings smallAtack;
    [Header("MiddleAttack: 中攻撃")]
    public SpringJointSettings middleAtack;

    [Header("SmallAttack: 大攻撃")]
    public SpringJointSettings bigAtack;


    /// <summary>
    /// 指定したStateに対応する設定を返す。
    /// </summary>
    public SpringJointSettings GetFromState(FisherState state) => state switch
    {
        FisherState.Idle => idle,
        FisherState.Waiting => waiting,
        FisherState.Swinging => swinging,
        _ => idle,
    };

    /// <summary>
    /// 指定したAttackに対応する設定を返す。
    /// </summary>
    public SpringJointSettings GetFromAttack(int i) => i switch
    {
        0 => smallAtack,
        1 => middleAtack,
        2 => bigAtack,
        _ => smallAtack,
    };
}
