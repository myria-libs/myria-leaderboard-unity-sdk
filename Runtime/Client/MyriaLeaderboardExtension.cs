using System;
using MyriaLeaderboard.Requests;
using System.Collections.Generic;
using UnityEngine;

namespace MyriaLeaderboard.MyriaLeaderboardEnums
{
    public enum EOrderBy { ASC, DESC };
    public static class EOrderByExtensions
    {
        public static string OderByToStringValue(this EOrderBy orderBy)
        {
            switch (orderBy)
            {
                case EOrderBy.ASC:
                    return "ASC";
                case EOrderBy.DESC:
                    return "DESC";
                default:
                    return string.Empty;
            }
        }
    }
    public enum ESortingField { createdAt, updatedAt, score, rank, period, leaderboardId, userId};
    public static class ESortingFieldExtensions
    {
        public static string SortingToStringValue(this ESortingField sortingField)
        {
            switch (sortingField)
            {
                case ESortingField.createdAt:
                    return "createdAt";
                case ESortingField.updatedAt:
                    return "updatedAt";
                case ESortingField.score:
                    return "score";
                case ESortingField.rank:
                    return "rank";
                case ESortingField.period:
                    return "period";
                case ESortingField.leaderboardId:
                    return "leaderboardId";
                case ESortingField.userId:
                    return "userId";
                default:
                    return "createdAt";
            }
        }
    }
}