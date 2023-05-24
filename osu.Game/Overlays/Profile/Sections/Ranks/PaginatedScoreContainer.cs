// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests;
using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using System.Collections.Generic;
using osu.Game.Online.API;
using osu.Framework.Allocation;
using osu.Framework.Localisation;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public partial class PaginatedScoreContainer : PaginatedProfileSubsection<SoloScoreInfo>
    {
        private readonly ScoreType type;

        public PaginatedScoreContainer(ScoreType type, Bindable<UserProfileData?> user, LocalisableString headerText)
            : base(user, headerText)
        {
            this.type = type;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ItemsContainer.Direction = FillDirection.Vertical;
        }

        protected override int GetCount(APIUser user)
        {
            switch (type)
            {
                case ScoreType.Best:
                    return user.ScoresBestCount;

                case ScoreType.Firsts:
                    return user.ScoresFirstCount;

                case ScoreType.Recent:
                    return user.ScoresRecentCount;

                case ScoreType.Pinned:
                    return user.ScoresPinnedCount;

                default:
                    return 0;
            }
        }

        protected override void OnItemsReceived(List<SoloScoreInfo> items)
        {
            if (CurrentPage == null || CurrentPage?.Offset == 0)
                drawableItemIndex = 0;

            base.OnItemsReceived(items);
        }

        protected override APIRequest<List<SoloScoreInfo>> CreateRequest(UserProfileData user, PaginationParameters pagination) =>
            new GetUserScoresRequest(user.User.Id, type, pagination, user.Ruleset);

        private int drawableItemIndex;

        protected override Drawable CreateDrawableItem(SoloScoreInfo model)
        {
            switch (type)
            {
                default:
                    return new DrawableProfileScore(model);

                case ScoreType.Best:
                    return new DrawableProfileWeightedScore(model, Math.Pow(0.95, drawableItemIndex++));
            }
        }
    }
}
