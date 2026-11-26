using System;

public abstract class IAdPlatform
{
    /* 调起侧边栏 */
    public abstract void NavigateToScene(Action onSuccess, Action<string> onFail);

    /* 激励视频广告 */
    public abstract void ShowRewardedVideoAd(string adId, Action onSuccess, Action<string> onFail);

    /* 插屏广告，返回是否真正展示了广告（未加载完 / 未到间隔 / 启动保护期内都会返回 false） */
    public abstract bool ShowInterstitialAd(string adId);

    /** 游戏分享 */
    public abstract void ShareGame(Action<bool> callBack);
}