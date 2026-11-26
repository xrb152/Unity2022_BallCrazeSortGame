using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 修复抖音小游戏打包报错：
/// "Error building Player: Incompatible color space with graphics API"
///
/// 原因：工程 Color Space = Linear（URP 项目），而 TTSDK 打包设置 isWebGL2 = false 时
/// 会把 WebGL 的 Graphics API 强制成 WebGL 1.0（OpenGLES2），Unity 不允许 Linear + WebGL1，
/// 于是在 BuildPlayer 阶段直接失败。
///
/// 处理：把 TTSDK 的 isWebGL2 打开，同时把 WebGL 的 Graphics API 设为 WebGL 2.0（OpenGLES3）。
/// 菜单：Tools/TTSDK/修复 WebGL2 打包设置
/// </summary>
public static class TTSDKWebGL2Fix
{
    const string SettingAssetPath = "Assets/Editor/StarkBuilderSetting.asset";

    [MenuItem("Tools/TTSDK/修复 WebGL2 打包设置")]
    public static void FixWebGL2()
    {
        Debug.Log("========== [TTSDK Fix] 开始修复 ==========");
        Debug.Log($"[TTSDK Fix] 当前 Color Space = {PlayerSettings.colorSpace}");
        LogCurrentApis("修复前");

        // 1) 打开发布设置里的 WebGL 2.0
        var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(SettingAssetPath);
        if (so == null)
        {
            Debug.LogError($"[TTSDK Fix] 找不到 {SettingAssetPath}，请确认 TTSDK 打包设置资源路径。");
        }
        else
        {
            var ser = new SerializedObject(so);
            var prop = ser.FindProperty("isWebGL2");
            if (prop == null)
            {
                Debug.LogWarning("[TTSDK Fix] 该资源里没有 isWebGL2 字段，请在 TTSDK 打包窗口里手动勾选「WebGL2.0」。");
            }
            else
            {
                prop.boolValue = true;
                ser.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(so);
                Debug.Log("[TTSDK Fix] 已把 StarkBuilderSetting.isWebGL2 设为 true（即勾选 WebGL2.0）。");
            }
        }

        // 2) Player Settings 里 WebGL 的 Graphics API 改为 WebGL 2.0
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, new[] { GraphicsDeviceType.OpenGLES3 });
        LogCurrentApis("修复后");

        AssetDatabase.SaveAssets();

        if (PlayerSettings.colorSpace == ColorSpace.Linear)
        {
            Debug.Log("[TTSDK Fix] Color Space 保持 Linear，WebGL2.0 支持 Linear，无需改动画面设置。");
        }
        else
        {
            Debug.Log("[TTSDK Fix] 当前为 Gamma 色彩空间，WebGL1/2 都可用，若只是想最大兼容可把 isWebGL2 关掉。");
        }

        Debug.Log("========== [TTSDK Fix] 修复完成，请重新打包 ==========");
    }

    static void LogCurrentApis(string tag)
    {
        var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.WebGL);
        var names = Array.ConvertAll(apis, a => a.ToString());
        Debug.Log($"[TTSDK Fix] {tag} WebGL Graphics API = [{string.Join(", ", names)}]");
    }
}
