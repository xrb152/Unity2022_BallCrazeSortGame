using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : Singleton<TutorialManager>
{
    public CoffeeWaitingQueue coffeeWaitingQueue;
    public GameObject handPrefab;
    public static bool isTutorial;

    private List<Tray> trays = new List<Tray>();
    public void StartTutorial()
    {
        isTutorial = true;
        trays = LevelManager.i.GetAllTrays();
    }
    private void Update()
    {
        if (isTutorial)
        {
            ProcessTutorial();
        }
    }

    GameObject handPointing;
    Tray targetTray;
    ECoffeeColor targetColor;
    private void ProcessTutorial()
    {


        foreach (CoffeeInfo coffeeInfo in coffeeWaitingQueue.coffeeInfos)
        {
            targetColor = coffeeInfo.coffeeColor.eCofeeColor;
            targetTray = trays.Find(x => x.readyToTake && x.typeOfCoffee == targetColor);
            if (targetTray != null) break;
        }

        if (targetTray == null)
        {
            if (handPointing.activeSelf) handPointing.SetActive(false);
            isTutorial = false;
            return;
        }

        if (!targetTray.readyToTake) targetTray = null;

        if (handPointing == null)
        {
            handPointing = Instantiate(handPrefab, UIManager.i.inGame);
        }

        if (!handPointing.activeSelf) handPointing.SetActive(true);
        LockTargetForHand(targetTray.transform);

    }
    private void LockTargetForHand(Transform target)
    {
        Vector3 viewportPos = GameManager.i.mainCamera.WorldToViewportPoint(target.position);
        Vector3 canvasPos = new Vector3(
            viewportPos.x * UIManager.i.mainCanvas.sizeDelta.x - UIManager.i.mainCanvas.sizeDelta.x * 0.5f,
            viewportPos.y * UIManager.i.mainCanvas.sizeDelta.y - UIManager.i.mainCanvas.sizeDelta.y * 0.5f,
            0
        );

        handPointing.transform.localPosition = canvasPos;
    }

}
