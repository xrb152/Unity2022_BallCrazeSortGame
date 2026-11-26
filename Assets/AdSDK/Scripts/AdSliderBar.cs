using System;
using System.Collections;
using System.Collections.Generic;
//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 侧边栏弹窗
/// </summary>
public class AdSliderBar : MonoBehaviour
{
    private static AdSliderBar instance;
    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnConfirm;
    [SerializeField] private GameObject awardPanel;
    private Action onCompleted;
    private bool showAward = false; 

    public static void Show(Action callBack)
    {
        if (instance == null)
        {
            // 加载 AdConfirm 预制体
            GameObject prefab = Resources.Load<GameObject>("AdSliderBar");
            if (prefab == null)
            {
                Debug.LogError("AdSliderBar 预制体未找到，请确保其位于 Resources 文件夹中。");
                return;
            }

            // 实例化 AdConfirm
            GameObject adConfirmInstance = Instantiate(prefab);
            instance = adConfirmInstance.GetComponent<AdSliderBar>();

            if (instance == null)
            {
                Debug.LogError("AdConfirm 脚本未附加到预制体上，请检查预制体设置。");
                Destroy(adConfirmInstance);
                return;
            }
        }

        // 设置回调并显示弹窗
        instance.onCompleted = callBack;
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
        gameObject.SetActive(false);
        btnClose?.onClick.AddListener(this.OnClose);
        btnConfirm?.onClick.AddListener(this.OnConfirm);
    }

    private void OnEnable()
    {
        // 如果超过24小时 不显示awardPanel：
        this.showAward = this.CheckAwardState();
        this.awardPanel.SetActive(this.showAward);
    }

    private void OnClose()
    {
        this.gameObject.SetActive(false);
    }

    private void OnConfirm()
    {
        this.OnClose();

        if (AdManager.instance == null)
        {
            Debug.LogWarning("[广告] AdManager 不存在，侧边栏无法打开");
            return;
        }

        AdManager.instance.NavigateToScene(() =>
        {
            Debug.Log("进入侧边栏成功！");
            if (this.showAward)
            {
                onCompleted?.Invoke();
                string awardKey = GetAwardKey();
                string currentTime = DateTime.UtcNow.ToString("o"); // 使用 ISO 8601 格式
                PlayerPrefs.SetString(awardKey, currentTime);
            }
            else
            {
                Debug.Log("上次领取奖励未满24小时");
            }
        });
    }

    /// <summary>奖励记录用的 PlayerPrefs key（按小游戏 AppID 区分）</summary>
    private string GetAwardKey()
    {
        string appId = AdManager.instance != null ? AdManager.instance.AppId : string.Empty;
        return appId + "Award";
    }

    private bool CheckAwardState()
    {
        string awardKey = GetAwardKey();
        string lastDisplayTimeStr = PlayerPrefs.GetString(awardKey, null);
        if (string.IsNullOrEmpty(lastDisplayTimeStr))
        {
            return true;
        }
        if (DateTime.TryParse(lastDisplayTimeStr, out DateTime lastDisplayTime))
        {
            TimeSpan timeSinceLastDisplay = DateTime.UtcNow - lastDisplayTime;
            return timeSinceLastDisplay.TotalHours >= 24;
        }
        return true;
    }
}
