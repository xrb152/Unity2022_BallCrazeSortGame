
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Firebase;
// using Firebase.RemoteConfig;
// using Firebase.Extensions;
// using System.Threading.Tasks;
// using System;

// [System.Serializable]
// public class Config_Time
// {
//     public float time_inter;
//     public float time_inter_afk;
//     public float time_inter_btn;
// }
// public class AnalyticManager : MonoBehaviour
// {
//     public static AnalyticManager i;
//     [Header("key remote config")]
//     public static string Key_ConfigTime = "Config_Time";
//     [Header("value default")]
//     [SerializeField] private string config_time;
//     [Header("config value")]
//     public Config_Time config_Time;
//     FirebaseApp app;
//     private void Awake()
//     {
//         i = this;
//     }
//     void Start()
//     {
//         Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
//         {
//             var dependencyStatus = task.Result;
//             if (dependencyStatus == Firebase.DependencyStatus.Available)
//             {
//                 // Create and hold a reference to your FirebaseApp,
//                 // where app is a Firebase.FirebaseApp property of your application class.
//                 InitData();
//                 app = Firebase.FirebaseApp.DefaultInstance;
//                 // LogEventStartGame();
//                 // Set a flag here to indicate whether Firebase is ready to use by your app.
//             }
//             else
//             {
//                 UnityEngine.Debug.LogError(System.String.Format(
//                   "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
//                 // Firebase Unity SDK is not safe to use here.
//             }
//         });
//     }
//     #region  Remote Config
//     public void InitData()
//     {
//         System.Collections.Generic.Dictionary<string, object> defaults =
//   new System.Collections.Generic.Dictionary<string, object>();

//         // These are the values that are used if we haven't fetched data from the
//         // server
//         // yet, or if we ask for values that the server doesn't have:
//         defaults.Add(Key_ConfigTime, config_time);

//         FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults);
//         FetchDataAsync();
//     }
//     private void FetchComplete(Task fetchTask)
//     {
//         if (!fetchTask.IsCompleted)
//         {
//             Debug.LogError("Retrieval hasn't finished.");
//             return;
//         }

//         var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
//         var info = remoteConfig.Info;
//         if (info.LastFetchStatus != LastFetchStatus.Success)
//         {
//             Debug.LogError($"{nameof(FetchComplete)} was unsuccessful\n{nameof(info.LastFetchStatus)}: {info.LastFetchStatus}");
//             return;
//         }
//         // Fetch successful. Parameter values must be activated to use.
//         remoteConfig.ActivateAsync()
//           .ContinueWithOnMainThread(
//             task =>
//             {
//                 Debug.Log($"Remote data loaded and ready for use. Last fetch time {info.FetchTime}.");
//                 SetData();
//             });
//     }
//     public Task FetchDataAsync()
//     {
//         Debug.Log("Fetching data...");
//         System.Threading.Tasks.Task fetchTask =
//         Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(
//             TimeSpan.Zero);
//         return fetchTask.ContinueWithOnMainThread(FetchComplete);
//     }
//     void SetData()
//     {
//         Debug.Log("Load remote config success === assign data ");
//         string timeString = FirebaseRemoteConfig.DefaultInstance.GetValue(Key_ConfigTime).StringValue;
//         Debug.Log(timeString);

//         config_Time = JsonUtility.FromJson<Config_Time>(timeString);
//         AdManager.instance.SetTime();
//     }

//     #endregion
//     public static void LogEventStartGame()
//     {
//         Firebase.Analytics.FirebaseAnalytics.LogEvent("start_level", "total_join", PlayerPrefs.GetInt("joingame", 0));
//         PlayerPrefs.SetInt("joingame", PlayerPrefs.GetInt("joingame", 0) + 1);
//     }
//     public static void LogEventStartLevel(int level)
//     {
//         Firebase.Analytics.FirebaseAnalytics.LogEvent("start_level", "level", level);
//     }
//     public static void LogEventLevelComplete(int level)
//     {
//         Firebase.Analytics.FirebaseAnalytics.LogEvent("level_complete", "level", level);
//     }
//     public static void LogEventLevelFail(int level)
//     {
//         Firebase.Analytics.FirebaseAnalytics.LogEvent("level_fail", "level", level);
//     }
//     public static void LogEventAdsInter()
//     {
//         Firebase.Analytics.FirebaseAnalytics.LogEvent("inter_ads");
//     }
//     public static void LogEventAdsReward()
//     {
//         Firebase.Analytics.FirebaseAnalytics.LogEvent("reward_ads");
//     }
//     public static void LogEventContent(string content)
//     {
//         Firebase.Analytics.FirebaseAnalytics.LogEvent(content);
//     }

// }
