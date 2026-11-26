using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public enum TypeBooster
{
    SortCoffee,
    ShuffleTray,
    ClearTray,
    MegaClearTray
}
[System.Serializable]
public class Booster
{
    public string nameBooster;
    public TypeBooster typeBooster;
    public Sprite ic_booster;
    public string context;
    public int price;
    public int levelUnlock;
}
[CreateAssetMenu(fileName = "BoosterInfo", menuName = "Tiko/Booster")]
// 这类资源是用 Resources.Load("ScriptableObject/" + 类名) 取的，类名被混淆就再也找不到，
// 所以禁止混淆器改它的类名。
[OPS.Obfuscator.Attribute.DoNotObfuscateClass]
public class BoosterInfo : SingletonScriptableObject<BoosterInfo>
{
    public List<Booster> boosters;
    public Booster GetInfoBooster(TypeBooster typeBooster)
    {
        return boosters.Find(x => x.typeBooster == typeBooster);
    }
}
