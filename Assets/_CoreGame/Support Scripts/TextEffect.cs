using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MEC;
using Observer;
using UnityEngine;

public class TextEffect : MonoBehaviour
{
    public GameObject[] allImage;
    public Transform effectHolder;

    private void OnEnable()
    {
        EventDispatcher.Instance.RegisterListener(EventID.OnCloseBox, PlayEffect);
    }
    private void OnDisable()
    {
        this.RemoveListener(EventID.OnCloseBox, PlayEffect);
    }
    CoroutineHandle coroutine;
    public void PlayEffect(object obj)
    {
        foreach (GameObject image in allImage)
        {
            image.gameObject.SetActive(false);
        }
        int rdm = Random.Range(0, allImage.Length);

        allImage[rdm].SetActive(true);

        DOTween.Kill(effectHolder);
        effectHolder.localScale = Vector3.zero;
        effectHolder.DOScale(1f, 0.5f).SetEase(Ease.OutBack);

        if (coroutine != null) Timing.KillCoroutines(coroutine);
        coroutine = Timing.RunCoroutine(_DelayEffect());
    }
    IEnumerator<float> _DelayEffect()
    {
        yield return Timing.WaitForSeconds(0.75f);
        foreach (GameObject image in allImage)
        {
            image.gameObject.SetActive(false);
        }
    }
}
