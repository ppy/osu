﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetUserRecentActivitiesRequest : APIRequest<List<APIRecentActivity>>
    {
        private readonly long userId;
        private readonly int offset;

        public GetUserRecentActivitiesRequest(long userId, int offset = 0)
        {
            this.userId = userId;
            this.offset = offset;
        }

        protected override string Target => $"users/{userId}/recent_activity?offset={offset}";
    }

    public enum RecentActivityType
    {
        Achievement,
        BeatmapPlaycount,
        BeatmapsetApprove,
        BeatmapsetDelete,
        BeatmapsetRevive,
        BeatmapsetUpdate,
        BeatmapsetUpload,
        Medal,
        Rank,
        RankLost,
        UserSupportAgain,
        UserSupportFirst,
        UserSupportGift,
        UsernameChange,
    }

    public enum BeatmapApproval
    {
        Ranked,
        Approved,
        Qualified,
    }
}
