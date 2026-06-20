using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    [SerializeField] GameObject selectPanel;
    
    public void ToSelect()
    {
        SceneManager.LoadScene("Select");
    }

    public void SelectSingle()
    {
        SceneManager.LoadScene("Single");
    }

    public void SelectVersus()
    {
        SceneManager.LoadScene("Versus");
    }

    public void ToTitle()
    {
        SceneManager.LoadScene("Title");
    }
}
