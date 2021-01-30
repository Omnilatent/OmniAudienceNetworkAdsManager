using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudienceNetwork;
using UnityEngine.UI;
//make a cover to distance play field from banner
public class FANBannerCover : MonoBehaviour
{
    [SerializeField] RectTransform rectTransform;
    public AdSize adSize;
    bool getAdSizeSuccess;
    int height = 50;
    int padding = 20;
    // Start is called before the first frame update
    void Start()
    {
        switch (adSize)
        {
            case AdSize.BANNER_HEIGHT_50:
                {
                    height = 50;
                }
                break;
            case AdSize.BANNER_HEIGHT_90:
                {
                    height = 90;
                }
                break;
            case AdSize.RECTANGLE_HEIGHT_250:
                {
                    height = 250;
                }
                break;
        }
#if !UNITY_EDITOR && UNITY_ANDROID
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
            AndroidJavaObject resources = context.Call<AndroidJavaObject>("getResources");
            AndroidJavaObject displayMetrics =
                resources.Call<AndroidJavaObject>("getDisplayMetrics");
            float density = displayMetrics.Get<float>("density");

            height = (int)((height + padding) * density);
            getAdSizeSuccess = true;
        }));
        StartCoroutine(CoWaitForAdSize());
#endif
    }

    IEnumerator CoWaitForAdSize()
    {
        float retryInterval = 0.5f;
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(retryInterval);
        int tryTimes = 0;

        while (!getAdSizeSuccess && tryTimes < 10f / retryInterval)
        {
            yield return delay;
        }
        if (getAdSizeSuccess)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            Debug.Log("set Ad Height Success " + height);
        }
    }
}
