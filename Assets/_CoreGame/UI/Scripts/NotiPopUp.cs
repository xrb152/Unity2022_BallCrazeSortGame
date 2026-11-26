using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class NotiPopUp : MonoBehaviour
{
     public CanvasGroup group;
     private void OnEnable()
     {
          if (group == null)
               group = GetComponent<CanvasGroup>();

          transform.localPosition = Vector3.zero;
          transform.DOLocalMoveY(transform.localPosition.y + 150f, 1f);
          group.DOFade(0f, 0.5f).OnComplete(() => gameObject.SetActive(false)).SetDelay(0.5f);
          group.alpha = 1f;
     }



}
