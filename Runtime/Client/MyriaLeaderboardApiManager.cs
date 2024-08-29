using System;
using MyriaLeaderboard.Requests;
using System.Collections.Generic;
using UnityEngine;


namespace MyriaLeaderboard
{
    public partial class MyriaLeaderboardAPIManager
    {
        public static void GetScoreList(GetScoreListRequestAPI getRequests, Action<GetScoreListResponse> onComplete)
        {
            EndPointClass requestEndPoint = MyriaLeaderboardEndPoints.MyriaEndPointGetScoreByLeaderboardId;

            string tempEndpoint = requestEndPoint.endPoint;
            string endPoint = string.Format(requestEndPoint.endPoint, getRequests.leaderboardKey, getRequests.page, getRequests.limit, getRequests.sortingField, getRequests.orderBy);

            // Handle all paging
            if (getRequests.page == 0)
            {
                onComplete?.Invoke(MyriaLeaderboardResponseFactory.InputUnserializableError<GetScoreListResponse>());
                return;
            }
            tempEndpoint = requestEndPoint.endPoint + "";
            endPoint = string.Format(tempEndpoint, getRequests.leaderboardKey, getRequests.page, getRequests.limit, getRequests.sortingField, getRequests.orderBy);

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
            Debug.Log("json" + json);

            EndPointClass requestEndPoint = MyriaLeaderboardEndPoints.MyriaEndPointPostScoreByLeaderboardId;
            string tempEndpoint = requestEndPoint.endPoint;
            string endPoint = string.Format(requestEndPoint.endPoint, getRequests.leaderboardKey);
            endPoint = string.Format(endPoint, getRequests.leaderboardKey);
            MyriaLeaderboardServerRequest.CallAPI(endPoint, requestEndPoint.httpMethod, json, (serverResponse) => { MyriaLeaderboardResponse.Deserialize(onComplete, serverResponse); }, false, MyriaLeaderboardEnums.MyriaLeaderboardCallerRole.Base);
        }

    }
}