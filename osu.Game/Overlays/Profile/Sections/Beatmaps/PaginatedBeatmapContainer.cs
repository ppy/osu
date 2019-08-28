// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Direct;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Profile.Sections.Beatmaps
{
    public class PaginatedBeatmapContainer : PaginatedContainer<APIBeatmapSet>
    {
        private const float panel_padding = 10f;
        private readonly BeatmapSetType type;

        public PaginatedBeatmapContainer(BeatmapSetType type, Bindable<User> user, string header, string missing = "None... yet.")
            : base(user, header, missing)
        {
            this.type = type;

            ItemsPerPage = 6;

            ItemsContainer.Spacing = new Vector2(panel_padding);
        }

        protected override APIRequest<List<APIBeatmapSet>> CreateRequest() =>
            new GetUserBeatmapsRequest(User.Value.Id, type, VisiblePages++, ItemsPerPage);

        protected override Drawable CreateDrawableItem(APIBeatmapSet model) => !model.OnlineBeatmapSetID.HasValue
            ? null
            : new DirectGridPanel(model.ToBeatmapSet(Rulesets))
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
            };
    }
}
