using UnityEngine;

/// <summary>
/// Resources 里的「全局单例 ScriptableObject」。
///
/// 注意：原来只用 "ScriptableObject/" + typeof(T).Name 按类名加载，打成发布包时会被
/// OPS Obfuscator 把类名改掉（日志里 BoosterInfo → bn、CofeeMaterialsData / TrayData 同理），
/// 于是 Resources.Load 取不到资源，i 变成 null：
///   - BoosterBtn.Start() 取 booster.ic_booster 直接空引用 → 底部 4 个道具按钮变白块、点击无反应
///   - TrayData / CofeeMaterialsData 为 null → 托盘、咖啡球不生成 → 场景下方托盘区一片空白
///   - 每帧/每次调用都抛 NullReferenceException，控制台刷满红字
///
/// 所以先用类名找（兼容没混淆的编辑器/旧包），找不到就用「按类型」兜底，
/// 类名被混淆也照样能拿到资源。
/// </summary>
public class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
{
    private static T instance;

    public static T i
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<T>("ScriptableObject/" + typeof(T).Name);

                if (instance == null)
                {
                    T[] all = Resources.LoadAll<T>("ScriptableObject");
                    if (all != null && all.Length > 0)
                    {
                        instance = all[0];
                        if (all.Length > 1)
                        {
                            Debug.LogWarning($"[SingletonScriptableObject] Resources/ScriptableObject 下有 {all.Length} 个 {typeof(T).Name}，已取第一个：{all[0].name}");
                        }
                    }
                }

                if (instance == null)
                {
                    Debug.LogError($"SingletonScriptableObject: No instance of {typeof(T).Name} found in Resources.");
                }
            }

            return instance;
        }
    }
}
