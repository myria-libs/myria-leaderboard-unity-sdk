using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager.UI;
#endif
using UnityEngine;
using MyriaLeaderboard.MyriaLeaderboardEnums;

namespace MyriaLeaderboard.MyriaLeaderboardEnums
{
    public enum MyriaEnvironment { Prod, PreProd, Staging, Dev };
}

namespace MyriaLeaderboard
{
    public class MyriaLeaderboardConfig : ScriptableObject
    {

        [HideInInspector] private static readonly string UrlAppendage = "/v1";
        // [HideInInspector] private static readonly string AdminUrlAppendage = "/admin";
        // [HideInInspector] private static readonly string PlayerUrlAppendage = "/player";
        // [HideInInspector] private static readonly string UserUrlAppendage = "/game";

        [HideInInspector] public static string baseUrl = "";

        [HideInInspector] public string url = baseUrl + UrlAppendage;
        // [HideInInspector] public string adminUrl = baseUrl + AdminUrlAppendage;
        // [HideInInspector] public string playerUrl = baseUrl + PlayerUrlAppendage;
        // [HideInInspector] public string userUrl = baseUrl + UserUrlAppendage;
        [HideInInspector] public float clientSideRequestTimeOut = 180f;

        public (string key, string value) dateVersion = ("LL-Version", "2024-08-27");
        public string xDeveloperApiKey;
        public string publicKey;
        public MyriaEnvironment env;
        //[HideInInspector]
//         public string token;
// #if UNITY_EDITOR
//         [HideInInspector]
//         public string adminToken;
// #endif
//         [HideInInspector]
//         public string refreshToken;
        [HideInInspector]
        public string domainKey;
//         [HideInInspector]
//         public int gameID;
//         public string game_version = "1.0.0.0";
//         [HideInInspector] 
//         public string sdk_version = "";
//         [HideInInspector]
//         public string deviceID = "defaultPlayerId";
        public enum DebugLevel { All, ErrorOnly, NormalOnly, Off , AllAsNormal}
        public DebugLevel currentDebugLevel = DebugLevel.All;


        private static MyriaLeaderboardConfig settingsInstance;

        public virtual string SettingName { get { return "MyriaLeaderboardConfig"; } }

        private static MyriaLeaderboardConfig _current;

        public static MyriaLeaderboardConfig current
        {
            get
            {
                if (_current == null)
                {
                    _current = Get();
                }

                return _current;
            }
        }

        private void ConstructUrls()
        {
            string startOfUrl = "";
            switch (env)
            {
                case MyriaEnvironment.Prod:
                    startOfUrl = "https://prod.myriaverse-leaderboard-api.nonprod-myria.com";
                    break;
                case MyriaEnvironment.PreProd:
                    startOfUrl = "https://preprod.myriaverse-leaderboard-api.nonprod-myria.com";
                    break;
                case MyriaEnvironment.Staging:
                    startOfUrl = "https://staging.myriaverse-leaderboard-api.nonprod-myria.com";
                    break;
                case MyriaEnvironment.Dev:
                    startOfUrl = "https://staging.myriaverse-leaderboard-api.nonprod-myria.com";
                    break;
                default:
                    startOfUrl = "https://staging.myriaverse-leaderboard-api.nonprod-myria.com";
                    break;
            }
            if (!string.IsNullOrEmpty(domainKey))
            {
                startOfUrl += domainKey + ".";
            }
            //adminUrl = startOfUrl + AdminUrlAppendage;
            //playerUrl = startOfUrl + PlayerUrlAppendage;
            //userUrl = startOfUrl + UserUrlAppendage;
            url = startOfUrl;
        }

        public static MyriaLeaderboardConfig Get() {
            if (settingsInstance != null)
            {
                settingsInstance.ConstructUrls();
#if MYRIA_LEADERBOARD_COMMANDLINE_SETTINGS
                settingsInstance.CheckForSettingOverrides();
#endif
                return settingsInstance;
            }

            //Try to load it
            settingsInstance = Resources.Load<MyriaLeaderboardConfig>("Config/MyriaLeaderboardConfig");

#if UNITY_EDITOR
            // Could not be loaded, create it
            if (settingsInstance == null)
            {
                // Create a new Config
                MyriaLeaderboardConfig newConfig = ScriptableObject.CreateInstance<MyriaLeaderboardConfig>();

                // Folder needs to exist for Unity to be able to create an asset in it
                string dir = Application.dataPath+ "/MyriaLeaderboardSDK/Resources/Config";

                // If directory does not exist, create it
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // Create config asset
                string configAssetPath = "Assets/MyriaLeaderboardSDK/Resources/Config/MyriaLeaderboardConfig.asset";
                AssetDatabase.CreateAsset(newConfig, configAssetPath);
                EditorApplication.delayCall += AssetDatabase.SaveAssets;
                AssetDatabase.Refresh();
                settingsInstance = newConfig;
            }

#else
            if (settingsInstance == null)
            {
                throw new ArgumentException("MyriaLeaderboard config does not exist. To fix this, play once in the Unity Editor before making a build.");
            }
#endif
            settingsInstance.ConstructUrls();
#if MYRIA_LEADERBOA_COMMANDLINE_SETTINGS
            settingsInstance.CheckForSettingOverrides();
#endif
            return settingsInstance;
        }

        public static bool CreateNewSettings(string xDeveloperApiKey, string publicKey, MyriaEnvironment env, string gameVersion, string domainKey, MyriaLeaderboardConfig.DebugLevel debugLevel = DebugLevel.All, string baseURLParam = null, bool allowTokenRefresh = false)
        {
            _current = Get();
            _current.xDeveloperApiKey = xDeveloperApiKey;
            _current.publicKey = publicKey;
            //_current.baseUrl = string.IsNullOrEmpty(baseURLParam) ? _current.baseUrl : baseURLParam;
            _current.env = env;
            // _current.game_version = gameVersion;
            // _current.currentDebugLevel = debugLevel;
            // _current.allowTokenRefresh = allowTokenRefresh;
            _current.domainKey = domainKey;
            _current.ConstructUrls();
            Debug.Log("_current" + _current.env + _current.xDeveloperApiKey);
            return true;
        }

        private void CheckForSettingOverrides()
        {
        #if MYRIA_LEADERBOARD_COMMANDLINE_SETTINGS
            string[] args = System.Environment.GetCommandLineArgs();
            string _xDeveloperApiKey = null;
            string _publicKey = null;
            string _baseUrl = null;
            string _domainKey = null;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-xDeveloperApiKey")
                {
                    _xDeveloperApiKey = args[i + 1];
                }
                else (args[i] == "-publicKey") {
                    _publicKey = args[i+1];
                }
                else if (args[i] == "-domainkey")
                {
                    _domainKey = args[i + 1];
                }
                else if (args[i] == "-baseURL")
                {
                    _baseURL = args[i + 1];
                }
            }

            if (string.IsNullOrEmpty(_xDeveloperApiKey) || string.IsNullOrEmpty(_publicKey) || string.IsNullOrEmpty(_domainKey) || string.IsNullOrEmpty(_baseURL))
            {
                return;
            }
            publicKey = _publicKey;
            xDeveloperApiKey = _xDeveloperApiKey;
            domainKey = _domainKey;
            baseURL = _baseURL;
        #endif
        }
    }
}