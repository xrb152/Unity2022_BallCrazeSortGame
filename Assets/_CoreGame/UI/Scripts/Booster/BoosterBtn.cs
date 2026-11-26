using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoosterBtn : MonoBehaviour
{
    public TypeBooster typeBooster;
    public GameObject unlocked;
    public Image ic_booster;
    public GameObject locked;
    public TextMeshProUGUI levelText;
    private Booster booster;
    private Button boosterBtn;
    private bool isUnlocked;
    private void Start()
    {
        booster = BoosterInfo.i.GetInfoBooster(typeBooster);
        ic_booster.sprite = booster.ic_booster;
        ic_booster.SetNativeSize();
        levelText.text = "第" + (booster.levelUnlock + 1) + "关";
        boosterBtn = GetComponent<Button>();
        boosterBtn.onClick.AddListener(() =>
        {
            if (isUnlocked)
            {
                if (BoosterManager.i.CheckCanUseBooster(typeBooster))
                {
                    UIManager.i.boosterUI.OpenUI(booster);
                }
            }
            else
            {
                UIManager.i.Notification("达到第" + (booster.levelUnlock + 1) + "关解锁");
            }

        });
    }
    private void Update()
    {
        isUnlocked = LevelManager.i.currentLevelCount >= booster.levelUnlock;
        unlocked.SetActive(isUnlocked);
        locked.SetActive(!isUnlocked);
    }
}
