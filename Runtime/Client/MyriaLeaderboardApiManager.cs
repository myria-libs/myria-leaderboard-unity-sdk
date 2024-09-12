using System;
using MyriaLeaderboard.Requests;
using MyriaLeaderboard.Response;
using System.Collections.Generic;
using UnityEngine;
using MyriaLeaderboard.MyriaLeaderboardExtension;


namespace MyriaLeaderboard
{
    public partial class MyriaLeaderboardAPIManager
    {
        public static void GetScoreList(GetScoreListRequestAPI getRequests, Action<GetScoreListResponse> onComplete)
        {
            EndPointClass requestEndPoint = MyriaLeaderboardEndPoints.MyriaEndPointGetScoreByLeaderboardId;

            // Handle all paging
            if (getRequests.page == 0)
            {
                onComplete?.Invoke(MyriaLeaderboardResponseFactory.InputUnserializableError<GetScoreListResponse>());
                return;
            }
            string tempEndpoint = requestEndPoint.endPoint;
            string endPoint = string.Format(requestEndPoint.endPoint, getRequests.leaderboardKey, getRequests.page, getRequests.limit, getRequests.sortingField, getRequests.orderBy);

            MyriaLeaderboardServerRequest.CallAPI(endPoint, requestEndPoint.httpMethod, null, (serverResponse) =>
            { MyriaLeaderboardResponse.Deserialize(onComplete, serverResponse); }, false, MyriaLeaderboardEnums.MyriaLeaderboardCallerRole.Base);
            //{
            //    MyriaLeaderboardGetScoreListResponse parseData = MyriaLeaderboardResponse.Deserialize<MyriaLeaderboardGetScoreListResponse>(serverResponse);
            //    onComplete?.Invoke(parseData);
            //}, false, MyriaLeaderboardEnums.MyriaLeaderboardCallerRole.Base);
        }

        public static void GetUserScore(GetUserScoreRequestAPI getRequests, Action<GetUserScoreResponse> onComplete)
        {
            EndPointClass requestEndPoint = MyriaLeaderboardEndPoints.MyriaEndPointGetUserScoreByLeaderboardIdAndUserId;

            // Handle all paging
            if (string.IsNullOrEmpty(getRequests.leaderboardKey) || string.IsNullOrEmpty(getRequests.userId))
            {
                onComplete?.Invoke(MyriaLeaderboardResponseFactory.InputUnserializableError<GetUserScoreResponse>());
                return;
            }
            string tempEndpoint = requestEndPoint.endPoint;
            string endPoint = string.Format(requestEndPoint.endPoint, getRequests.leaderboardKey, getRequests.userId);
            endPoint = endPoint + (getRequests.period != null ? ("/?period=" + getRequests.period) : "");

            MyriaLeaderboardServerRequest.CallAPI(endPoint, requestEndPoint.httpMethod, null, (serverResponse) =>
            { MyriaLeaderboardResponse.Deserialize(onComplete, serverResponse); }, false, MyriaLeaderboardEnums.MyriaLeaderboardCallerRole.Base);
            //{
            //    MyriaLeaderboardGetScoreListResponse parseData = MyriaLeaderboardResponse.Deserialize<MyriaLeaderboardGetScoreListResponse>(serverResponse);
            //    onComplete?.Invoke(parseData);
            //}, false, MyriaLeaderboardEnums.MyriaLeaderboardCallerRole.Base);
        }

        public static void GetListLeaderboard(BaseGetRequests getRequests, Action<GetListLeaderboardResponse> onComplete)
        {
            EndPointClass requestEndPoint = MyriaLeaderboardEndPoints.MyriaEndPointGetListLeaderboard;

            // Handle all paging
            if (getRequests.limit == 0 || getRequests.page == 0)
            {
                onComplete?.Invoke(MyriaLeaderboardResponseFactory.InputUnserializableError<GetListLeaderboardResponse>());
                return;
            }
            string tempEndpoint = requestEndPoint.endPoint;
            string endPoint = string.Format(requestEndPoint.endPoint, getRequests.page, getRequests.limit);

            MyriaLeaderboardServerRequest.CallAPI(endPoint, requestEndPoint.httpMethod, null, (serverResponse) =>
            { MyriaLeaderboardResponse.Deserialize(onComplete, serverResponse); }, false, MyriaLeaderboardEnums.MyriaLeaderboardCallerRole.Base);
            //{
            //    MyriaLeaderboardGetScoreListResponse parseData = MyriaLeaderboardResponse.Deserialize<MyriaLeaderboardGetScoreListResponse>(serverResponse);
            //    onComplete?.Invoke(parseData);
            //}, false, MyriaLeaderboardEnums.MyriaLeaderboardCallerRole.Base);
        }

        public static void PostScores(PostScoreRequestAPI getRequests, PostScoreParams data, Action<PostScoreResponse> onComplete)
        {
            if (data == null)
            {
                onComplete?.Invoke(MyriaLeaderboardResponseFactory.InputUnserializableError<PostScoreResponse>());
                return;
            }
            var json = MyriaLeaderboardJson.SerializeObject(data);
            EndPointClass requestEndPoint = MyriaLeaderboardEndPoints.MyriaEndPointPostScoreByLeaderboardId;
            string tempEndpoint = requestEndPoint.endPoint;
            string endPoint = string.Format(requestEndPoint.endPoint, getRequests.leaderboardKey);
            string xHmac = EncryptExtentsions.EncryptHMACSHA256(json, MyriaLeaderboardConfig.current.developerApiKey);
            string encryptApiKey = EncryptExtentsions.EncryptApiKey(MyriaLeaderboardConfig.current.developerApiKey, MyriaLeaderboardConfig.current.publicKey);
            Dictionary<string, string> xHmacHeader = new Dictionary<string, string>() { { "x-hmac", xHmac } };
            MyriaLeaderboardServerRequest.CallAPI(endPoint, requestEndPoint.httpMethod, json, (serverResponse) => { MyriaLeaderboardResponse.Deserialize(onComplete, serverResponse); }, false, MyriaLeaderboardEnums.MyriaLeaderboardCallerRole.Base, xHmacHeader);
        }

    }
}