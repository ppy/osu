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
using osu.Framework.Allocation;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public class PaginatedScoreContainer : PaginatedProfileSubsection<APILegacyScoreInfo>
    {
        private readonly ScoreType type;

        public PaginatedScoreContainer(ScoreType type, Bindable<User> user, string headerText, CounterVisibilityState counterVisibilityState, string missingText = "")
            : base(user, headerText, missingText, counterVisibilityState)
        {
            this.type = type;

            ItemsPerPage = 5;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ItemsContainer.Direction = FillDirection.Vertical;
        }

        protected override int GetCount(User user)
        {
            switch (type)
            {
                case ScoreType.Firsts:
                    return user.ScoresFirstCount;

                default:
                    return 0;
            }
        }

        protected override void OnItemsReceived(List<APILegacyScoreInfo> items)
        {
            if (VisiblePages == 0)
                drawableItemIndex = 0;

            base.OnItemsReceived(items);

            if (type == ScoreType.Recent)
                SetCount(items.Count);
        }

        protected override APIRequest<List<APILegacyScoreInfo>> CreateRequest() =>
            new GetUserScoresRequest(User.Value.Id, type, VisiblePages++, ItemsPerPage);

        private int drawableItemIndex;

        protected override Drawable CreateDrawableItem(APILegacyScoreInfo model)
        {
            switch (type)
            {
                default:
                    return new DrawableProfileScore(model.CreateScoreInfo(Rulesets));

                case ScoreType.Best:
                    return new DrawableProfileWeightedScore(model.CreateScoreInfo(Rulesets), Math.Pow(0.95, drawableItemIndex++));
            }
        }
    }
}
