using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using TTSDK;
#endif

/// <summary>
/// 广告总入口（沿用原工程写法：暂停/恢复、插屏冷却、激励视频、Banner 都在这一个脚本里）。
/// 广告后端固定为抖音小游戏 TTSDK，通过 AdSDK 里的 IAdPlatform / TikTokPlatform 调起广告。
/// 全局只有这一个广告管理脚本，场景里不需要再挂别的广告管理器。
/// </summary>
public class AdManager : MonoBehaviour
{
    public static AdManager instance { get; set; }

    [Header("Time Ads")]
    public float timeInter;
    public float timeInterAfk;
    public float timeInterBtn;

    private float currentTimeInter;
    private float currentTimeInterAfk;
    private float currentTimeInterBtn;

    [Header("小游戏ID")]
    public string AppId;

    [Header("激励视频广告位")]
    public List<string> videoAdIdList = new List<string>();

    [Header("插屏广告位")]
    public List<string> interAdIdList = new List<string>();

    [Header("插屏最小间隔(秒)，抖音要求两次插屏间隔不小于30秒")]
    public float interstitialMinInterval = 30f;

    /// <summary>抖音广告实现（TTSDK）。</summary>
    private IAdPlatform platform;

    /// <summary>上一次真正展示插屏的时间（Time.realtimeSinceStartup）</summary>
    private float lastInterstitialTime = -999f;

    /// <summary>广告点击节流：0.5 秒内只允许触发一次，防连点</summary>
    private float lastAdClickTime;

    /// <summary>激励视频是否正在播放（插屏要避开这个时间）</summary>
    private bool rewardAdShowing;

    bool canShowOpenAd = true;

    private bool bannerWarned;

    /// <summary>激励视频是否正在播放（插屏检查用）</summary>
    public bool IsRewardAdShowing { get { return this.rewardAdShowing; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            if (this != instance)
            {
                Destroy(gameObject);
                return;
            }
        }
    }

    private void Start()
    {
        Init();

        // 开局不预冷却：抖音本身有「广告模块启动 15 秒内不展示插屏」的限制，
        // 这里再压 30 秒会让第一次打开设置 / 首次结算永远看不到插屏。
        currentTimeInter = 0f;
        currentTimeInterAfk = 0f;
        currentTimeInterBtn = 0f;
    }

    /// <summary>
    /// 确保存在广告入口。直接从 GameScene 按 Play 调试时场景里没有 AdManager，
    /// 老写法 `AdManager.instance?.ShowReward(...)` 会静默什么都不做，表现就是「点看广告没反应」。
    /// </summary>
    public static AdManager Ensure()
    {
        if (instance != null) return instance;
        if (!Application.isPlaying) return null;

        instance = FindObjectOfType<AdManager>();
        if (instance != null) return instance;

        GameObject go = new GameObject("AdManager (Auto)");
        instance = go.AddComponent<AdManager>();
        Debug.LogWarning("[广告] 当前场景没有 AdManager，已自动创建（使用默认广告位）");
        return instance;
    }

    /// <summary>激励视频统一入口：广告系统不可用时会回调 fail，不会静默无反应。</summary>
    public static void ShowRewardSafe(Action complete, Action fail, string placement)
    {
        AdManager mgr = Ensure();
        if (mgr == null)
        {
            Debug.LogWarning($"[广告] 广告系统不可用，激励视频跳过（{placement}）");
            fail?.Invoke();
            return;
        }

        mgr.ShowReward(complete, fail, placement);
    }

    /// <summary>插屏统一入口：失败也回调，不影响主流程。</summary>
    public static void ShowInterSafe(Action complete, Action fail, string placement)
    {
        AdManager mgr = Ensure();
        if (mgr == null)
        {
            fail?.Invoke();
            return;
        }

        mgr.ShowInter(complete, fail, placement);
    }

