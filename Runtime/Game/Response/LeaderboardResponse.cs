using System;
using MyriaLeaderboard.Requests;
using System.Collections.Generic;
using UnityEngine;

namespace MyriaLeaderboard.Response
{
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