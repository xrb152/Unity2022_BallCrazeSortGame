using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lose : MonoBehaviour
{
    public Button tryAgainBtn;
    private void Start()
    {
        tryAgainBtn.onClick.AddListener(() =>
        {
            AdManager.ShowInterSafe(null, null, "inter_lose");
            GlobalAudioPlayer.PlaySFX("Button");
            gameObject.SetActive(false);
            LevelManager.i.RestartLevel();
        });
    }
}
