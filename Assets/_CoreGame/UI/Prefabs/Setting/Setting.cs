using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Setting : MonoBehaviour
{
    public Button closeBtn;
    public Button sfxBtn;
    public Button musicBtn;
    public Button vibratonBtn;
    public Button homeBtn;
    public Button continueBtn;
    private void OnEnable()
    {
        homeBtn.gameObject.SetActive(GameManager.i.gameStatus != GameStatus.None);
        continueBtn.gameObject.SetActive(GameManager.i.gameStatus != GameStatus.None);
        sfxBtn.transform.GetChild(0).gameObject.SetActive(GlobalAudioPlayer.audioPlayer.IsSoundOn());
        musicBtn.transform.GetChild(0).gameObject.SetActive(GlobalAudioPlayer.audioPlayer.IsMusicOn());
        vibratonBtn.transform.GetChild(0).gameObject.SetActive(GameManager.vibration);
    }
    private void Start()
    {
        closeBtn.onClick.AddListener(() =>
        {
            GlobalAudioPlayer.PlaySFX("ClosePop");
            if (GameManager.i.gameStatus != GameStatus.None)
            {
                GameManager.i.ContinuePlay();
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }

        });
        sfxBtn.onClick.AddListener(() =>
        {
            GlobalAudioPlayer.audioPlayer.ToggleSound();
            sfxBtn.transform.GetChild(0).gameObject.SetActive(GlobalAudioPlayer.audioPlayer.IsSoundOn());
        });
        musicBtn.onClick.AddListener(() =>
        {
            GlobalAudioPlayer.audioPlayer.ToggleMusic();
            musicBtn.transform.GetChild(0).gameObject.SetActive(GlobalAudioPlayer.audioPlayer.IsMusicOn());
        });
        vibratonBtn.onClick.AddListener(() =>
        {
            GameManager.vibration = !GameManager.vibration;
            vibratonBtn.transform.GetChild(0).gameObject.SetActive(GameManager.vibration);
        });
        homeBtn.onClick.AddListener(() =>
        {
            GlobalAudioPlayer.PlaySFX("ClosePop");
            AdManager.ShowInterSafe(null, null, "inter_home");
            GameManager.i.BackToHome();
            gameObject.SetActive(false);
        });
        continueBtn.onClick.AddListener(() =>
        {
            GlobalAudioPlayer.PlaySFX("ClosePop");
            GameManager.i.ContinuePlay();
            gameObject.SetActive(false);
        });
    }
}
