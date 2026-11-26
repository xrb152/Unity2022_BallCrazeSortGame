using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public enum LevelTheme
{
    BasketBall = 0,
    PickleBall = 1,
    SoccerBall = 2,
    TennisBall = 3,
    VolleyBall = 4,
}
[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public int reward;
    public LevelTheme theme;
    public List<LayerData> layers = new List<LayerData>();

    public int GetTotalSlots()
    {
        int totalSlots = 0;
        foreach (var layer in layers)
        {
            foreach (var tray in layer.trays)
            {
                totalSlots += (int)tray.size;
            }
        }
        return totalSlots;
    }



}