    public void Init()
    {
        InitTTSdk();

        if (string.IsNullOrEmpty(this.AppId))
        {
            Debug.LogWarning("[广告] 小游戏 AppID 为空，广告可正常播放，仅埋点回传不可用");
        }

        if (this.videoAdIdList == null || this.videoAdIdList.Count == 0)
        {
            Debug.LogWarning("[广告] 激励视频广告位列表为空，将使用默认广告位");
        }

        if (this.interAdIdList == null || this.interAdIdList.Count == 0)
        {
            Debug.LogWarning("[广告] 插屏广告位列表为空，将使用默认广告位");
        }

        string videoId = this.GetRandVideoAdId();
        string interId = this.GetRandScreenAdId();
        this.platform = new TikTokPlatform(this.AppId, videoId, interId);

        if (this.platform == null)
        {
            Debug.LogError("[广告] 抖音广告平台初始化失败");
            return;
        }

        Debug.Log($"[广告] 平台已就绪：抖音小游戏，激励视频={videoId}，插屏={interId}");
    }

    private void InitTTSdk()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            // OnTTContainerInitCallback 的签名是 void Invoke(int code, ContainerEnv env)
            TT.InitSDK((code, env) =>
            {
                if (code == 0)
                {
                    Debug.Log("[抖音SDK] 初始化完成");
                }
                else
                {
                    // 1 = SDK 版本不支持，2 = 引擎版本不支持
                    Debug.LogWarning($"[抖音SDK] 初始化失败，错误码 {code}");
                }
            });
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[抖音SDK] 初始化异常（不影响广告接口调用）：{e.Message}");
        }
