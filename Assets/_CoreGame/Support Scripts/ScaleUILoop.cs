using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ScaleUILoop : MonoBehaviour
{
    public Vector3 targetScale ;
    public float timeScale = 1f;
    public Ease ease = Ease.Linear;
    public float delayLoop=0;
    private void OnEnable()
    {
        this.transform.DOScale(targetScale, timeScale).SetEase(ease).SetLoops(-1, LoopType.Yoyo).SetDelay(delayLoop);
    }
    private void OnDisable()
    {
        DOTween.Kill(this.transform);
    }
}
