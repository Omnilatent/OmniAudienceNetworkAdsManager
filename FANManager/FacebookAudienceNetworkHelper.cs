﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudienceNetwork;
using Omnilatent.AdsMediation;

public class FANBannerData
{
    public AdSize adSize;
    public AudienceNetwork.AdPosition adPosition;
    public float xPos;
    public float yPos;
    public enum AnchorPosition { Center, Bottom }
    public AnchorPosition anchorPosition;

    public FANBannerData(AdSize adSize, AudienceNetwork.AdPosition adPosition, float xPos, float yPos, AnchorPosition anchorPos = AnchorPosition.Center)
    {
        this.adSize = adSize;
        this.adPosition = adPosition;
        this.xPos = xPos;
        this.yPos = yPos;
        this.anchorPosition = anchorPos;
    }
}

public class FacebookAudienceNetworkHelper : MonoBehaviour, IAdsNetworkHelper
{

    public static FacebookAudienceNetworkHelper instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

#if !UNITY_EDITOR
        if (!AudienceNetworkAds.IsInitialized())
        {
            AudienceNetworkAds.Initialize();
        }
#endif
    }

    AdsManager.InterstitialDelegate interstitialDelegate;
    AdsManager.InterstitialDelegate interstitialClosedDelegate;
    //AdsManager.BoolDelegate onRewardWatched;
    RewardDelegate onRewardWatched;
    private RewardedVideoAd rewardedVideoAd;
    InterstitialAd interstitialAd;
    private AdView adView;
    private AudienceNetwork.AdPosition currentAdViewPosition;

    public void ShowBanner(string placementId, AudienceNetwork.AdPosition adPosition, AdsManager.InterstitialDelegate onAdLoaded = null)
    {
        if (string.IsNullOrEmpty(placementId)) onAdLoaded(false);
        if (adView)
        {
            adView.Dispose();
        }

        // Create a banner's ad view with a unique placement ID (generate your own on the Facebook app settings).
        // Use different ID for each ad placement in your app.
        adView = new AdView(placementId, AdSize.BANNER_HEIGHT_50);

        adView.Register(gameObject);
        currentAdViewPosition = adPosition;

        adView.AdViewDidLoad = delegate ()
        {
            adView.Show(currentAdViewPosition);
            onAdLoaded?.Invoke(true);
        };
        adView.AdViewDidFailWithError = delegate (string error)
        {
            LogError(error);
            onAdLoaded?.Invoke(false);
        };
        adView.LoadAd();
    }

    //public void ShowBanner(string placementId, AdSize adSize, AdPosition adPosition, float yPos, AdsManager.InterstitialDelegate onAdLoaded = null)
    /*public void ShowBanner(string placementId, AdsManager.InterstitialDelegate onAdLoaded, FANBannerData bannerData)
    {
        if (adView)
        {
            adView.Dispose();
        }

        adView = new AdView(placementId, bannerData.adSize);

        adView.Register(gameObject);
        currentAdViewPosition = bannerData.adPosition;

        adView.AdViewDidLoad = delegate ()
        {
            if (bannerData.adPosition != AdPosition.CUSTOM)
            { adView.Show(bannerData.adPosition); }
            else
            {
                switch (bannerData.anchorPosition)
                {
                    case FANBannerData.AnchorPosition.Center:
                        adView.ShowCenter(bannerData.xPos, bannerData.yPos);
                        break;
                    case FANBannerData.AnchorPosition.Bottom:
                        adView.ShowBottom(bannerData.yPos);
                        break;
                }
            }
            onAdLoaded?.Invoke(true);
        };
        adView.AdViewDidFailWithError = delegate (string error)
        {
            LogError(error);
            onAdLoaded?.Invoke(false);
        };
        adView.LoadAd();
    }*/

    public void HideBanner()
    {
        if (adView)
        {
            adView.Dispose();
        }
    }

    void AdViewDidLoad()
    {
        adView.Show(currentAdViewPosition);
    }

    void AdViewDidFailWithError(string error)
    {
        LogError(error);
    }

    public void RequestInterstitialNoShow(string placementId, AdsManager.InterstitialDelegate onAdLoaded = null, bool showLoading = true)
    {
        if (string.IsNullOrEmpty(placementId)) onAdLoaded(false);
        if (interstitialAd != null)
        {
            interstitialAd.Dispose();
        }
        interstitialAd = new InterstitialAd(placementId);

        interstitialAd.Register(gameObject);

        if (onAdLoaded != null)
            interstitialDelegate = onAdLoaded;

        interstitialAd.InterstitialAdDidLoad = InterstitialAdDidLoad;
        interstitialAd.InterstitialAdDidFailWithError = InterstitialAdDidFailWithError;
        interstitialAd.InterstitialAdWillLogImpression = InterstitialAdWillLogImpression;
        interstitialAd.InterstitialAdDidClick = InterstitialAdDidClick;
        interstitialAd.InterstitialAdDidClose = InterstitialAdDidClose;

#if UNITY_ANDROID
        /* This callback will only be triggered if the Interstitial activity has
         * been destroyed without being properly closed. This can happen if an
         * app with launchMode:singleTask (such as a Unity game) goes to
         * background and is then relaunched by tapping the icon. */
        interstitialAd.interstitialAdActivityDestroyed = InterstitialAdActivityDestroyed;
#endif

        interstitialAd.LoadAd();
    }

    public static void ShowInterstitial(string placementId)
    {
        if (string.IsNullOrEmpty(placementId)) return;
        if (instance.interstitialAd != null && instance.interstitialAd.IsValid())
        {
            instance.interstitialAd.Show();
        }
        else
        {
            Debug.Log("FAN Show: Ad not loaded.");
        }
    }

    void InterstitialAdDidLoad()
    {
        interstitialDelegate?.Invoke(true);
        //Manager.LoadingAnimation(false);
    }

    void InterstitialAdDidFailWithError(string error)
    {
        LogError(error);
        interstitialDelegate?.Invoke(false);
        //Manager.LoadingAnimation(false);
    }

    void InterstitialAdWillLogImpression() { }

    void InterstitialAdDidClick()
    {
        Debug.Log("ad clicked");
    }

    void InterstitialAdDidClose()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Dispose();
            if (interstitialClosedDelegate != null) interstitialClosedDelegate.Invoke(true);
            Debug.Log("ad closed");
        }
    }

    void InterstitialAdActivityDestroyed()
    {
        Debug.Log("ad activity destroyed");
    }

    public void Reward(RewardDelegate rewardDelegate, string placementId)
    {
        if (string.IsNullOrEmpty(placementId)) rewardDelegate(new RewardResult(RewardResult.Type.LoadFailed, "FAN: Placement ID is empty"));
        if (rewardedVideoAd != null)
        {
            rewardedVideoAd.Dispose();
        }
        onRewardWatched = rewardDelegate;

        rewardedVideoAd = new RewardedVideoAd(placementId);
        rewardedVideoAd.Register(gameObject);

        rewardedVideoAd.RewardedVideoAdDidLoad = RewardedVideoAdDidLoad;
        rewardedVideoAd.RewardedVideoAdDidFailWithError = RewardedVideoAdDidFailWithError;
        //rewardedVideoAd.RewardedVideoAdWillLogImpression
        //rewardedVideoAd.RewardedVideoAdDidClick
        rewardedVideoAd.RewardedVideoAdDidSucceed = RewardedVideoAdDidSucceed;
        rewardedVideoAd.RewardedVideoAdDidFail = RewardedVideoAdDidFail;
        rewardedVideoAd.RewardedVideoAdDidClose = RewardedVideoAdDidClose;

#if UNITY_ANDROID
        //rewardedVideoAd.RewardedVideoAdActivityDestroyed 
#endif
        rewardedVideoAd.LoadAd();
    }

    void RewardedVideoAdDidLoad()
    {
        if (rewardedVideoAd.IsValid())
        {
            rewardedVideoAd.Show();
        }
        //Manager.LoadingAnimation(false);
    }

    void RewardedVideoAdDidFailWithError(string error)
    {
        onRewardWatched?.Invoke(new RewardResult(RewardResult.Type.LoadFailed, error));
        LogError(error);
        //Manager.LoadingAnimation(false);
        //AdsManager.ShowError(error);
    }

    void RewardedVideoAdDidSucceed()
    {
        Debug.Log("Facebook ads reward succeed");
    }

    void RewardedVideoAdDidFail()
    {
        onRewardWatched?.Invoke(new RewardResult(RewardResult.Type.LoadFailed, string.Empty));
    }

    void RewardedVideoAdDidClose()
    {
        onRewardWatched?.Invoke(new RewardResult(RewardResult.Type.Finished, string.Empty));
        onRewardWatched = null;
        if (rewardedVideoAd != null)
        {
            rewardedVideoAd.Dispose();
        }
    }

    void LogError(string error)
    {
        Debug.Log("FAN error: " + error);
    }

    public void ShowBanner(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdLoaded = null)
    {
        ShowBanner(placementType, Omnilatent.AdsMediation.BannerTransform.defaultValue, onAdLoaded);
    }

    public void ShowBanner(AdPlacement.Type placementType, BannerTransform bannerTransform, AdsManager.InterstitialDelegate onAdLoaded = null)
    {
        AudienceNetwork.AdPosition adPosition;
        switch (bannerTransform.adPosition)
        {
            case Omnilatent.AdsMediation.AdPosition.Top:
            case Omnilatent.AdsMediation.AdPosition.TopLeft:
            case Omnilatent.AdsMediation.AdPosition.TopRight:
                adPosition = AudienceNetwork.AdPosition.TOP;
                break;
            case Omnilatent.AdsMediation.AdPosition.Bottom:
            case Omnilatent.AdsMediation.AdPosition.Center:
            default:
                adPosition = AudienceNetwork.AdPosition.BOTTOM;
                break;
        }
        ShowBanner(CustomMediation.GetFANPlacementId(placementType), adPosition, onAdLoaded);
    }

    public void ShowInterstitial(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdClosed)
    {
        interstitialClosedDelegate = onAdClosed;
        ShowInterstitial(CustomMediation.GetFANPlacementId(placementType));
    }

    public void RequestInterstitialNoShow(AdPlacement.Type placementType, AdsManager.InterstitialDelegate onAdLoaded = null, bool showLoading = true)
    {
        RequestInterstitialNoShow(CustomMediation.GetFANPlacementId(placementType), onAdLoaded, showLoading);
    }

    public void Reward(AdPlacement.Type placementType, RewardDelegate onFinish)
    {
        Reward(onFinish, CustomMediation.GetFANPlacementId(placementType));
    }

    public void RequestInterstitialRewardedNoShow(AdPlacement.Type placementType, RewardDelegate onFinish = null)
    {
        onFinish?.Invoke(new RewardResult(RewardResult.Type.LoadFailed, "Not supported by Audience Network"));
    }

    public void ShowInterstitialRewarded(AdPlacement.Type placementType, RewardDelegate onAdClosed)
    {
        onAdClosed?.Invoke(new RewardResult(RewardResult.Type.LoadFailed, "Not supported by Audience Network"));
    }
}
