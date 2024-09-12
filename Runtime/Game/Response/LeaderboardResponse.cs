using System;
using MyriaLeaderboard.Requests;
using System.Collections.Generic;
using UnityEngine;

namespace MyriaLeaderboard.Response
{

    public class LeaderboardInfo
    {
        public int id { get; set; }
        public string createdAt { get; set; }
        public string expireAt { get; set; }
        public string availableAt { get; set; }
        public string updatedAt { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string updateScoreStrategy { get; set; }
        public string gameName { get; set; }
        public string status { get; set; }
        public int livePeriodInDays { get; set; }
        public int currentPeriod { get; set; }
        public int dataPersistenceInDays { get; set; }
        public bool enableMetadata { get; set; }
        public bool enablePeriodRefresh { get; set; }
        public string developerId { get; set; }
        public string daysToNextPeriod { get; set; }
    }

    public class User
    {
        public string userId { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
        public string username { get; set; }
        public string displayName { get; set; }
    }
    public class GetScoreItemResponse
    {
        public int id { get; set; }
        public string userId { get; set; }
        public int leaderboardId { get; set; }
        public int score { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
        public int rank { get; set; }
    }
    public class GetScoreSingleItemResponse
    {
        public int id { get; set; }
        public string userId { get; set; }
        public int leaderboardId { get; set; }
        public int score { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
        public int rank { get; set; }
        public LeaderboardInfo leaderboard { get; set; }
        public User user { get; set; }
    }
    public class GetScoreListDataResponse
    {
        public PaginationMetaResponse meta { get; set; }
        public GetScoreItemResponse[] items { get; set; }
    }
    public class GetScoreListResponse : MyriaLeaderboardResponse
    {
        public string status { get; set; }
        public GetScoreListDataResponse data { get; set; }
    }

    public class GetUserScoreDataResponse
    {
        public PaginationMetaResponse meta { get; set; }
        public GetListScoreItemResponse[] items { get; set; }
    }

    public class GetUserScoreResponse : MyriaLeaderboardResponse
    {
        public string status { get; set; }
        public GetScoreSingleItemResponse data { get; set; }
    }

    public class LeaderboardResponseItem
    {
        public LeaderboardInfo[] items { get; set; }
        public PaginationMetaResponse meta { get; set; }
    }

    public class GetListLeaderboardResponse : MyriaLeaderboardResponse
    {
        public string status { get; set; }
        public LeaderboardResponseItem data { get; set; }
    }

    // Postscore
    public class PostScoreDataResponse
    {
        public int id { get; set; }
        public string userId { get; set; }
        public int leaderboardId { get; set; }
        public int score { get; set; }
        public int period { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
        public User user { get; set; }
    }

    public class PostScoreResponse : MyriaLeaderboardResponse
    {
        public string status { get; set; }
        public PostScoreDataResponse[] data { get; set; }
    }

}