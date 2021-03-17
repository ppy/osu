// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapListing.Panels;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Profile.Sections.Beatmaps
{
    public class PaginatedBeatmapContainer : PaginatedProfileSubsection<APIBeatmapSet>
    {
        private const float panel_padding = 10f;
        private readonly BeatmapSetType type;

        public PaginatedBeatmapContainer(BeatmapSetType type, Bindable<User> user, string headerText)
            : base(user, headerText, "", CounterVisibilityState.AlwaysVisible)
        {
            this.type = type;
            ItemsPerPage = 6;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ItemsContainer.Spacing = new Vector2(panel_padding);
        }

        protected override int GetCount(User user)
        {
            switch (type)
            {
                case BeatmapSetType.Favourite:
                    return user.FavouriteBeatmapsetCount;

                case BeatmapSetType.Graveyard:
                    return user.GraveyardBeatmapsetCount;

                case BeatmapSetType.Loved:
                    return user.LovedBeatmapsetCount;

                case BeatmapSetType.RankedAndApproved:
                    return user.RankedAndApprovedBeatmapsetCount;

                case BeatmapSetType.Unranked:
                    return user.UnrankedBeatmapsetCount;

                default:
                    return 0;
            }
        }

        protected override APIRequest<List<APIBeatmapSet>> CreateRequest() =>
            new GetUserBeatmapsRequest(User.Value.Id, type, VisiblePages++, ItemsPerPage);

        protected override Drawable CreateDrawableItem(APIBeatmapSet model) => !model.OnlineBeatmapSetID.HasValue
            ? null
            : new GridBeatmapPanel(model.ToBeatmapSet(Rulesets))
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
            };
    }
}
