using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using GoogleMobileAds.Sample;
using GoogleMobileAds.Api;
public class StartGame : MonoBehaviour
{
    float progress = 0;
    public Text loadingText;
    public GameObject loadingPanel;

    public GameObject textLogoFirstGameobject,textLogoSecondGameobject;

    IEnumerator Start()
    {
        
        StartUpAnimation();

        yield return new WaitForSeconds(2f);
        loadingPanel.LeanMoveLocalX(Screen.width*2f,1f).setEaseInOutBack();
        // while (progress <= 100)
        // {
        //     progress += 90 * Time.deltaTime;
        //     loadingText.text = "loading" + progress.ToString("0") + "%";
        //     yield return null;
        // }
        // if (GoogleAdsMobManager.instance._isInitialized)
        // {
        //     //GoogleAdsMobManager.instance.rewardedInterstitialAdController.ShowAd();
        //     loadingPanel.SetActive(false);
        // }


    }

    protected void StartUpAnimation(){
        textLogoFirstGameobject
            .LeanMoveLocalY(textLogoFirstGameobject.transform.position.y,1f)
            .setFrom(Screen.height + 100f)
            .setEaseInElastic()
            .setOnComplete(() => {
                textLogoSecondGameobject
                    .LeanMoveLocalY(textLogoSecondGameobject.transform.position.y,1f)
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



}
