using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopUI : MonoBehaviour
{
    public TextMeshProUGUI levelTxt;
    public Button setting;
    private void Start()
    {
        setting.onClick.AddListener(() =>
        {
            GlobalAudioPlayer.PlaySFX("OpenPop");
            UIManager.i.setting.gameObject.SetActive(true);
            GameManager.i.gameStatus = GameManager.i.gameStatus == GameStatus.None ? GameStatus.None : GameStatus.Paused;

            // 打开设置是天然的停顿点，在这里尝试一次插屏（有冷却与抖音 30 秒间隔保护）
            AdManager.ShowInterSafe(null, null, "inter_setting");
        });
    }
    private void Update()
    {
        levelTxt.text = "第" + (LevelManager.i.currentLevelCount + 1) + "关";
    }

}
