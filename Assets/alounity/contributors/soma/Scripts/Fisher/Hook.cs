using R3;
using UnityEngine;
using FishRumble;
using System;
/// <summary>
/// 釣り糸の先端フック。釣り糸の先端についているオブジェクト操作のクラス。
/// </summary>
public class Hook : MonoBehaviour
{
    [SerializeField] private Transform lineAttachPoint;

    private Rigidbody hookRigidbody;

    private Transform caughtFish;

    void Awake()
    {
        hookRigidbody = GetComponent<Rigidbody>();

        MakeLockState();
    }

    public Transform GetCaughtFish()
    {
        return caughtFish;
    }


    private void MakeLockState(bool isPivot = false)
    {
        hookRigidbody.isKinematic = true;
        transform.SetParent(lineAttachPoint);
        if(!isPivot)
        {
            transform.localPosition = new Vector3(0, 0, 0.1f);
        }
    }

    private void MakeFreeState()
    {
        hookRigidbody.isKinematic = false;
        transform.SetParent(null);
    }

    /// <summary>
    /// 魚を釣る。Hook に魚をセットする
    /// </summary>
    /// <param name="fish"></param>
    public void CatchFish(Transform fish)
    {
        caughtFish = fish;
        caughtFish.SetParent(this.transform);
    }

    /// <summary>
    /// 魚を落として Idle に戻る。
    /// </summary>
    public void ReleaseFish()
    {
        if(caughtFish != null)
        {
            Destroy(caughtFish.gameObject);
            caughtFish = null;
        }

        MakeLockState(); 
    }

    /// <summary>
    /// 
    public void Release()
    {
        MakeFreeState();
    }
    public Action onAbleCatch;
    public Action onUnableCatch;

    void OnTriggerEnter(Collider other)
    {
        if(caughtFish != null) return;

        if(!other.gameObject.CompareTag("FishZone")) return;
        onAbleCatch?.Invoke();
    }
}
