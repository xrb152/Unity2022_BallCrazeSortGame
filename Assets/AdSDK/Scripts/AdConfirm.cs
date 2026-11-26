using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 激励广告弹窗
/// </summary>
public class AdConfirm : MonoBehaviour
{
    private static AdConfirm instance;

    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnConfirm;

    /// <summary>奖励标题文本（预制体里的「添加金币」）</summary>
    private Text rewardTitleTxt;
    /// <summary>奖励数值文本（预制体里的 100）</summary>
    private Text rewardValueTxt;
    private string defaultRewardTitle;
    private string defaultRewardValue;

    private Action onCompleted;

    /// <summary>已经发起广告、还在等回调。防止玩家连点确认重复请求广告。</summary>
    private bool waitingAd;

    /// <summary>弹窗没设置回调时使用的兜底金币奖励（读预制体文案，读不到就用 100）。</summary>
    private const int FallbackCoinReward = 100;

    /// <summary>
    /// 显示激励视频确认弹窗
    /// </summary>
    /// <param name="callBack">看完广告后的回调</param>
    /// <param name="rewardTitle">奖励标题，传 null 用预制体默认文案（添加金币）</param>
    /// <param name="rewardValue">奖励数值，传 null 用预制体默认数值（100）</param>
    public static void Show(Action callBack, string rewardTitle = null, string rewardValue = null)
    {
        if (instance == null)
        {
            // 加载 AdConfirm 预制体
            GameObject prefab = Resources.Load<GameObject>("AdConfirm");
            if (prefab == null)
            {
                Debug.LogError("AdConfirm 预制体未找到，请确保其位于 Resources 文件夹中。");
                return;
            }

            // 实例化 AdConfirm
            GameObject adConfirmInstance = Instantiate(prefab);
            instance = adConfirmInstance.GetComponent<AdConfirm>();

            if (instance == null)
            {
                Debug.LogError("AdConfirm 脚本未附加到预制体上，请检查预制体设置。");
                Destroy(adConfirmInstance);
                return;
            }
        }

        // 设置回调并显示弹窗
        instance.onCompleted = callBack;
        instance.waitingAd = false;          // 每次打开都重置，上次异常中断不会卡住
        instance.SetConfirmInteractable(true);
        instance.SetRewardText(rewardTitle, rewardValue);
        instance.gameObject.SetActive(true);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // 按钮引用兜底：预制体里序列化的字段一旦丢失（换预制体、混淆器改字段名等），
        // btnConfirm/btnClose 会是 null，点「确认」毫无反应，表现就是「看完广告没发奖」。
        // 这里按对象名再找一次，找不到就打错误日志。
        if (btnConfirm == null)
            btnConfirm = FindButtonByName("btn_confirm");
        if (btnClose == null)
            btnClose = FindButtonByName("btn_close");

        if (btnConfirm == null)
            Debug.LogError("[广告] AdConfirm 没找到「确认」按钮（btn_confirm），点确认不会有任何反应");
        if (btnClose == null)
            Debug.LogWarning("[广告] AdConfirm 没找到「关闭」按钮（btn_close）");

        // 奖励文案没有序列化到脚本上，按对象名从预制体里找，找不到就跳过（不影响弹窗本身）
        rewardTitleTxt = FindTextByName("tips (1)");
        rewardValueTxt = FindTextByName("value");
        defaultRewardTitle = rewardTitleTxt != null ? rewardTitleTxt.text : null;
        defaultRewardValue = rewardValueTxt != null ? rewardValueTxt.text : null;

        gameObject.SetActive(false);
        btnClose?.onClick.AddListener(this.OnClose);
        btnConfirm?.onClick.AddListener(this.OnConfirm);
    }

    /// <summary>
    /// 兜底：场景里的「激励」按钮（jiliBtn）的 onClick 直接绑的是 AdConfirm.SetActive(true)，
    /// 绕过了 Show(回调)，onCompleted 永远是 null —— 表现就是「广告播出去了、也提示观看完成，
    /// 但奖励一个都没到账」。这里在弹窗被打开时补一个默认回调，保证任何一个入口点确认都能拿到奖励。
    /// </summary>
    private void OnEnable()
    {
        if (onCompleted == null)
        {
            onCompleted = GrantDefaultCoinReward;
            Debug.LogWarning("[广告] AdConfirm 被直接打开（未传回调），本次使用默认金币奖励");
        }

        // 每次打开都重置等待状态，避免上次异常中断后按钮永远点不动
        this.waitingAd = false;
        this.SetConfirmInteractable(true);
    }

