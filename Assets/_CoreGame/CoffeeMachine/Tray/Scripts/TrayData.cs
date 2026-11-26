using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[System.Serializable]
public enum TrayDirection
{
    Vertical = 0, Horizontal = 1
}
[System.Serializable]
public enum TrayType
{
    _2x2 = 4, _2x3 = 6, _2x4 = 8, _2x5 = 10, _2x6 = 12, _2x7 = 14, _2x8 = 16
}
[System.Serializable]
public class TrayInfo
{
    public TrayType trayType;
    public GameObject trayPrefab;
}
[CreateAssetMenu(fileName = "TrayData", menuName = "Tiko/TrayData")]
// Resources.Load 依赖类名，禁止混淆器改名
[OPS.Obfuscator.Attribute.DoNotObfuscateClass]
public class TrayData : SingletonScriptableObject<TrayData>
{
    public TrayInfo[] trayInfos;
    public CoffeeColor[] trayColors;
    public CoffeeColor GetColor(ECoffeeColor eCofeeColor)
    {
        return trayColors[(int)eCofeeColor];
    }
    public GameObject GetTrayPrefab(TrayType trayType)
    {
        return trayInfos.FirstOrDefault(x => x.trayType == trayType).trayPrefab;
    }
}
