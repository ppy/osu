// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests;
using osu.Game.Users;
using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using System.Collections.Generic;
using osu.Game.Online.API;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public class PaginatedScoreContainer : PaginatedContainer<APILegacyScoreInfo>
    {
        private readonly ScoreType type;

        public PaginatedScoreContainer(ScoreType type, Bindable<User> user, string header, string missing)
            : base(user, header, missing)
        {
            this.type = type;

            ItemsPerPage = 5;

            ItemsContainer.Direction = FillDirection.Vertical;
        }

        protected override APIRequest<List<APILegacyScoreInfo>> CreateRequest() =>
            new GetUserScoresRequest(User.Value.Id, type, VisiblePages++, ItemsPerPage);

        protected override int GetCount(User user) => type switch
        {
            ScoreType.Firsts => user.ScoresFirstCount,
            _ => 0
        };

        protected override Drawable CreateDrawableItem(APILegacyScoreInfo model)
        {
            switch (type)
            {
                default:
                    return new DrawableProfileScore(model.CreateScoreInfo(Rulesets));

                case ScoreType.Best:
                    return new DrawableProfileWeightedScore(model.CreateScoreInfo(Rulesets), Math.Pow(0.95, ItemsContainer.Count));
            }
        }
    }
}
