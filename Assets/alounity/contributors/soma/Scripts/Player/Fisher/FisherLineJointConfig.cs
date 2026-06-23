using UnityEngine;
using FishRumble;
using Unity.VisualScripting;

/// <summary>
/// FisherStateごとのConfigurableJoint設定を一括管理するScriptableObject。
/// </summary>
[CreateAssetMenu(fileName = "FisherSpringJointConfig", menuName = "Fisher/SpringJoint Config")]
public class FisherLineJointConfig : ScriptableObject
{
    [Header("Idle: 竿先にぶら下がってぷらぷらする状態")]
    public LineJointSettings idle;

    [Header("Waiting: 魚がかかるのを待つ状態")]
    public LineJointSettings waiting;

    [Header("Catching: 魚がかかった状態")]
    public LineJointSettings catching;

    [Header("Swinging: 魚をぶん回している状態")]
    public LineJointSettings swinging;

    [Header("SmallAttack: 小攻撃")]
    public LineJointSettings smallAtack;
    [Header("MiddleAttack: 中攻撃")]
    public LineJointSettings middleAtack;

    [Header("BigAttack: 大攻撃")]
    public LineJointSettings bigAtack;


    /// <summary>
    /// 指定したStateに対応する設定を返す。
    /// </summary>
    public LineJointSettings GetFromState(FisherState state) => state switch
    {
        FisherState.Idle => idle,
        FisherState.Waiting => waiting,
        FisherState.Swinging => swinging,
        _ => idle,
    };

    /// <summary>
    /// 指定したAttackに対応する設定を返す。
    /// </summary>
    public LineJointSettings GetFromAttack(int i) => i switch
    {
        0 => smallAtack,
        1 => middleAtack,
        2 => bigAtack,
        _ => smallAtack,
    };
}
