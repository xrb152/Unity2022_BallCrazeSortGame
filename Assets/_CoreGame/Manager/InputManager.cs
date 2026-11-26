using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public TrayHolder trayHolder;
    private void Update()
    {
        if (GameManager.i.gameStatus == GameStatus.None)
        {
            return;
        }
        HandleMouseInput();
    }
    public bool IsPointerOverUI()
    {
        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(0);
        }
        else
        {
            return EventSystem.current.IsPointerOverGameObject();
        }
    }
    Tray selectedTray;
    Slot selectedSlot;
    private void HandleMouseInput()
    {
        if (!IsPointerOverUI())
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.TryGetComponent(out selectedTray))
                    {
                        if (!selectedTray.blocked)
                        {
                            if (selectedTray.readyToTake && !selectedTray.isInBar)
                            {
                                GameManager.i.trayHolder.CheckAvailableSlot(selectedTray);
                            }
                        }
                        else
                        {
                            Transform childTransform = selectedTray.transform.GetChild(0);
                            childTransform.localPosition = Vector3.zero + Vector3.up * -0.6f;
                            DOTween.Kill(childTransform);
                            childTransform.DOShakePosition(1f, 0.25f, 10, 90, false, true);
                            GlobalAudioPlayer.PlaySFX("BoxLock");
                        }
                        selectedTray = null;
                    }
                    if (hit.transform.TryGetComponent(out selectedSlot))
                    {
                        if (!selectedSlot.isUnlocked)
                        {

                            GameManager.i.trayHolder.UnlockNextSlotWithAds();
                            selectedSlot = null;

                        }
                    }


                }


            }

        }
    }
}
