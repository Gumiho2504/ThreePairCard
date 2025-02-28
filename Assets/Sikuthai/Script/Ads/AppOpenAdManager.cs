using System;
using GoogleMobileAds.Api;
using UnityEngine;

public class AppOpenAdManager : MonoBehaviour
{
    private AppOpenAd appOpenAd;
    private DateTime loadTime;
    private bool isShowingAd = false;

#if UNITY_ANDROID
    private string openAdId = "ca-app-pub-3940256099942544/9257395921"; // Test Ad ID
#elif UNITY_IPHONE
    private string openAdId = "ca-app-pub-3940256099942544/5575463023"; // Test Ad ID
#else
    private string openAdId = "unused";
#endif

    private static AppOpenAdManager instance;

    public static AppOpenAdManager Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object when changing scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
       // MobileAds.Initialize(initStatus => { Debug.Log("AdMob Initialized!"); });
       // LoadAppOpenAd();
    }

    // ✅ LOAD APP OPEN AD
    public void LoadAppOpenAd()
    {
        if (appOpenAd != null)
        {
            appOpenAd.Destroy();
            appOpenAd = null;
        }

        Debug.Log("Loading App Open Ad...");
        AdRequest request = new AdRequest();
        request.Keywords.Add("unity-admob-sample");
        AppOpenAd.Load(openAdId, request, (AppOpenAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("App Open Ad failed to load: " + error);
                return;
            }

            appOpenAd = ad;
            loadTime = DateTime.UtcNow;
            RegisterAdEvents(appOpenAd);
            Debug.Log("App Open Ad Loaded Successfully!");
        });
    }

    // ✅ SHOW APP OPEN AD
    public void ShowAppOpenAd()
    {
        if (IsAdAvailable() && !isShowingAd)
        {
            Debug.Log("Showing App Open Ad...");
            appOpenAd.Show();
            isShowingAd = true;
        }
        else
        {
            Debug.Log("App Open Ad not ready yet.");
            LoadAppOpenAd(); // Reload if not available
        }
    }

    // ✅ CHECK IF AD IS AVAILABLE
    private bool IsAdAvailable()
    {
        return appOpenAd != null && (DateTime.UtcNow - loadTime).TotalHours < 4;
    }

    // ✅ REGISTER AD EVENTS
    private void RegisterAdEvents(AppOpenAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("App Open Ad Closed.");
            isShowingAd = false;
            LoadAppOpenAd(); // Load a new ad after closing
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("App Open Ad failed to show: " + error);
            isShowingAd = false;
            LoadAppOpenAd();
        };

        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log($"App Open Ad earned: {adValue.Value} {adValue.CurrencyCode}");
        };

        ad.OnAdClicked += () =>
        {
            Debug.Log("App Open Ad Clicked!");
        };
    }
}
