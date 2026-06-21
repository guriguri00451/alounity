using UnityEngine;
using UnityEngine.UI;

public class VersusUIController: MonoBehaviour
{
    [Header("TeamA")]
    [SerializeField] Image HPBar_TeamA;
    [SerializeField] GameObject threeLives_TeamA;
    [SerializeField] GameObject twoLives_TeamA;
    [SerializeField] GameObject oneLives_TeamA;

    [Header("TeamB")]
    [SerializeField] Image HPBar_TeamB;
    [SerializeField] GameObject threeLives_TeamB;
    [SerializeField] GameObject twoLives_TeamB;
    [SerializeField] GameObject oneLives_TeamB;

    public void SetHPBar(int TeamNum, int lifeValue)
    {
        float value = lifeValue / 100;
        if(TeamNum == 0)
        {
            HPBar_TeamA.fillAmount = lifeValue;
        }
        else
        {
            HPBar_TeamB.fillAmount = lifeValue;
        }
    }

    public void SetLivesCount(int TeamNum ,int livesCount)
    {
        if(TeamNum == 0)
        {
            if(livesCount == 2)threeLives_TeamA.SetActive(false);
            if(livesCount == 1)twoLives_TeamA.SetActive(false);
        }
        else
        {
            if(livesCount == 2)threeLives_TeamB.SetActive(false);
            if(livesCount == 1)twoLives_TeamB.SetActive(false);
        }
    }
}