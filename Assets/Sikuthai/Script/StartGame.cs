using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using GoogleMobileAds.Sample;
using GoogleMobileAds.Api;
using System;
public class StartGame : MonoBehaviour
{

    float progress = 0;
    public Text userNameText, coinText, gemText;
    public Text loadingText;

    public GameObject loadingPanel;

    public GameObject textLogoFirstGameobject, textLogoSecondGameobject;
  
    public GameObject settingPanel;

    IEnumerator Start()
    {

        StartUpAnimation();

        yield return new WaitForSeconds(2f);
        loadingPanel.LeanMoveLocalX(Screen.width * 2f, 1f).setEaseInOutBack();

       
    }

    public  void UpdateTextUI(){
        userNameText.text = FindAnyObjectByType<AuthInitialization>().user.name;
        coinText.text = FindAnyObjectByType<AuthInitialization>().user.coin.ToString();
        gemText.text = FindAnyObjectByType<AuthInitialization>().user.gem.ToString();
    }

    protected void StartUpAnimation()
    {
        textLogoFirstGameobject
            .LeanMoveLocalY(textLogoFirstGameobject.transform.position.y, 1f)
            .setFrom(Screen.height + 100f)
            .setEaseInElastic()
            .setOnComplete(() =>
            {
                textLogoSecondGameobject
                    .LeanMoveLocalY(textLogoSecondGameobject.transform.position.y, 1f)
                    .setFrom(-Screen.height - 100f);
            })
            ;

    }

    public void StartButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ExitApp()
    {
        Application.Quit();
    }


    public void ShowSettingPanel()
    {
        settingPanel.SetActive(true);
    }
    public void HideSettingPanel(){
        settingPanel.SetActive(false);
    }


}