    /// <summary>默认奖励：按弹窗上显示的金币数值发放。</summary>
    private void GrantDefaultCoinReward()
    {
        int amount = FallbackCoinReward;
        if (!string.IsNullOrEmpty(this.defaultRewardValue))
        {
            int parsed;
            if (int.TryParse(this.defaultRewardValue, out parsed) && parsed > 0)
            {
                amount = parsed;
            }
        }

        if (!CurrenciesManager.isInstanceAlive)
        {
            Debug.LogError("[广告] CurrenciesManager 不存在，默认金币奖励无法发放");
            return;
        }

        CurrenciesManager.i.UpdateValue(TypeCurrency.Coin, amount);
        Debug.Log($"[广告] 已发放默认奖励：金币 +{amount}");
    }

    private Button FindButtonByName(string name)
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].gameObject.name == name)
            {
                return buttons[i];
            }
        }
        return null;
    }

    private Text FindTextByName(string name)
    {
        Text[] texts = GetComponentsInChildren<Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i].gameObject.name == name)
            {
                return texts[i];
            }
        }
        return null;
    }

    /// <summary>切换弹窗展示的奖励内容，空值回落到预制体默认文案</summary>
    private void SetRewardText(string rewardTitle, string rewardValue)
    {
        if (rewardTitleTxt != null)
        {
            rewardTitleTxt.text = string.IsNullOrEmpty(rewardTitle) ? defaultRewardTitle : rewardTitle;
        }

        if (rewardValueTxt != null)
        {
            rewardValueTxt.text = string.IsNullOrEmpty(rewardValue) ? defaultRewardValue : rewardValue;
        }
    }


    private void OnClose()
    {
        this.waitingAd = false;
        this.SetConfirmInteractable(true);
        // 关掉弹窗就把回调清掉，避免下一次由按钮直接打开时复用到上一次的匿名回调
        this.onCompleted = null;
        this.gameObject.SetActive(false);
    }

    void OnConfirm()
    {
        // 广告还在播的时候连点确认，会被 AdManager 的「0.5 秒节流」判成失败，
        // 表现就是弹窗莫名关掉、奖励没发。这里直接忽略重复点击。
        if (this.waitingAd) return;

        // 统一走 AdManager（工程里唯一的广告总入口），不再直接碰平台实现，
        // 这样所有激励视频的入口都只有一条链路，日志和冷却也统一。
        AdManager mgr = AdManager.Ensure();

        if (mgr == null)
        {
            // 广告系统整个不可用是系统性故障，玩家点什么都没反应比「白给一次奖励」更糟，
            // 这里直接发奖并打错误日志。
            Debug.LogError("[广告] 广告系统不可用，无法播放激励视频：直接发放奖励兜底");
            InvokeRewardSafely();
            this.OnClose();
            return;
        }

        this.waitingAd = true;
        this.SetConfirmInteractable(false);
        Debug.Log("[广告] 请求激励视频（AdManager → 抖音 TTSDK）");

        // ShowReward 的失败回调没有参数（具体原因由 AdManager 打日志），
        // 这里转成字符串回调给 OnAdError，保持弹窗的错误提示流程不变。
        mgr.ShowReward(OnAdWatched, () => OnAdError("激励视频未完成"), "rv_confirm");
    }

    private void SetConfirmInteractable(bool value)
    {
        if (btnConfirm != null)
        {
            btnConfirm.interactable = value;
        }
    }

    private void OnAdWatched()
    {
        this.waitingAd = false;
        this.SetConfirmInteractable(true);
        Debug.Log("[广告] 广告观看完成，发放奖励");
        InvokeRewardSafely();
        OnClose();
    }

    /// <summary>
    /// 发奖回调统一包一层 try/catch：回调里任何异常（音效、UI 引用丢失等）
    /// 都不能把「奖励已发放」这件事吞掉，同时把异常打到日志里方便定位。
    /// </summary>
    private void InvokeRewardSafely()
    {
        if (onCompleted == null)
        {
            Debug.LogWarning("[广告] 没有发奖回调，本次看完广告不会获得奖励");
            return;
        }

        try
        {
            onCompleted.Invoke();
            Debug.Log("[广告] 发奖回调已执行完毕");
        }
        catch (Exception e)
        {
            Debug.LogError($"[广告] 发奖回调执行异常：{e}");
        }
    }

    private void OnAdError(string errorMessage)
    {
        this.waitingAd = false;
        this.SetConfirmInteractable(true);
        Debug.LogError($"广告播放失败: {errorMessage}");
        // 加载 AdError 预制体
        GameObject adErrorPrefab = Resources.Load<GameObject>("AdError");
        if (adErrorPrefab != null)
        {
            // 实例化 AdError
            GameObject adErrorInstance = Instantiate(adErrorPrefab, transform.position, Quaternion.identity);
            // 1 秒后销毁实例
            Destroy(adErrorInstance, 1.5f);
        }
        else
        {
            Debug.LogError("AdError 预制体未找到，请确保其位于 Resources 文件夹中。");
        }

        // 广告失败时把确认弹窗关掉，避免玩家以为「点了没反应」或者卡在弹窗上
        OnClose();
    }
}
