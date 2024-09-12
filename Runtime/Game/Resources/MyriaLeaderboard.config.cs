using System;
using System.IO;
using UnityEngine;
using MyriaLeaderboard.MyriaLeaderboardEnums;
using MyriaLeaderboard.MyriaLeaderboardExtension;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager.UI;
#endif

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

        public static string baseUrl = "";

        [HideInInspector] public string url = string.IsNullOrEmpty(baseUrl) ? (baseUrl + UrlAppendage) : baseUrl;
        // [HideInInspector] public string adminUrl = baseUrl + AdminUrlAppendage;
        // [HideInInspector] public string playerUrl = baseUrl + PlayerUrlAppendage;
        // [HideInInspector] public string userUrl = baseUrl + UserUrlAppendage;
        [HideInInspector] public float clientSideRequestTimeOut = 180f;
        [HideInInspector] public bool allowCustomUrl;

        public string developerApiKey;
        public string publicKey;
        public MyriaEnvironment env;
        [HideInInspector] public string customUrl;
        [HideInInspector] public string xApiKey;

        [HideInInspector] public string domainKey;
        [HideInInspector] public enum DebugLevel { All, ErrorOnly, NormalOnly, Off , AllAsNormal}
        [HideInInspector] public DebugLevel currentDebugLevel = DebugLevel.All;


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
            if (allowCustomUrl && !string.IsNullOrEmpty(customUrl))
            {
                url = customUrl;
            } else
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
                url = startOfUrl + UrlAppendage;
            }
            
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

        public static bool CreateNewSettings(string developerApiKey, string publicKey, MyriaEnvironment env, string gameVersion, string domainKey, MyriaLeaderboardConfig.DebugLevel debugLevel = DebugLevel.All, string baseUrl = null)
        {
            _current = Get();
            _current.developerApiKey = developerApiKey;
            _current.publicKey = publicKey;
            
            string newUrl = string.IsNullOrEmpty(baseUrl) ? _current.url : baseUrl;
            _current.url = newUrl;
            _current.env = env;
            _current.xApiKey = EncryptExtentsions.EncryptApiKey(developerApiKey, publicKey);
             //_current.game_version = gameVersion;
             //_current.currentDebugLevel = debugLevel;
            // _current.allowTokenRefresh = allowTokenRefresh;
            _current.domainKey = domainKey;
            return true;
        }

        private void CheckForSettingOverrides()
        {
#if MYRIA_LEADERBOARD_COMMANDLINE_SETTINGS
            string[] args = System.Environment.GetCommandLineArgs();
            string _developerApiKey = null;
            string _baseUrl = null;
            string _domainKey = null;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-developerApiKey")
                {
                    _developerApiKey = args[i + 1];
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

            if (string.IsNullOrEmpty(_developerApiKey) || string.IsNullOrEmpty(_domainKey) || string.IsNullOrEmpty(_baseURL))
            {
                return;
            }
            developerApiKey = _developerApiKey;
            domainKey = _domainKey;
            baseURL = _baseURL;
#endif
        }
    }
}