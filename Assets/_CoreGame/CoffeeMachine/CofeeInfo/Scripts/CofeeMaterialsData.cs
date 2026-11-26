using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "CofeeMaterialsData", menuName = "Tiko/CofeeData/CofeeColor")]
// Resources.Load 依赖类名，禁止混淆器改名
[OPS.Obfuscator.Attribute.DoNotObfuscateClass]
public class CofeeMaterialsData : SingletonScriptableObject<CofeeMaterialsData>
{
    public CoffeeColor[] cofeeColors;
    public CoffeeColor GetColor(ECoffeeColor eCofeeColor)
    {
        return cofeeColors[(int)eCofeeColor];
    }
}
[System.Serializable]
public class CoffeeColor
{
    public ECoffeeColor eCofeeColor;
    public Material material;
}
[System.Serializable]
public enum ECoffeeColor
{
    yellow = 0,
    red = 1,
    pink = 2,
    purple = 3,
    blue = 4,
    green = 5,
    orange = 6
}
