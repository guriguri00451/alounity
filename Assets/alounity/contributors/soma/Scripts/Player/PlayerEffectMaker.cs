using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

using UnityEngine.Experimental.VFX;

public class PlayerEffectMaker : MonoBehaviour
{
    [SerializeField] VisualEffect hitEffect;
    [SerializeField] VisualEffect deadEffect;

    [SerializeField] Transform[] spawnPoints;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void makeHitEffect()
    {
        Debug.Log("kk");
        hitEffect.Play();
    }

    public void makeDeadEffect()
    {
        deadEffect.Play();
    }

    VFXEventAttribute MakeRandomPosAttribute(VisualEffect effect)
    {
        VFXEventAttribute attr = effect.CreateVFXEventAttribute();
        attr.SetVector3(Shader.PropertyToID("position"), GetRandomPos());
        return attr;
    }

    Vector3 GetRandomPos()
    {
        return spawnPoints[Random.Range(0,spawnPoints.Length-1)].position;
    }

}
