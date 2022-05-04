// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetUserRecentActivitiesRequest : PaginatedAPIRequest<List<APIRecentActivity>>
    {
        private readonly long userId;

        public GetUserRecentActivitiesRequest(long userId, PaginationParameters pagination)
            : base(pagination)
        {
            this.userId = userId;
        }

        protected override string Target => $"users/{userId}/recent_activity";
    }

    public enum RecentActivityType
    {
        Achievement,

        // ReSharper disable once IdentifierTypo
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
        Loved
    }
}
