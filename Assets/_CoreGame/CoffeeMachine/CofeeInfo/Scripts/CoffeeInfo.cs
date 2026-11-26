using System;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class CoffeeInfo : MonoBehaviour
{
    [Header("Flag")]
    public bool isInTray = false;
    public bool readyToTake = false;
    [Header("Info")]
    public CoffeeColor coffeeColor;
    public Vector3[] pathPoints;

    public void MoveAlongPath(Vector3[] points, Action action = null)
    {

        pathPoints = points;

        Vector3 currentPosition = transform.position;


        List<Vector3> subPath = new List<Vector3>();
        bool startAdding = false;



        foreach (var point in pathPoints)
        {
            if (startAdding)
            {
                subPath.Add(point);
            }

            if (Vector3.Distance(currentPosition, point) < 0.5f && !startAdding)
            {
                startAdding = true;
            }
        }

        if (subPath.Count > 0)
        {
            DOTween.Kill(transform);
            readyToTake = false;
            transform.DOPath(subPath.ToArray(), 20f, PathType.CatmullRom, PathMode.Full3D)
                            .SetEase(Ease.OutCubic)
                             .SetLookAt(0f)
                             .OnComplete(() =>
                             {
                                 readyToTake = true;
                                 action?.Invoke();
                             }).SetSpeedBased();

        }
    }

}

