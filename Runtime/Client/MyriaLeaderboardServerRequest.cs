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

    #region Rate Limiting Support

    public class RateLimiter
    {
        /* -- Configurable constants -- */
        // Tripwire settings, allow for a max total of n requests per x seconds
        protected const int TripWireTimeFrameSeconds = 60;
        protected const int MaxRequestsPerTripWireTimeFrame = 280;
        protected const int SecondsPerBucket = 5; // Needs to evenly divide the time frame

        // Moving average settings, allow for a max average of n requests per x seconds
        protected const float AllowXPercentOfTripWireMaxForMovingAverage = 0.8f; // Moving average threshold (the average number of requests per bucket) is set slightly lower to stop constant abusive call behaviour just under the tripwire limit
        protected const int CountMovingAverageAcrossNTripWireTimeFrames = 3; // Count Moving average across a longer time period

        /* -- Calculated constants -- */
        protected const int BucketsPerTimeFrame = TripWireTimeFrameSeconds / SecondsPerBucket;
        protected const int RateLimitMovingAverageBucketCount = CountMovingAverageAcrossNTripWireTimeFrames * BucketsPerTimeFrame;
        private const int MaxRequestsPerBucketOnMovingAverage = (int)((MaxRequestsPerTripWireTimeFrame * AllowXPercentOfTripWireMaxForMovingAverage) / (BucketsPerTimeFrame));


        /* -- Functionality -- */
        protected readonly int[] buckets = new int[RateLimitMovingAverageBucketCount];

        protected int lastBucket = -1;
        private DateTime _lastBucketChangeTime = DateTime.MinValue;
        private int _totalRequestsInBuckets;
        private int _totalRequestsInBucketsInTripWireTimeFrame;

        protected bool isRateLimited = false;
        private DateTime _rateLimitResolvesAt = DateTime.MinValue;

        protected virtual DateTime GetTimeNow()
        {
            return DateTime.Now;
        }

        public int GetSecondsLeftOfRateLimit()
        {
            if (!isRateLimited)
            {
                return 0;
            }
            return (int)Math.Ceiling((_rateLimitResolvesAt - GetTimeNow()).TotalSeconds);
        }
        private int MoveCurrentBucket(DateTime now)
        {
            int moveOverXBuckets = _lastBucketChangeTime == DateTime.MinValue ? 1 : (int)Math.Floor((now - _lastBucketChangeTime).TotalSeconds / SecondsPerBucket);
            if (moveOverXBuckets == 0)
            {
                return lastBucket;
            }

            for (int stepIndex = 1; stepIndex <= moveOverXBuckets; stepIndex++)
            {
                int bucketIndex = (lastBucket + stepIndex) % buckets.Length;
                if (bucketIndex == lastBucket)
                {
                    continue;
                }
                int bucketMovingOutOfTripWireTimeFrame = (bucketIndex - BucketsPerTimeFrame) < 0 ? buckets.Length + (bucketIndex - BucketsPerTimeFrame) : bucketIndex - BucketsPerTimeFrame;
                _totalRequestsInBucketsInTripWireTimeFrame -= buckets[bucketMovingOutOfTripWireTimeFrame]; // Remove the request count from the bucket that is moving out of the time frame from trip wire count
                _totalRequestsInBuckets -= buckets[bucketIndex]; // Remove the count from the bucket we're moving into from the total before emptying it
                buckets[bucketIndex] = 0;
            }

            return (lastBucket + moveOverXBuckets) % buckets.Length; // Step to next bucket and wrap around if necessary;
        }

        public virtual bool AddRequestAndCheckIfRateLimitHit()
        {
            DateTime now = GetTimeNow();
            var currentBucket = MoveCurrentBucket(now);

            if (isRateLimited)
            {
                if (_totalRequestsInBuckets <= 0)
                {
                    isRateLimited = false;
                    _rateLimitResolvesAt = DateTime.MinValue;
                }
            }
            else
            {
                buckets[currentBucket]++; // Increment the current bucket
                _totalRequestsInBuckets++; // Increment the total request count
                _totalRequestsInBucketsInTripWireTimeFrame++; // Increment the request count for the current time frame

                isRateLimited |= _totalRequestsInBucketsInTripWireTimeFrame >= MaxRequestsPerTripWireTimeFrame; // If the request count for the time frame is greater than the max requests per time frame, set isRateLimited to true
                isRateLimited |= _totalRequestsInBuckets / RateLimitMovingAverageBucketCount > MaxRequestsPerBucketOnMovingAverage; // If the average number of requests per bucket is greater than the max requests on moving average, set isRateLimited to true
#if UNITY_EDITOR
                if (_totalRequestsInBucketsInTripWireTimeFrame >= MaxRequestsPerTripWireTimeFrame) MyriaLeaderboardLogger.GetForLogLevel()("Rate Limit Hit due to Trip Wire, count = " + _totalRequestsInBucketsInTripWireTimeFrame + " out of allowed " + MaxRequestsPerTripWireTimeFrame);
                if (_totalRequestsInBuckets / RateLimitMovingAverageBucketCount > MaxRequestsPerBucketOnMovingAverage) MyriaLeaderboardLogger.GetForLogLevel()("Rate Limit Hit due to Moving Average, count = " + _totalRequestsInBuckets / RateLimitMovingAverageBucketCount + " out of allowed " + MaxRequestsPerBucketOnMovingAverage);
#endif
                if (isRateLimited)
                {
                    _rateLimitResolvesAt = (now - TimeSpan.FromSeconds(now.Second % SecondsPerBucket)) + TimeSpan.FromSeconds(buckets.Length * SecondsPerBucket);
                }
            }
            if (currentBucket != lastBucket)
            {
                _lastBucketChangeTime = now;
                lastBucket = currentBucket;
            }
            return isRateLimited;
        }

        protected int GetMaxRequestsInSingleBucket()
        {
            int maxRequests = 0;
            foreach (var t in buckets)
            {
                maxRequests = Math.Max(maxRequests, t);
            }

            return maxRequests;
        }

        private static RateLimiter _rateLimiter = null;

        public static RateLimiter Get()
        {
            if (_rateLimiter == null)
            {
                Reset();
            }
            return _rateLimiter;
        }

        public static void Reset()
        {
            _rateLimiter = new RateLimiter();
        }

//#if UNITY_EDITOR
//        [InitializeOnEnterPlayMode]
//        static void OnEnterPlaymodeInEditor(EnterPlayModeOptions options)
//        {
//            Debug.Log("OnEnterPLayMode");
//            MyriaLeaderboardLogger.GetForLogLevel(MyriaLeaderboardLogger.LogLevel.Verbose)("Reset RateLimiter due to entering play mode");
//            Reset();
//        }
//#endif
    }
    #endregion

    /// <summary>
    /// Construct a request to send to the server.
    /// </summary>
    [Serializable]
    public struct MyriaLeaderboardServerRequest
    {
        public string endpoint { get; set; }
        public MyriaLeaderboardHTTPMethod httpMethod { get; set; }
        public Dictionary<string, object> payload { get; set; }
        public string jsonPayload { get; set; }
        public byte[] upload { get; set; }
        public string uploadName { get; set; }
        public string uploadType { get; set; }
        public MyriaLeaderboardCallerRole callerRole { get; set; }
        public WWWForm form { get; set; }

        /// <summary>
        /// Leave this null if you don't need custom headers
        /// </summary>
        public Dictionary<string, string> extraHeaders;

        /// <summary>
        /// Query parameters to append to the end of the request URI
        /// Example: If you include a dictionary with a key of "page" and a value of "42" (as a string) then the url would become "https://mydomain.com/endpoint?page=42"
        /// </summary>
        public Dictionary<string, string> queryParams;

        public int retryCount { get; set; }

        #region ServerRequest constructor

        public MyriaLeaderboardServerRequest(string endpoint, MyriaLeaderboardHTTPMethod httpMethod = MyriaLeaderboardHTTPMethod.GET, byte[] upload = null, string uploadName = null, string uploadType = null, Dictionary<string, string> body = null,
            Dictionary<string, string> extraHeaders = null, bool useAuthToken = true, MyriaLeaderboard.MyriaLeaderboardEnums.MyriaLeaderboardCallerRole callerRole = MyriaLeaderboard.MyriaLeaderboardEnums.MyriaLeaderboardCallerRole.User, bool isFileUpload = true)
        {
            this.retryCount = 0;
            this.endpoint = endpoint;
            this.httpMethod = httpMethod;
            this.payload = null;
            this.upload = upload;
            this.uploadName = uploadName;
            this.uploadType = uploadType;
            this.jsonPayload = null;
            this.extraHeaders = extraHeaders != null && extraHeaders.Count == 0 ? null : extraHeaders; // Force extra headers to null if empty dictionary was supplied
            this.queryParams = null;
            this.callerRole = callerRole;
            this.form = new WWWForm();

            foreach (var kvp in body)
            {
                this.form.AddField(kvp.Key, kvp.Value);
            }

            this.form.AddBinaryData("file", upload, uploadName);

            bool isNonPayloadMethod = (this.httpMethod == MyriaLeaderboardHTTPMethod.GET || this.httpMethod == MyriaLeaderboardHTTPMethod.HEAD || this.httpMethod == MyriaLeaderboardHTTPMethod.OPTIONS);

            if (this.payload != null && isNonPayloadMethod)
            {
                MyriaLeaderboardLogger.GetForLogLevel(MyriaLeaderboardLogger.LogLevel.Warning)("Payloads should not be sent in GET, HEAD, OPTIONS, requests. Attempted to send a payload to: " + this.httpMethod.ToString() + " " + this.endpoint);
            }
        }

        public MyriaLeaderboardServerRequest(string endpoint, MyriaLeaderboardHTTPMethod httpMethod = MyriaLeaderboardHTTPMethod.GET, Dictionary<string, object> payload = null, Dictionary<string, string> extraHeaders = null, Dictionary<string, string> queryParams = null,
            bool useAuthToken = true, MyriaLeaderboard.MyriaLeaderboardEnums.MyriaLeaderboardCallerRole callerRole = MyriaLeaderboard.MyriaLeaderboardEnums.MyriaLeaderboardCallerRole.User)
        {
            this.retryCount = 0;
            this.endpoint = endpoint;
            this.httpMethod = httpMethod;
            this.payload = payload != null && payload.Count == 0 ? null : payload; //Force payload to null if an empty dictionary was supplied
            this.upload = null;
            this.uploadName = null;
            this.uploadType = null;
            this.jsonPayload = null;
            this.extraHeaders = extraHeaders != null && extraHeaders.Count == 0 ? null : extraHeaders; // Force extra headers to null if empty dictionary was supplied
            this.queryParams = queryParams != null && queryParams.Count == 0 ? null : queryParams;
            this.callerRole = callerRole;
            bool isNonPayloadMethod = (this.httpMethod == MyriaLeaderboardHTTPMethod.GET || this.httpMethod == MyriaLeaderboardHTTPMethod.HEAD || this.httpMethod == MyriaLeaderboardHTTPMethod.OPTIONS);
            this.form = null;
            if (this.payload != null && isNonPayloadMethod)
            {
                MyriaLeaderboardLogger.GetForLogLevel(MyriaLeaderboardLogger.LogLevel.Warning)("Payloads should not be sent in GET, HEAD, OPTIONS, requests. Attempted to send a payload to: " + this.httpMethod.ToString() + " " + this.endpoint);
            }
        }

        public MyriaLeaderboardServerRequest(string endpoint, MyriaLeaderboardHTTPMethod httpMethod = MyriaLeaderboardHTTPMethod.GET, string payload = null, Dictionary<string, string> extraHeaders = null, Dictionary<string, string> queryParams = null, bool useAuthToken = true,
            MyriaLeaderboard.MyriaLeaderboardEnums.MyriaLeaderboardCallerRole callerRole = MyriaLeaderboard.MyriaLeaderboardEnums.MyriaLeaderboardCallerRole.User)
        {
            this.retryCount = 0;
            this.endpoint = endpoint;
            this.httpMethod = httpMethod;
            this.jsonPayload = payload;
            this.upload = null;
            this.uploadName = null;
            this.uploadType = null;
            this.payload = null;
            this.extraHeaders = extraHeaders != null && extraHeaders.Count == 0 ? null : extraHeaders; // Force extra headers to null if empty dictionary was supplied
            this.queryParams = queryParams != null && queryParams.Count == 0 ? null : queryParams;
            this.callerRole = callerRole;
            bool isNonPayloadMethod = (this.httpMethod == MyriaLeaderboardHTTPMethod.GET || this.httpMethod == MyriaLeaderboardHTTPMethod.HEAD || this.httpMethod == MyriaLeaderboardHTTPMethod.OPTIONS);
            this.form = null;
            if (!string.IsNullOrEmpty(jsonPayload) && isNonPayloadMethod)
            {
                MyriaLeaderboardLogger.GetForLogLevel(MyriaLeaderboardLogger.LogLevel.Warning)("Payloads should not be sent in GET, HEAD, OPTIONS, requests. Attempted to send a payload to: " + this.httpMethod.ToString() + " " + this.endpoint);
            }
        }

        #endregion


        #region Make ServerRequest and call send (3 functions)
        public static void CallAPI(string endPoint, MyriaLeaderboardHTTPMethod httpMethod, string body = null, Action<MyriaLeaderboardResponse> onComplete = null, bool useAuthToken = true, MyriaLeaderboard.MyriaLeaderboardEnums.MyriaLeaderboardCallerRole callerRole = MyriaLeaderboard.MyriaLeaderboardEnums.MyriaLeaderboardCallerRole.User, Dictionary<string, string> additionalHeaders = null)
        {
            Debug.Log("json" + body );
            if (RateLimiter.Get().AddRequestAndCheckIfRateLimitHit())
            {
                onComplete?.Invoke(MyriaLeaderboardResponseFactory.RateLimitExceeded<MyriaLeaderboardResponse>(endPoint, RateLimiter.Get().GetSecondsLeftOfRateLimit()));
                return;
            }

#if UNITY_EDITOR
            MyriaLeaderboardLogger.GetForLogLevel(MyriaLeaderboardLogger.LogLevel.Verbose)("Caller Type: " + callerRole);
#endif

            Dictionary<string, string> headers = new Dictionary<string, string>();


            if (MyriaLeaderboardConfig.current != null)
                headers.Add("x-api-developer-key", MyriaLeaderboardConfig.current.xDeveloperApiKey);
                headers.Add(MyriaLeaderboardConfig.current.dateVersion.key, MyriaLeaderboardConfig.current.dateVersion.value);

            if (additionalHeaders != null)
            {
                foreach (var additionalHeader in additionalHeaders)
                {
                    headers.Add(additionalHeader.Key, additionalHeader.Value);
                }
            }

            new MyriaLeaderboardServerRequest(endPoint, httpMethod, body, headers, callerRole: callerRole).Send((response) => { onComplete?.Invoke(response); });
        }

        public static void CallDomainAuthAPI(string endPoint, MyriaLeaderboardHTTPMethod httpMethod, string body = null, Action<MyriaLeaderboardResponse> onComplete = null)
        {
            if (RateLimiter.Get().AddRequestAndCheckIfRateLimitHit())
            {
                onComplete?.Invoke(MyriaLeaderboardResponseFactory.RateLimitExceeded<MyriaLeaderboardResponse>(endPoint, RateLimiter.Get().GetSecondsLeftOfRateLimit()));
                return;
            }

            if (MyriaLeaderboardConfig.current.domainKey.ToString().Length == 0)
            {
#if UNITY_EDITOR
                MyriaLeaderboardLogger.GetForLogLevel(MyriaLeaderboardLogger.LogLevel.Error)("MyriaLeaderboard domain key must be set in settings");
#endif
                onComplete?.Invoke(MyriaLeaderboardResponseFactory.ClientError<MyriaLeaderboardResponse>("MyriaLeaderboard domain key must be set in settings"));

                return;
            }

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("domain-key", MyriaLeaderboardConfig.current.domainKey);

            new MyriaLeaderboardServerRequest(endPoint, httpMethod, body, headers, callerRole: MyriaLeaderboardCallerRole.Base).Send((response) => { onComplete?.Invoke(response); });
        }

        public static void UploadFile(string endPoint, MyriaLeaderboardHTTPMethod httpMethod, byte[] file, string fileName = "file", string fileContentType = "text/plain", Dictionary<string, string> body = null, Action<MyriaLeaderboardResponse> onComplete = null, bool useAuthToken = true, MyriaLeaderboard.MyriaLeaderboardEnums.MyriaLeaderboardCallerRole callerRole = MyriaLeaderboard.MyriaLeaderboardEnums.MyriaLeaderboardCallerRole.User)
        {
            if (RateLimiter.Get().AddRequestAndCheckIfRateLimitHit())
            {
                onComplete?.Invoke(MyriaLeaderboardResponseFactory.RateLimitExceeded<MyriaLeaderboardResponse>(endPoint, RateLimiter.Get().GetSecondsLeftOfRateLimit()));
                return;
            }
            Dictionary<string, string> headers = new Dictionary<string, string>();
            if (file.Length == 0)
            {
#if UNITY_EDITOR
                    MyriaLeaderboardLogger.GetForLogLevel(MyriaLeaderboardLogger.LogLevel.Error)("File content is empty, not allowed.");
#endif
                onComplete?.Invoke(MyriaLeaderboardResponseFactory.ClientError<MyriaLeaderboardResponse>("File content is empty, not allowed."));
                return;
            }
            

            new MyriaLeaderboardServerRequest(endPoint, httpMethod, file, fileName, fileContentType, body, headers, callerRole: callerRole).Send((response) => { onComplete?.Invoke(response); });
        }

        public static void UploadFile(EndPointClass endPoint, byte[] file, string fileName = "file", string fileContentType = "text/plain", Dictionary<string, string> body = null, Action<MyriaLeaderboardResponse> onComplete = null,
            bool useAuthToken = true, MyriaLeaderboard.MyriaLeaderboardEnums.MyriaLeaderboardCallerRole callerRole = MyriaLeaderboard.MyriaLeaderboardEnums.MyriaLeaderboardCallerRole.User)
        {
            UploadFile(endPoint.endPoint, endPoint.httpMethod, file, fileName, fileContentType, body, onComplete: (serverResponse) => { MyriaLeaderboardResponse.Deserialize(onComplete, serverResponse); }, useAuthToken, callerRole);
        }

        #endregion

        /// <summary>
        /// just debug and call ServerAPI.SendRequest which takes the current ServerRequest and pass this response
        /// </summary>
        public void Send(System.Action<MyriaLeaderboardResponse> OnServerResponse)
        {
            MyriaLeaderboardServerApi.SendRequest(this, (response) => { OnServerResponse?.Invoke(response); });
        }
    }
}