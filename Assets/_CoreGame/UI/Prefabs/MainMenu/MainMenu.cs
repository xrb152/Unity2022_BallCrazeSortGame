using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button startGameBtn;
    private void Start()
    {
        startGameBtn.onClick.AddListener(() =>
        {
            GlobalAudioPlayer.PlaySFX("Button");
            gameObject.SetActive(false);
            GameManager.i.StartGame();
        });
    }
}
