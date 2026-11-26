using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Observer;
using UnityEngine;

public class TrayHolder : MonoBehaviour
{
    public Slot[] slotTransforms;
    public Dictionary<Slot, Tray> barSlots;
    private void Awake()
    {
        barSlots = new Dictionary<Slot, Tray>();
        foreach (var slot in slotTransforms)
        {
            barSlots[slot] = null;
        }
        ResetStatusSlot();
    }
    private void OnEnable()
    {
        EventDispatcher.Instance.RegisterListener(EventID.OnCloseBox, HandleOnCloseBox);

    }
    private void OnDisable()
    {
        this.RemoveListener(EventID.OnCloseBox, HandleOnCloseBox);
    }
    public int CountNullSlot()
    {
        int count = barSlots.Count(x => x.Key.isUnlocked && x.Value == null);

        return count;
    }
    public bool IsATrayFull()
    {


        foreach (Tray tray in barSlots.Values)
        {
            if (tray == null) continue;
            if (tray.isUsingBooster) return true;
            if (tray.IsFull())
            {
                return true;
            }
        }
        return false;
    }
    public void ClearTray()
    {
        for (int i = 0; i < barSlots.Count; i++)
        {
            if (barSlots[slotTransforms[i]] != null)
            {
                Destroy(barSlots[slotTransforms[i]].gameObject);
                barSlots[slotTransforms[i]] = null;
            }
        }
        ResetStatusSlot();
    }
    public bool CheckAvailableSlot(Tray tray)
    {
        for (int i = 0; i < slotTransforms.Length; i++)
        {
            if (barSlots[slotTransforms[i]] == null && slotTransforms[i].isUnlocked)
            {

                AddTrayToBar(tray, i);
                tray.canShuffle = false;
                tray.trayCollider.enabled = false;
                return true;
            }
        }
        return false;

    }
    private void AddTrayToBar(Tray tray, int index)
    {
        barSlots[slotTransforms[index]] = tray;
        tray.readyToTake = false;

        Slot targetSlot = slotTransforms[index];
        tray.transform.parent = targetSlot.posHolder;
        barSlots[targetSlot] = tray;
        barSlots[targetSlot].animator.SetTrigger("Open");
        tray.transform.DOLocalRotateQuaternion(Quaternion.identity, 0.5f);

        Vector3 oldPosition = targetSlot.posHolder.position;
        //if (GameManager.vibration)
            //Vibration.VibratePop();
        Tween tween = tray.transform.DOJump(targetSlot.posHolder.position, 5, 1, 0.5f).OnComplete(() =>
          {
              targetSlot.boxInEff.Play();
              GlobalAudioPlayer.PlaySFX("BoxDrop");
              tray.isInBar = true;
              this.PostEvent(EventID.OnCallCheckTakeCoffee);
          });

        tween.OnUpdate(() =>
        {
            if (oldPosition != targetSlot.posHolder.position)
            {
                oldPosition = targetSlot.posHolder.position;
                tween.Kill();
                tray.transform.DOJump(targetSlot.posHolder.position, 5, 1, 0.5f).OnComplete(() =>
                {
                    targetSlot.boxInEff.Play();
                    GlobalAudioPlayer.PlaySFX("BoxDrop");
                    tray.isInBar = true;
                    this.PostEvent(EventID.OnCallCheckTakeCoffee);
                });
            }
        });
    }
    private void HandleOnCloseBox(object obj)
    {
        Tray tray = (Tray)obj;

        foreach (var key in barSlots.Keys.ToList())
        {
            if (barSlots[key] == null) continue;

            if (barSlots[key] == tray)
            {
                barSlots[key] = null;
                return;
            }
        }
    }
    public Tray FindTrayWithColor(ECoffeeColor eCoffeeColor)
    {
        foreach (var tray in barSlots.Values)
        {
            if (tray == null) continue;

            if (tray.typeOfCoffee == eCoffeeColor && tray.traySlots.Any(slot => slot.Value == null))
            {
                return tray;
            }
        }

        return null;
    }

    public float offset = 3.2f;
    private void ResetStatusSlot()
    {
        slotTransforms[slotTransforms.Length - 3].ToggleAds(false);
        slotTransforms[slotTransforms.Length - 2].ToggleAds(false);
        slotTransforms[slotTransforms.Length - 1].ToggleAds(false);

        slotTransforms[slotTransforms.Length - 2].gameObject.SetActive(false);
        slotTransforms[slotTransforms.Length - 1].gameObject.SetActive(false);

        slotTransforms[0].transform.parent.localPosition = Vector3.zero;
        Vector3 tempPos = slotTransforms[0].transform.parent.position;
        tempPos.x += offset;
        slotTransforms[0].transform.parent.position = tempPos;
    }
    public void UnlockNextSlotWithAds()
    {
        // 编辑器里也走同一条链路（TikTokPlatform 的编辑器分支会模拟一次广告），
        // 这样「点广告 → 看广告 → 回调解锁」能在 PC 上完整验证。
        AdManager.ShowRewardSafe(() =>
        {
            UnlockNextSlot();
        }, () =>
        {
            UIManager.i.Notification("广告暂不可用");
        }, "rv_unlock_slot");
    }
    public void UnlockNextSlot()
    {
        for (int i = 0; i < slotTransforms.Length; i++)
        {
            if (!slotTransforms[i].isUnlocked)
            {
                switch (i)
                {
                    case 4:
                        UnlockSlot5();
                        break;
                    case 5:
                        UnlockSlot6();
                        break;
                    case 6:
                        UnlockSlot7();
                        break;
                    default:
                        Debug.LogWarning("No matching logic for this slot index.");
                        break;
                }
                slotTransforms[i].isUnlocked = true;
                return;
            }
        }
    }
    private void UnlockSlot5()
    {
        Vector3 tempPos = slotTransforms[0].transform.parent.position;
        tempPos.x -= offset / 2f;
        slotTransforms[0].transform.parent.position = tempPos;
        slotTransforms[slotTransforms.Length - 3].ToggleAds(true);
        slotTransforms[slotTransforms.Length - 2].gameObject.SetActive(true);
    }
    private void UnlockSlot6()
    {
        slotTransforms[0].transform.parent.localPosition = Vector3.zero;
        slotTransforms[slotTransforms.Length - 2].ToggleAds(true);
        slotTransforms[slotTransforms.Length - 1].gameObject.SetActive(true);
    }
    private void UnlockSlot7()
    {
        slotTransforms[slotTransforms.Length - 1].ToggleAds(true);
    }
}