#endif
    }

    public void SetTime()
    {
        currentTimeInter = timeInter;
        currentTimeInterAfk = timeInterAfk;
        currentTimeInterBtn = timeInterBtn;
    }

    private void Update()
    {
        if (currentTimeInterBtn > 0) currentTimeInterBtn -= Time.deltaTime;
    }

    public static void PauseGame()
    {
        AudioListener.volume = 0;
        Time.timeScale = 0f;
    }

    public static void Resume()
    {
        Time.timeScale = 1f;
        AudioListener.volume = 1;
    }

    public bool CheckCanShowInterBtn()
    {
        return currentTimeInterBtn <= 0f;
    }

    /// <summary>抖音小游戏没有开屏广告，保留方法兼容旧调用。</summary>
    public void ShowOpen()
    {
    }

    /// <summary>
    /// 插屏广告。抖音的插屏是「投放即结束」的，没有关闭回调，
    /// 所以这里不再 PauseGame，避免 Time.timeScale 卡在 0。
    /// </summary>
    public void ShowInter(Action complete, Action fail, string placement)
    {
        if (!CheckCanShowInterBtn())
        {
            Debug.Log($"[广告] 插屏冷却中（剩余 {currentTimeInterBtn:F1} 秒），跳过本次：{placement}");
            fail?.Invoke();
            return;
        }

        if (this.platform == null)
        {
            Debug.LogWarning("[广告] 抖音广告平台未初始化，跳过插屏");
            fail?.Invoke();
            return;
        }

        // TryShowInterstitialAd 内部还会做「激励视频播放中」「抖音 30 秒间隔」「广告未加载完」等检查，
        // 只有真的展示出去了才重置冷却，避免白白吃掉一次机会。
        if (!this.TryShowInterstitialAd())
        {
            Debug.Log($"[广告] 插屏本次未展示（{placement}），原因见上一条日志");
            fail?.Invoke();
            return;
        }

        canShowOpenAd = false;
        SetTime();
        Debug.Log($"[广告] 插屏已展示：{placement}");
        complete?.Invoke();
    }

    /// <summary>激励视频广告，看完由抖音回调发奖。</summary>
    public void ShowReward(Action complete, Action fail, string placement)
    {
        // 广告正在播的时候又被请求（连点按钮、同时点两处），直接拒绝，
        // 避免同一次广告被重复展示、奖励被发两次。
        if (this.rewardAdShowing)
        {
            Debug.LogWarning("[广告] 激励视频正在播放，忽略本次重复请求");
            fail?.Invoke();
            return;
        }

        float now = Time.time;
        if (now - this.lastAdClickTime < 0.5f)
        {
            Debug.LogWarning("[广告] 广告点击过于频繁，请稍后再试");
            fail?.Invoke();
            return;
        }

        this.lastAdClickTime = now;

        if (this.platform == null)
        {
            Debug.LogError("[广告] 抖音广告平台未初始化，无法显示广告");
            fail?.Invoke();
            return;
        }

        canShowOpenAd = false;
        PauseGame();

        // 无论成功失败都要恢复时间缩放与冷却，避免游戏卡在 timeScale=0
        complete += () =>
        {
            SetTime();
            Resume();
        };
        fail += () =>
        {
            SetTime();
            Resume();
        };

        this.rewardAdShowing = true;

        try
        {
            this.platform.ShowRewardedVideoAd(this.GetRandVideoAdId(),
                () =>
                {
                    this.rewardAdShowing = false;
                    complete?.Invoke();
                },
                error =>
                {
                    this.rewardAdShowing = false;
                    Debug.LogWarning($"[广告] 激励视频未完成（{placement}）：{error}");
                    fail?.Invoke();
                });
        }
        catch (Exception e)
        {
            this.rewardAdShowing = false;
            Resume();
            Debug.LogError($"[广告] 显示激励视频异常：{e.Message}");
            fail?.Invoke();
        }
    }

    /// <summary>带节流的插屏展示：距上次展示不足 interstitialMinInterval 秒时直接跳过。</summary>
    public bool TryShowInterstitialAd()
    {
        if (this.platform == null)
        {
            Debug.LogWarning("[广告] 抖音广告平台未初始化，跳过本次插屏");
            return false;
        }

        // 抖音不允许在激励视频播放期间插屏，会直接报错，这里提前挡掉
        if (this.rewardAdShowing)
        {
            Debug.LogWarning("[广告] 激励视频播放中，跳过插屏");
            return false;
        }

        float now = Time.realtimeSinceStartup;
        if (now - this.lastInterstitialTime < this.interstitialMinInterval)
        {
            Debug.Log($"[广告] 插屏间隔不足 {this.interstitialMinInterval} 秒（剩余 {this.interstitialMinInterval - (now - this.lastInterstitialTime):F1} 秒），本次跳过");
            return false;
        }

        return this.ShowInterstitialAd();
    }

    /// <summary>直接展示插屏（不做间隔检查），返回是否真的展示出去了。</summary>
    public bool ShowInterstitialAd()
    {
        if (this.platform == null)
        {
            Debug.LogWarning("[广告] 抖音广告平台未初始化，跳过本次插屏");
            return false;
        }

        bool shown = this.platform.ShowInterstitialAd(this.GetRandScreenAdId());
        if (shown)
        {
            this.lastInterstitialTime = Time.realtimeSinceStartup;
        }

        return shown;
    }

    /// <summary>跳转抖音侧边栏（原 AdSliderBar 在用）。</summary>
    public void NavigateToScene(Action onSuccess = null, Action<string> onFail = null)
    {
        if (this.platform == null)
        {
            Debug.LogError("[广告] 抖音广告平台未初始化，无法跳转场景");
            onFail?.Invoke("广告平台未初始化");
            return;
        }

        try
        {
            this.platform.NavigateToScene(onSuccess, onFail);
        }
        catch (Exception e)
        {
            Debug.LogError($"[广告] 跳转场景异常：{e.Message}");
            onFail?.Invoke($"跳转场景异常：{e.Message}");
        }
    }

    public void ShareGame(Action<bool> callBack = null)
    {
        this.platform?.ShareGame(callBack);
    }

    /// <summary>抖音小游戏没有 Banner 广告位，保留空实现兼容旧调用。</summary>
    public void ShowBanner()
    {
        if (bannerWarned) return;
        bannerWarned = true;
        Debug.Log("[广告] 抖音小游戏无 Banner 广告，已忽略 ShowBanner");
    }

    public void HideBanner()
    {
    }

    // 随机取一个激励视频广告位ID
    private string GetRandVideoAdId()
    {
        if (this.videoAdIdList == null || this.videoAdIdList.Count == 0)
        {
            Debug.LogWarning("[广告] 激励视频广告位列表为空，使用默认广告位");
            return "f32bf9970ah83mq1kg";
        }

        int index = UnityEngine.Random.Range(0, this.videoAdIdList.Count);
        return this.videoAdIdList[index];
    }

    // 随机取一个插屏广告位ID
    private string GetRandScreenAdId()
    {
        if (this.interAdIdList == null || this.interAdIdList.Count == 0)
        {
            Debug.LogWarning("[广告] 插屏广告位列表为空，使用默认广告位");
            return "dk217ga5e5b48uo4gh";
        }

        int index = UnityEngine.Random.Range(0, this.interAdIdList.Count);
        return this.interAdIdList[index];
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus && canShowOpenAd)
        {
            ShowOpen();
        }
    }
}
