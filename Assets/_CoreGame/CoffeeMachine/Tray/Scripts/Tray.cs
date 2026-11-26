using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MEC;
using Observer;
using UnityEngine;

public class Tray : MonoBehaviour
{
    [Header("Flag")]
    public bool isInBar = false;
    public bool readyToTake = true;
    public bool canShuffle = true;
    public bool blocked = false;
    public bool isUsingBooster = false;
    [Header("Info")]
    public Animator animator;
    public TrayType size;
    public ECoffeeColor typeOfCoffee;
    public Transform slotTransformsParent;
    public Transform rayParent;
    public ParticleSystem completeEffect;
    public Dictionary<Transform, CoffeeInfo> traySlots;
    public Collider trayCollider;
    private void Awake()
    {
        traySlots = new Dictionary<Transform, CoffeeInfo>();
        foreach (Transform slot in slotTransformsParent)
        {
            traySlots[slot] = null;
        }
        animator = GetComponentInChildren<Animator>();
    }
    private void Update()
    {
        if (readyToTake)
            CheckRays();
    }
    private void CheckRays()
    {
        foreach (Transform point in rayParent)
        {
            RaycastHit hit;
            if (Physics.Raycast(point.position, Vector3.up, out hit, 5f))
            {
                if (!blocked)
                {
                    blocked = true;
                    MeshRenderer[] meshRenderers = transform.GetComponentsInChildren<MeshRenderer>();
                    foreach (MeshRenderer mesh in meshRenderers)
                    {
                        if (mesh.gameObject.name == "Khay" || mesh.gameObject.name == "khay") continue;

                        Material[] trayMat = mesh.materials;
                        Color newColor;
                        ColorUtility.TryParseHtmlString("#848484", out newColor);
                        trayMat[0].SetColor("_GoochBrightColor", newColor);
                        mesh.materials = trayMat;
                    }
                }

                Debug.DrawRay(point.position, Vector3.up * 5f, Color.red);
                return;
            }
            else
            {
                Debug.DrawRay(point.position, Vector3.up * 5f, Color.green);
            }
        }
        if (blocked)
        {
            blocked = false;
            MeshRenderer[] meshRenderers = transform.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mesh in meshRenderers)
            {
                if (mesh.gameObject.name == "Khay" || mesh.gameObject.name == "khay") continue;

                Material[] trayMat = mesh.materials;
                trayMat[0].SetColor("_GoochBrightColor", Color.white);
                mesh.materials = trayMat;
            }
        }
    }
    public bool IsFull()
    {
        return traySlots.All(x => x.Value != null);
    }

    public bool CheckAvailableSlot(CoffeeInfo coffeeInfo)
    {
        for (int i = 0; i < slotTransformsParent.childCount; i++)
        {
            if (traySlots[slotTransformsParent.GetChild(i)] == null)
            {
                AddCoffee(coffeeInfo, i);
                return true;
            }
        }
        return false;

    }
    public int CountAvailableSlot()
    {
        return traySlots.Count(x => x.Value == null);
    }
    private void AddCoffee(CoffeeInfo coffeeInfo, int index)
    {
        traySlots[slotTransformsParent.GetChild(index)] = coffeeInfo;

        Transform targetSlot = slotTransformsParent.GetChild(index);
        coffeeInfo.transform.parent = targetSlot;
        traySlots[targetSlot] = coffeeInfo;

        DOTween.Kill(coffeeInfo.transform);

        coffeeInfo.transform.DOJump(targetSlot.position, 5, 1, 0.5f).OnComplete(() =>
        {
            transform.DOScale(1.1f, 0.1f).SetEase(Ease.Linear).OnComplete(() =>
            {
                transform.DOScale(1f, 0.1f).SetEase(Ease.Linear);
            });
            GlobalAudioPlayer.PlaySFX("BallDrop");
            coffeeInfo.isInTray = true;
            if (traySlots.All(x => x.Value != null && x.Value.isInTray))
            {
                GlobalAudioPlayer.PlaySFX("BoxComplete");
                completeEffect.Play();
                CloseTrayAndMove();
            }
        }).SetEase(Ease.InQuad);


    }

    private void CloseTrayAndMove()
    {
        this.PostEvent(EventID.OnCloseBox, this);
        transform.parent = null;
        animator.SetTrigger("Close");
        transform.DOMove(new Vector3(transform.position.x, 2f, 8f), 0.3f).SetEase(Ease.Linear).OnComplete(() =>
        {
            Timing.RunCoroutine(_DelaySound());

            transform.DOMoveX(transform.position.x + 20f, 20f).SetEase(Ease.InBack).OnComplete(() =>
            {
                gameObject.SetActive(false);
                GameManager.i.CheckWinCondition();
            }).SetSpeedBased();


        });
    }
    private IEnumerator<float> _DelaySound()
    {
        yield return Timing.WaitForSeconds(0.75f);
        GlobalAudioPlayer.PlaySFX("BoxFlying");

    }

}
