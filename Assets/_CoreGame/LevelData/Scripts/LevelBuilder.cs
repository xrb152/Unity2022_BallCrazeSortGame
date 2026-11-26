using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerBuilder : MonoBehaviour
{
    public bool debugLayer;
    public LayerData layerData;
    public float offsetLayer;
    private void OnDrawGizmos()
    {
        if (!debugLayer) return;
        if (layerData == null) return;


        foreach (var tray in layerData.trays)
        {
            Vector3 position = new Vector3(tray.position.x, 0, tray.position.y);

            Gizmos.color = GetColorFromTrayColorEnum(tray.trayColor);

            Quaternion rotation = Quaternion.Euler(0f, (int)tray.direction * 90f, 0f);
            Gizmos.matrix = Matrix4x4.TRS(position, rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(2f, 1f, (int)tray.size / 2) * 1.2f);
        }

    }
    private Color GetColorFromTrayColorEnum(ECoffeeColor trayColor)
    {
        switch (trayColor)
        {
            case ECoffeeColor.yellow:
                return Color.yellow;
            case ECoffeeColor.red:
                return Color.red;
            case ECoffeeColor.pink:
                return new Color(1f, 0.75f, 0.8f);
            case ECoffeeColor.purple:
                return new Color(0.5f, 0f, 0.5f);
            case ECoffeeColor.blue:
                return Color.blue;
            case ECoffeeColor.green:
                return Color.green;
            case ECoffeeColor.orange:
                return new Color(1f, 0.647f, 0f);
            default:
                return Color.white;
        }


    }
}
