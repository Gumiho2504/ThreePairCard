using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Sample;
using UnityEngine;

public class GoogleAdsMobManager : MonoBehaviour
{
    public string appId = "ca-app-pub-3366480085674476~9709856136";
#if UNITY_ANDROID
    private string bannerId = "ca-app-pub-3940256099942544/6300978111";
    private string openAdsId = "ca-app-pub-3940256099942544/9257395921";
    private string interId = "";
    private string rewardId = "";
   // private string nativeId = "";
#elif UNITY_IPHONE
    private string bannerId = "ca-app-pub-3940256099942544/2934735716";
    private string interId = "";
    private string rewardId = "";
    // private string nativeId = "";
    private string openAdsId = "ca-app-pub-3940256099942544/5575463023";
#else
    private string bannerId = "unused";
    private string interId = "";
    private string rewardId = "";
#endif

    public static GoogleAdsMobManager instance;
    BannerView bannerView;
    InterstitialAd interstitialAd;
    RewardedAd rewardedAd;
    private AppOpenAd appOpenId;

    public bool _isInitialized = true;

    public BannerViewController bannerViewController;
    public InterstitialAdController interstitialAdController;
    public RewardedAdController rewardedAdController;
    public RewardedInterstitialAdController rewardedInterstitialAdController;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
        MobileAds.Initialize(initStatus =>
        {
            if (initStatus == null)
            {
                Debug.LogError("Google Mobile Ads initialization failed.");
                _isInitialized = false;
                return;
            }



            print("Ads initialized : " + initStatus.ToString());
            _isInitialized = true;
            //bannerViewController.LoadAd();
            interstitialAdController.LoadAd();
            rewardedAdController.LoadAd();
           // rewardedInterstitialAdController.LoadAd();
        });


    }


    public void ShowBanner()
    {
        CreateBannerView();
        ListenToAdEvents();
        if (bannerView == null)
        {
            CreateBannerView();
        }
        AdRequest request = new AdRequest();
        request.Keywords.Add("unity-admob-sample");
        bannerView.LoadAd(request);

    }

    private void ListenToAdEvents()
    {
        // Raised when an ad is loaded into the banner view.
        bannerView.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner view loaded an ad with response : "
                + bannerView.GetResponseInfo());
        };

        bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("Banner view failed to load an ad with error : "
                + error);
        };

        bannerView.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Banner view paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        bannerView.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner view recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        bannerView.OnAdClicked += () =>
        {
            Debug.Log("Banner view was clicked.");
        };
        // Raised when an ad opened full screen content.
        bannerView.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Banner view full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        bannerView.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Banner view full screen content closed.");
        };
    }

    public void CreateBannerView()
    {
        if (bannerView != null)
        {
            DestroyBannerAd();
        }
        bannerView = new BannerView(bannerId, AdSize.SmartBanner, AdPosition.Bottom);
    }

    public void DestroyBannerAd()
    {
        if (bannerView != null)
        {
            Debug.Log("Destroying banner view.");
            bannerView.Destroy();
            bannerView = null;
        }
    }
}
