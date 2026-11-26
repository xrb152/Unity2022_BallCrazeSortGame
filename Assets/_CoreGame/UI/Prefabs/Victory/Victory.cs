using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Victory : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI moneyText;
    public Button x5Btn;
    public Button continueBtn;
    private void OnEnable()
    {
        levelText.text = "第" + (LevelManager.i.currentLevelCount + 1) + "关";
        moneyText.text = "+ " + LevelManager.i.currentLevelData.reward;
    }
    private void Start()
    {
        x5Btn.onClick.AddListener(() =>
        {
            AdManager.ShowRewardSafe(() =>
            {
                GlobalAudioPlayer.PlaySFX("Button");
                CurrenciesManager.i.FlyingIcon(moneyText.transform, false, UIManager.i.mainCanvas, 10);
                CurrenciesManager.i.UpdateValue(TypeCurrency.Coin, (int)(LevelManager.i.currentLevelData.reward * 5f));
                LevelManager.i.NextLevel();
                gameObject.SetActive(false);
            }, () =>
            {
                UIManager.i.Notification("广告暂不可用");
            }, "rv_x5_win");

        });

        continueBtn.onClick.AddListener(() =>
        {
            GlobalAudioPlayer.PlaySFX("Button");
            CurrenciesManager.i.FlyingIcon(moneyText.transform, false, UIManager.i.mainCanvas, 5);
            CurrenciesManager.i.UpdateValue(TypeCurrency.Coin, LevelManager.i.currentLevelData.reward);
            AdManager.ShowInterSafe(null, null, "inter_win");
            LevelManager.i.NextLevel();
            gameObject.SetActive(false);
        });
    }
}
