using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ScaleOnEnable : MonoBehaviour
{
     public Vector3 targetScaleValue = Vector3.one; // Default to (1,1,1)
     public Transform StartPos;
     public bool isSetPos = true;
     public Vector2 EndPos;
     public float moveTime = 0.5f;
     public Ease easeMove = Ease.Linear; // Default ease for movement
     public float scaleTime = 0.5f;
     public Ease easeScale = Ease.Linear; // Default ease for scaling
     public bool setUpdate;
     private RectTransform rect;
     public float delay = 1f; // Delay before animations start

     private void Start()
     {
          rect = GetComponent<RectTransform>();
     }

     private void OnEnable()
     {
          // Ensure rect is set
          if (rect == null)
          {
               rect = GetComponent<RectTransform>();
          }

          // Initialize the scale to zero
          rect.localScale = Vector3.zero;

          // Create a sequence to combine both animations
          Sequence mySequence = DOTween.Sequence();
          UpdateType updateType = setUpdate ? UpdateType.Late : UpdateType.Normal;
          // Add delay to the entire sequence
          mySequence.AppendInterval(delay);

          // Move to EndPos if isSetPos is true
          if (isSetPos && StartPos != null)
          {
               //rect.position = StartPos.position;
               mySequence.Append(rect.DOMove(StartPos.position, 0f).SetEase(easeMove)).SetUpdate(updateType, setUpdate);
               mySequence.Append(rect.DOAnchorPos(EndPos, moveTime).SetEase(easeMove)).SetUpdate(updateType, setUpdate);
          }
          else
          {
               // If isSetPos is false, still add a placeholder animation to maintain delay
               mySequence.AppendInterval(0.001f).SetUpdate(updateType, setUpdate); // Small delay to maintain sequence structure
          }

          // Scale to targetScaleValue in both cases
          mySequence.Join(rect.DOScale(targetScaleValue, scaleTime).SetEase(easeScale)).SetUpdate(updateType, setUpdate);

          // Play the sequence
          mySequence.Play();
     }
}
