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
    public class MyriaLeaderboardResponse
    {
        /// <summary>
        /// HTTP Status Code
        /// </summary>
        public int statusCode { get; set; }

        /// <summary>
        /// Whether this request was a success
        /// </summary>
        public bool success { get; set; }

        /// <summary>
        /// Raw text/http body from the server response
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// If this request was not a success, this structure holds all the information needed to identify the problem
        /// </summary>
        public MyriaLeaderboardErrorData errorData { get; set; }

        /// <summary>
        /// inheritdoc added this because unity main thread executing style cut the calling stack and make the event orphan see also calling multiple events 
        /// of the same type makes use unable to identify each one
        /// </summary>
        public string EventId { get; set; }

        public static void Deserialize<T>(Action<T> onComplete, MyriaLeaderboardResponse serverResponse,
#if MYRIA_LEADERBOARD_USE_NEWTONSOFTJSON
            JsonSerializerSettings options = null
#else //MYRIA_LEADERBOARD_USE_NEWTONSOFTJSON
            JsonOptions options = null
#endif
            )
            where T : MyriaLeaderboardResponse, new()
        {
            onComplete?.Invoke(Deserialize<T>(serverResponse, options));
        }

        public static T Deserialize<T>(MyriaLeaderboardResponse serverResponse,
#if MYRIA_LEADERBOARD_USE_NEWTONSOFTJSON
            JsonSerializerSettings options = null
#else //MYRIA_LEADERBOARD_USE_NEWTONSOFTJSON
            JsonOptions options = null
#endif
            )
            where T : MyriaLeaderboardResponse, new()
        {
            if (serverResponse == null)
            {
                return MyriaLeaderboardResponseFactory.ClientError<T>("Unknown error, please check your internet connection.");
            }
            else if (serverResponse.errorData != null)
            {
                return new T() { success = false, errorData = serverResponse.errorData, statusCode = serverResponse.statusCode, text = serverResponse.text };
            }

            var response = MyriaLeaderboardJson.DeserializeObject<T>(serverResponse.text, options ?? MyriaLeaderboardJsonSettings.Default) ?? new T();

            response.text = serverResponse.text;
            response.success = serverResponse.success;
            response.errorData = serverResponse.errorData;
            response.statusCode = serverResponse.statusCode;

            return response;
        }
    }

    public class User
    {
        public string userId { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
        public string username { get; set; }
        public string displayName { get; set; }
    }

    public class GetListScoreItemResponse
    {
        public int id { get; set; }
        public string userId { get; set; }
        public int leaderboardId { get; set; }
        public int period { get; set; }
        public int score { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
        public int rank { get; set; }
    }

    public class GetScoreDataResponse
    {
        public PaginationMetaResponse meta { get; set; }
        public GetListScoreItemResponse[] items { get; set; }
    }
    public class GetScoreListResponse : MyriaLeaderboardResponse
    {
        public string status { get; set; }
        public GetScoreDataResponse data { get; set; }
    }

    // Postscore
    public class PostScoreDataResponse
    {
        // we are doing thisfor legacy reasons, since it is no longer being set on the backend
        public int id { get; set; }
        public string userId { get; set; }
        public int leaderboardId { get; set; }
        public int score { get; set; }
        public int period { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
    }

    public class PostScoreResponseData
    {
        public PostScoreDataResponse[] success { get; set; }
        public PostScoreDataResponse[] failed { get; set; }
    }

    public class PostScoreResponse : MyriaLeaderboardResponse
    {
        public string status { get; set; }
        public PostScoreResponseData data { get; set; }
    }


    public class PaginationResponse<TKey>
    {
        /// <summary>
        /// The total available items in this list
        /// </summary>
        public int total { get; set; }
        /// <summary>
        /// The cursor that points to the next item in the list. Use this in subsequent requests to get additional items from the list.
        /// </summary>
        public TKey next_cursor { get; set; }
        /// <summary>
        /// The cursor that points to the first item in this batch of items.
        /// </summary>
        public TKey previous_cursor { get; set; }
    }

    public class PaginationMetaResponse
    {
        /// <summary>
        /// HTTP Status Code
        /// </summary>
        public int totalItems { get; set; }

        /// <summary>
        /// Whether this request was a success
        /// </summary>
        public int itemCount { get; set; }

        /// <summary>
        /// Raw text/http body from the server response
        /// </summary>
        public int itemsPerPage { get; set; }

        /// <summary>
        /// Raw text/http body from the server response
        /// </summary>
        public int totalPages { get; set; }

        /// <summary>
        /// Raw text/http body from the server response
        /// </summary>
        public int currentPage { get; set; }
    }

    /// <summary>
    /// Convenience factory class for creating some responses that we use often.
    /// </summary>
    public class MyriaLeaderboardResponseFactory
    {
        /// <summary>
        /// Construct an error response from a network request to send to the client.
        /// </summary>
        public static T NetworkError<T>(string errorMessage, int httpStatusCode) where T : MyriaLeaderboardResponse, new()
        {
            return new T()
            {
                success = false,
                text = "{ \"message\": \"" + errorMessage + "\"}",
                statusCode = httpStatusCode,
                errorData = new MyriaLeaderboardErrorData(httpStatusCode, errorMessage)
            };
        }

        /// <summary>
        /// Construct an error response from a client side error to send to the client.
        /// </summary>
        public static T ClientError<T>(string errorMessage) where T : MyriaLeaderboardResponse, new()
        {
            return new T()
            {
                success = false,
                text = "{ \"message\": \"" + errorMessage + "\"}",
                statusCode = 0,
                errorData = new MyriaLeaderboardErrorData
                {
                    message = errorMessage,
                }
            };
        }

        /// <summary>
        /// Construct an error response for token expiration.
        /// </summary>
        public static T TokenExpiredError<T>() where T : MyriaLeaderboardResponse, new()
        {
            return NetworkError<T>("Token Expired", 401);
        }

        /// <summary>
        /// Construct an error response specifically when the SDK has not been initialized.
        /// </summary>
        public static T SDKNotInitializedError<T>() where T : MyriaLeaderboardResponse, new()
        {
            return ClientError<T>("The MyriaLeaderboard SDK has not been initialized, please start a session to call this method");
        }

        /// <summary>
        /// Construct an error response because an unserializable input has been given
        /// </summary>
        public static T InputUnserializableError<T>() where T : MyriaLeaderboardResponse, new()
        {
            return ClientError<T>("Method parameter could not be serialized");
        }

        /// <summary>
        /// Construct an error response because the rate limit has been hit
        /// </summary>
        public static T RateLimitExceeded<T>(string method, int secondsLeftOfRateLimit) where T : MyriaLeaderboardResponse, new()
        {
            return ClientError<T>($"Your request to {method} was not sent. You are sending too many requests and are being rate limited for {secondsLeftOfRateLimit} seconds");
        }
    }




}