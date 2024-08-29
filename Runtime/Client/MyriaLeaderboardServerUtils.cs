using System.Collections.Generic;
using UnityEngine;
using System;
using MyriaLeaderboard.MyriaLeaderboardEnums;
#if MYRIA_LEADERBOARD_USE_NEWTONSOFTJSON
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
#else
using LLlibs.ZeroDepJson;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

//this is common between user and admin
namespace MyriaLeaderboard
{

    public static class MyriaLeaderboardJsonSettings
    {
#if MYRIA_LEADERBOARD_USE_NEWTONSOFTJSON
        public static readonly JsonSerializerSettings Default = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() },
            Formatting = Formatting.None
        };
#else
        public static readonly JsonOptions Default = new JsonOptions(JsonSerializationOptions.Default & ~JsonSerializationOptions.SkipGetOnly & ~JsonSerializationOptions.ConvertToSnakeCase);
#endif
    }

    public static class MyriaLeaderboardJson
    {
#if MYRIA_LEADERBOARD_USE_NEWTONSOFTJSON
        public static string SerializeObject(object obj)
        {
            return SerializeObject(obj, MyriaLeaderboardJsonSettings.Default);
        }

        public static string SerializeObject(object obj, JsonSerializerSettings settings)
        {
            return JsonConvert.SerializeObject(obj, settings ?? MyriaLeaderboardJsonSettings.Default);
        }

        public static T DeserializeObject<T>(string json)
        {
            return DeserializeObject<T>(json, MyriaLeaderboardJsonSettings.Default);
        }


        public static T DeserializeObject<T>(string json, JsonSerializerSettings settings)
        {
            return JsonConvert.DeserializeObject<T>(json, settings ?? MyriaLeaderboardJsonSettings.Default);
        }
#else //MYRIA_LEADERBOARD_USE_NEWTONSOFTJSON
        public static string SerializeObject(object obj)
        {
            return SerializeObject(obj, MyriaLeaderboardJsonSettings.Default);
        }

        public static string SerializeObject(object obj, JsonOptions options)
        {
            return Json.Serialize(obj, options ?? MyriaLeaderboardJsonSettings.Default);
        }

        public static T DeserializeObject<T>(string json)
        {
            return DeserializeObject<T>(json, MyriaLeaderboardJsonSettings.Default);
        }

        public static T DeserializeObject<T>(string json, JsonOptions options)
        {
            return Json.Deserialize<T>(json, options ?? MyriaLeaderboardJsonSettings.Default);
        }
#endif //MYRIA_LEADERBOARD_USE_NEWTONSOFTJSON
    }

    [Serializable]
    public enum MyriaLeaderboardHTTPMethod
    {
        GET = 0,
        POST = 1,
        DELETE = 2,
        PUT = 3,
        HEAD = 4,
        CREATE = 5,
        OPTIONS = 6,
        PATCH = 7,
        UPLOAD_FILE = 8,
        UPDATE_FILE = 9
    }

    public class MyriaLeaderboardErrorData
    {
        public MyriaLeaderboardErrorData(int httpStatusCode, string errorMessage)
        {
            code = $"HTTP{httpStatusCode}";
            doc_url = $"https://developer.mozilla.org/docs/Web/HTTP/Status/{httpStatusCode}";
            message = errorMessage;
        }

        public MyriaLeaderboardErrorData() { }

        /// <summary>
        /// A descriptive code identifying the error.
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// A link to further documentation on the error.
        /// </summary>
        public string doc_url { get; set; }

        /// <summary>
        /// A unique identifier of the request to use in contact with support.
        /// </summary>
        public string request_id { get; set; }

        /// <summary>
        /// A unique identifier for tracing the request through MyriaLeaderboard systems, use this in contact with support.
        /// </summary>
        public string trace_id { get; set; }

        /// <summary>
        /// If the request was not a success this property will hold any error messages
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// An easy way of debugging MyriaLeaderboardErrorData class, example: Debug.Log(onComplete.errorData);
        /// </summary>
        /// <returns>string used to debug errors</returns>
        public override string ToString()
        {
            // Empty error, make sure we print something
            if (string.IsNullOrEmpty(message) && string.IsNullOrEmpty(trace_id) && string.IsNullOrEmpty(request_id))
            {
                return $"An unexpected MyriaLeaderboard error without error data occurred. Please try again later.\n If the issue persists, please contact MyriaLeaderboard support.";
            }

            //Prinost important info first
            string prettyError = $"MyriaLeaderboard Error: \"{message}\"";

            // Look for intermittent, non user errors
            if (!string.IsNullOrEmpty(code) && code.StartsWith("HTTP5"))
            {
                prettyError +=
                    $"\nTry again later. If the issue persists, please contact MyriaLeaderboard support and provide the following error details:\n trace ID - \"{trace_id}\",\n request ID - \"{request_id}\",\n message - \"{message}\".";
                if (!string.IsNullOrEmpty(doc_url))
                {
                    prettyError += $"\nFor more information, see {doc_url} (error code was \"{code}\").";
                }
            }
            // Print user errors
            else
            {
                prettyError +=
                    $"\nThere was a problem with your request. The error message provides information on the problem and will help you fix it.";
                if (!string.IsNullOrEmpty(doc_url))
                {
                    prettyError += $"\nFor more information, see {doc_url} (error code was \"{code}\").";
                }

                prettyError +=
                    $"\nIf you are unable to fix the issue, contact MyriaLeaderboard support and provide the following error details:";
                if (!string.IsNullOrEmpty(trace_id))
                {
                    prettyError += $"\n     trace ID - \"{trace_id}\"";
                }
                if (!string.IsNullOrEmpty(request_id))
                {
                    prettyError += $"\n     request ID - \"{request_id}\"";
                }

                prettyError += $"\n     message - \"{message}\".";
            }
            return prettyError;
        }
    }


}