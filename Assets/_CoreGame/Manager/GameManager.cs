using System.Linq;
using UnityEngine;

[System.Serializable]
public enum GameStatus
{
    None, Playing, Paused
}
public class GameManager : Singleton<GameManager>
{
    public GameStatus gameStatus;
    public Camera mainCamera;
    public TrayHolder trayHolder;
    public InputManager inputManager;

    public static bool vibration
    {
        get { return PlayerPrefs.GetInt("vibration", 1) == 1; }
        set { PlayerPrefs.SetInt("vibration", value ? 1 : 0); }
    }
    private void Start()
    {
        //Vibration.Init();
        Application.targetFrameRate = 60;
        AdManager.instance?.ShowBanner();
    }
    public void StartGame()
    {
        gameStatus = GameStatus.Playing;
    }
    public void BackToHome()
    {
        gameStatus = GameStatus.None;
        LevelManager.i.RestartLevel();
        UIManager.i.mainMenu.gameObject.SetActive(true);
    }
    public void ContinuePlay()
    {
        gameStatus = GameStatus.Playing;

    }
    public void PauseGame()
    {
        gameStatus = GameStatus.Paused;
    }
    public void CheckWinCondition()
    {
        if (LevelManager.i.GetAllTrays().Count(x => x.gameObject.activeSelf) > 0) return;
        CallEnding(true);
    }
    public void CheckLoseCondition()
    {
        if (LevelManager.i.coffeeWaitingQueue.trayHolder.CountNullSlot() > 0) return;
        if (LevelManager.i.coffeeWaitingQueue.trayHolder.IsATrayFull()) return;

        if (trayHolder.slotTransforms.All(x => x.isUnlocked))
        {
            CallEnding(false);
        }
        else
        {
            GlobalAudioPlayer.PlaySFX("OpenPop");
            UIManager.i.reviveUI.gameObject.SetActive(true);
        }

    }
    public void CallEnding(bool isWin)
    {
        if (isWin)
        {
            GlobalAudioPlayer.PlaySFX("Victory");
            UIManager.i.victory.gameObject.SetActive(true);
        }
        else
        {
            GlobalAudioPlayer.PlaySFX("Lose");
            UIManager.i.lose.gameObject.SetActive(true);

        }
    }

}
