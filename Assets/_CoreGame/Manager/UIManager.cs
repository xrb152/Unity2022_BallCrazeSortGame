using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject notificationPrefab;
    public RectTransform mainCanvas;
    public BoosterUI boosterUI;
    public TopUI topUI;
    public MainMenu mainMenu;
    public Transform inGame;
    public Setting setting;
    public Victory victory;
    public Lose lose;
    public ReviveUI reviveUI;
    private List<GameObject> notificationPool = new List<GameObject>();
    public void Notification(string text)
    {
        GameObject noti = notificationPool.Find(n => !n.activeInHierarchy);

        if (noti == null)
        {
            noti = Instantiate(notificationPrefab, mainCanvas.transform);
            notificationPool.Add(noti);
        }
        noti.GetComponent<TextMeshProUGUI>().text = text;
        noti.SetActive(true);
    }

}
