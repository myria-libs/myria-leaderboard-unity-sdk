
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyriaLeaderboard
{
    public class MyriaLeaderboardEndPoints
    {
        [Header("GetListLeaderboard")]
        public static EndPointClass MyriaEndPointGetListLeaderboard = new EndPointClass("leaderboard?page={0}&limit={1}", MyriaLeaderboardHTTPMethod.GET);

        // Authentication
        [Header("GetScoreList")]
        public static EndPointClass MyriaEndPointGetScoreByLeaderboardId = new EndPointClass("leaderboard/{0}/scores?page={1}&limit={2}&sortingField={3}&orderBy={4}", MyriaLeaderboardHTTPMethod.GET);

        [Header("GetUserScore")]
        public static EndPointClass MyriaEndPointGetUserScoreByLeaderboardIdAndUserId = new EndPointClass("leaderboard/{0}/scores/by-user-id/{1}", MyriaLeaderboardHTTPMethod.GET);

        [Header("PostScore")]
        public static EndPointClass MyriaEndPointPostScoreByLeaderboardId = new EndPointClass("leaderboard/{0}/scores", MyriaLeaderboardHTTPMethod.POST);

    }
}
