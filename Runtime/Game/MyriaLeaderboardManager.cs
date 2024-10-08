using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;
using MyriaLeaderboard;
using MyriaLeaderboard.Requests;
using MyriaLeaderboard.Response;
using MyriaLeaderboard.MyriaLeaderboardEnums;
using MyriaLeaderboard.MyriaLeaderboardExtension;
using System.Linq;
using System.Security.Cryptography;
#if MYRIA_LEADERBOARD_USE_NEWTONSOFTJSON
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#else
using LLlibs.ZeroDepJson;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MyriaLeaderboard
{
    public partial class MyriaLeaderboardSDKManager
    {
        #region Init
        static bool initialized;
        static bool Init()
        {
           MyriaLeaderboardServerApi.Instantiate();
           return LoadConfig();
        }
        #endregion

        /// <summary>
        /// Manually initialize the SDK.
        /// </summary>
        /// <returns>True if initialized successfully, false otherwise</returns>
        public static bool Init(string developerApiKey, string publicKey, MyriaEnvironment env, string gameVersion, string baseURL, MyriaLeaderboardConfig.DebugLevel debugLevel)
        {
            MyriaLeaderboardServerApi.Instantiate();
            MyriaLeaderboardConfig.CreateNewSettings(developerApiKey, publicKey, env, gameVersion, null, debugLevel, baseURL);
            return LoadConfig();
        }

        static bool LoadConfig()
        {
            initialized = false;
            if (MyriaLeaderboardConfig.current == null)
            {
                MyriaLeaderboardLogger.GetForLogLevel(MyriaLeaderboardLogger.LogLevel.Error)("SDK could not find settings, please contact support \n You can also set config manually by calling Init(string apiKey, string gameVersion, bool onDevelopmentMode, string domainKey)");
                return false;
            }
            if (string.IsNullOrEmpty(MyriaLeaderboardConfig.current.developerApiKey))
            {
                MyriaLeaderboardLogger.GetForLogLevel(MyriaLeaderboardLogger.LogLevel.Error)("API Key has not been set, set it in project settings or manually calling Init(string apiKey, string gameVersion, bool onDevelopmentMode, string domainKey)");
                return false;
            }

            // TODO Validate API Key
            Debug.Log("initialized" + initialized);
            initialized = true;
            return initialized;
        }

        public static bool CheckInitialized()
        {
            if (!initialized)
            {
                // MyriaLeaderboardConfig.current.token = "";
                // MyriaLeaderboardConfig.current.refreshToken = "";
                // MyriaLeaderboardConfig.current.deviceID = "";
                if (!Init())
                {
                    return false;
                }
            }

            return true;
        }

        [InitializeOnEnterPlayMode]
        static void OnEnterPlaymodeInEditor(EnterPlayModeOptions options)
        {
            initialized = false;
        }

        /// <summary>
        /// Get the entries for a specific leaderboard.
        /// </summary>
        /// <param name="page">How many entries to get</param>
        /// <param name="limit">How many after the last entry to receive</param>
        /// <param name="onComplete">onComplete Action for handling the response of type MyriaLeaderboardGetScoreListResponse</param>
        public static void GetListLeaderboard(int page, int limit, Action<GetListLeaderboardResponse> onComplete)
        {
            if (!CheckInitialized())
            {
                Debug.Log("LeaderboardSDK-is-not-initialized");
                onComplete?.Invoke(MyriaLeaderboardResponseFactory.SDKNotInitializedError<GetListLeaderboardResponse>());
                return;
            }
            BaseGetRequests request = new BaseGetRequests();
            request.limit = limit > 0 ? limit : 10;
            request.page = page > 0 ? page : 1;
            Action<GetListLeaderboardResponse> callback = (response) =>
            {
                GetListLeaderboardResponse parsedData = GetListLeaderboardResponse.Deserialize<GetListLeaderboardResponse>(response);
                onComplete?.Invoke(parsedData);
            };
            MyriaLeaderboardAPIManager.GetListLeaderboard(request, callback);
        }

        /// <summary>
        /// Get the entries for a specific leaderboard.
        /// </summary>
        /// <param name="leaderboardId">Key of the leaderboard to get entries for</param>
        /// <param name="page">How many entries to get</param>
        /// <param name="limit">How many after the last entry to receive</param>
        /// <param name="onComplete">onComplete Action for handling the response of type MyriaLeaderboardGetScoreListResponse</param>
        public static void GetScoreList(string leaderboardId, int page, int limit, Action<GetScoreListResponse> onComplete, ESortingField? sortingField, EOrderBy? orderBy)
        {
            if (!CheckInitialized())
            {
                Debug.Log("LeaderboardSDK-is-not-initialized");
                onComplete?.Invoke(MyriaLeaderboardResponseFactory.SDKNotInitializedError<GetScoreListResponse>());
                return;
            }
            if (string.IsNullOrEmpty(leaderboardId))
            {
                onComplete?.Invoke(MyriaLeaderboardResponseFactory.SDKNotInitializedError<GetScoreListResponse>());
                return;
            }
            GetScoreListRequestAPI request = new GetScoreListRequestAPI();
            request.leaderboardKey = leaderboardId;
            request.limit = limit > 0 ? limit : 10;
            request.page = page > 0 ? page : 1;
            request.sortingField = sortingField.HasValue ? ESortingFieldExtensions.SortingToStringValue(sortingField.Value) : "createdAt";
            request.orderBy = orderBy.HasValue ? EOrderByExtensions.OderByToStringValue(orderBy.Value) : "DESC";
            Action<GetScoreListResponse> callback = (response) =>
            {
                GetScoreListResponse parsedData = GetScoreListResponse.Deserialize<GetScoreListResponse>(response);
                onComplete?.Invoke(parsedData);
            };
            MyriaLeaderboardAPIManager.GetScoreList(request, callback);
        }

        public static void GetUserScore(string leaderboardId, string userId, Action<GetUserScoreResponse> onComplete, int? period)
        {
            if (!CheckInitialized())
            {
                Debug.Log("LeaderboardSDK-is-not-initialized");
                onComplete?.Invoke(MyriaLeaderboardResponseFactory.SDKNotInitializedError<GetUserScoreResponse>());
                return;
            }
            if (string.IsNullOrEmpty(leaderboardId)) {
                onComplete?.Invoke(MyriaLeaderboardResponseFactory.SDKNotInitializedError<GetUserScoreResponse>());
                return;
            }
            if (string.IsNullOrEmpty(userId)) {
                onComplete?.Invoke(MyriaLeaderboardResponseFactory.SDKNotInitializedError<GetUserScoreResponse>());
                return;
            }
            GetUserScoreRequestAPI request = new GetUserScoreRequestAPI();
            request.leaderboardKey = leaderboardId;
            request.userId = userId;
            Debug.Log("period" + period);
            if (period != null)
            {
                request.period = period;
            }
            Action<GetUserScoreResponse> callback = (response) =>
            {
                GetUserScoreResponse parsedData = GetUserScoreResponse.Deserialize<GetUserScoreResponse>(response);
                onComplete?.Invoke(parsedData);
            };
            MyriaLeaderboardAPIManager.GetUserScore(request, callback);
        }

        /// <summary>
        /// Get the entries for a specific leaderboard.
        /// </summary>
        /// <param name="leaderboardId">Key of the leaderboard to get entries for</param>
        /// <param name="PostScoreParams">How many entries to get</param>
        /// <param name="onComplete">onComplete Action for handling the response of type MyriaLeaderboardGetScoreListResponse</param>
        public static void PostScore(string leaderboardId, PostScoreParams data, Action<PostScoreResponse> onComplete)
        {
            if (!CheckInitialized())
            {
                onComplete?.Invoke(MyriaLeaderboardResponseFactory.SDKNotInitializedError<PostScoreResponse>());
                return;
            }
            PostScoreRequestAPI request = new PostScoreRequestAPI();
            request.leaderboardKey = leaderboardId;
            Action<PostScoreResponse> callback = (response) =>
            {
                onComplete?.Invoke(response);
            };
            MyriaLeaderboardAPIManager.PostScores(request, data, callback);
        }


    }

}
