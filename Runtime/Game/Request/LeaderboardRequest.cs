using System;
using MyriaLeaderboard.Requests;
using System.Collections.Generic;
using UnityEngine;

namespace MyriaLeaderboard {
    public class GetScoreListRequestAPI : BaseGetRequests
    {
        public string leaderboardKey { get; set; }
        public static int? nextCursor { get; set; }
        public static int? prevCursor { get; set; }
    }
    public class PostScoreRequestAPI : BaseGetRequests
    {
        public string leaderboardKey { get; set; }
    }

    public class BaseGetRequests
    {
        public int limit { get; set; }
        public int page { get; set; }
        public string sortingField { get; set; }
        public string orderBy { get; set; }
    }
}

namespace MyriaLeaderboard.Requests
{

    public class Leaderboard
    {
        public int leaderboard_id { get; set; }
    }
    public class GetScoreListRequest : BaseGetRequests
    {
        public string leaderboardKey { get; set; }
        public static int? nextCursor { get; set; }
        public static int? prevCursor { get; set; }
    }
    public class PostScoreRequest : BaseGetRequests
    {
        public string leaderboardKey { get; set; }
    }

    public class BaseGetRequests
    {
        public int limit { get; set; }
        public int page { get; set; }
        public string sortingField { get; set; }
        public string orderBy { get; set; }
    }

    public class ItemPostScore
    {
        /// <summary>
        /// Amount of the given currency to debit/credit to/from the given wallet
        /// </summary>
        public int score { get; set; }
        /// <summary>
        /// The id of the currency that the amount is given in
        /// </summary>
        public string userId { get; set; }
        /// <summary> The id of the wallet to credit/debit to/from
        /// </summary>
        public string username { get; set; }
        /// <summary> The id of the wallet to credit/debit to/from
        /// </summary>
        public string displayName { get; set; }
    }

    public class PostScoreParams
    {
        public ItemPostScore[] items { get; set; }
    }
}