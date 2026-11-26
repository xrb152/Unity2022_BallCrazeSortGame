using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoosterUI : MonoBehaviour
{
    public TextMeshProUGUI nameBoosterText;
    public Image ic_booster;
    public TextMeshProUGUI contextText;
    public Button useWithMoneyBtn;
    public TextMeshProUGUI moneyText;
    public Button useWithAdsBtn;
    public void OpenUI(Booster booster)
    {
        gameObject.SetActive(true);

        nameBoosterText.text = booster.nameBooster;
        ic_booster.sprite = booster.ic_booster;
        ic_booster.SetNativeSize();
        contextText.text = booster.context;
        moneyText.text = booster.price.ToString();

        useWithMoneyBtn.onClick.RemoveAllListeners();
        useWithMoneyBtn.onClick.AddListener(() =>
        {
            if (booster.price <= CurrenciesManager.i.GetValue(TypeCurrency.Coin))
            {
                CurrenciesManager.i.UpdateValue(TypeCurrency.Coin, -booster.price);
                BoosterManager.i.UseBooster(booster.typeBooster);
                gameObject.SetActive(false);
            }
            else
            {
                UIManager.i.Notification("金币不足！");
            }

        });

        useWithAdsBtn.onClick.RemoveAllListeners();
        useWithAdsBtn.onClick.AddListener(() =>
        {
            AdManager.ShowRewardSafe(() =>
            {
                BoosterManager.i.UseBooster(booster.typeBooster);
                gameObject.SetActive(false);
            }, () =>
            {
                UIManager.i.Notification("广告暂不可用");
            }, "rv_" + booster.typeBooster);

        });
    }
}
