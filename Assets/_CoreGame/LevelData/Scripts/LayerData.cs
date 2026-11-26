using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "LayerData", menuName = "Game/Layer Data")]
public class LayerData : ScriptableObject
{
    public List<TrayData> trays = new List<TrayData>();

    [System.Serializable]
    public class TrayData
    {
        public TrayType size;
        public ECoffeeColor trayColor;
        public Vector2 position;
        public TrayDirection direction;
    }
}

