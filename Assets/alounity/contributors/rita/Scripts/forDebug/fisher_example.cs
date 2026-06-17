using UnityEngine;

public class Fisher_example : MonoBehaviour
{
    
    [Header("フィッシャー設定")]
    [SerializeField] float forceMultiplier = 50f;
    [SerializeField] float shakeThreshold = 100f;

    public void OnFisher(SensorDataPayload data)
    {
      //Debug.Log($"[Fisher] 向き:{data.orientation.alpha:F1},振り:({data.rotation.alpha:F1}");

      //dirの値に向く
      float dir = data.orientation.alpha;
      transform.rotation = Quaternion.Euler(transform.eulerAngles.x, -dir, transform.eulerAngles.z);

      //rotationの値に応じて釣り竿を振る
      float force = data.rotation.alpha * forceMultiplier;
      
      // 閾値以下の動きは無視
        if(force>shakeThreshold){
          Debug.Log("釣り竿を戻した　強さ：" + force); 
        }
        else if(force<-shakeThreshold){
          Debug.Log("釣り竿を振った　強さ：" + force);
        }
      
    }
}
