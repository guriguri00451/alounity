using FishRumble;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ResultManager: MonoBehaviour
{
    [SerializeField] Transform cam;
    [SerializeField] Transform APoint;
    [SerializeField] Transform BPoint;

    void Start()
    {
        if(AppManager.Instance.winnerPlayerID == 0)
        {
            cam.transform.position = APoint.transform.position;
            cam.transform.rotation = APoint.transform.rotation;
        }
        else
        {
            cam.transform.position = BPoint.transform.position;
            cam.transform.rotation = BPoint.transform.rotation;
        }
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.Space))
        {
            SceneManager.LoadScene(SceneName.Title.ToString());
        }
    }
}