using System;
using UnityEngine;

/// <summary>
/// 1つのFisherStateに対応するSpringJointパラメータ一式。
/// </summary>
[Serializable]
public class SpringJointSettings
{
    public float spring = 10f;
    public float damper = 0.2f;
    public float minDistance = 0f;
    public float maxDistance = 0f;
    public float tolerance = 0.025f;
    public bool enableCollision = false;

    /// <summary>
    /// 指定したSpringJointにこの設定を書き込む。
    /// </summary>
    public void ApplyTo(SpringJoint joint)
    {
        joint.spring = spring;
        joint.damper = damper;
        joint.minDistance = minDistance;
        joint.maxDistance = maxDistance;
        joint.tolerance = tolerance;
        joint.enableCollision = enableCollision;
    }
}
