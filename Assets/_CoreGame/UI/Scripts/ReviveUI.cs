using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReviveUI : MonoBehaviour
{
    public int reviveCost = 500;
    public Button closeBtn;
    public TextMeshProUGUI moneyText;
    public Button reviveWithMoneyBtn;
    public Button reviveWithAdsBtn;
    private void Start()
    {
        moneyText.text = reviveCost.ToString();
        closeBtn.onClick.AddListener(() =>
        {
            GameManager.i.CallEnding(false);
            gameObject.SetActive(false);
        });
        reviveWithMoneyBtn.onClick.AddListener(() =>
        {
            if (CurrenciesManager.i.GetValue(TypeCurrency.Coin) >= reviveCost)
            {
                UnlockAndSort();
                CurrenciesManager.i.UpdateValue(TypeCurrency.Coin, -reviveCost);
            }
            else
            {
                UIManager.i.Notification("金币不足！");
            }

        });
        reviveWithAdsBtn.onClick.AddListener(() =>
        {
            AdManager.ShowRewardSafe(() =>
            {
                UnlockAndSort();
            }, () =>
            {
                UIManager.i.Notification("广告暂不可用");
            }, "rv_revive");

        });
    }
    private void UnlockAndSort()
    {
        GameManager.i.trayHolder.UnlockNextSlot();
        BoosterManager.i.SortCoffee();
        gameObject.SetActive(false);
    }
}
