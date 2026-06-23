using System;
using UnityEngine;

/// <summary>
/// 1つのFisherStateに対応するConfigurableJointのロープ制約パラメータ。
/// </summary>
[Serializable]
public class LineJointSettings
{
    public float maxDistance = 3f;
    public float contactDistance = 0.01f;
    public bool enableCollision = false;

    /// <summary>
    /// 指定したConfigurableJointにロープ挙動の設定を書き込む。
    /// spring/damperは常に0（バネ挙動なし）で固定。
    /// </summary>
    public void ApplyTo(ConfigurableJoint joint)
    {
        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;
        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;

        joint.linearLimitSpring = new SoftJointLimitSpring { spring = 0f, damper = 0f };

        joint.linearLimit = new SoftJointLimit
        {
            limit = maxDistance,
            bounciness = 0f,
            contactDistance = contactDistance
        };

        joint.enableCollision = enableCollision;
    }
}
