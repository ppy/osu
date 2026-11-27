// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Overlays.Rankings.Tables
{
    public partial class MatchmakingTable : RankingsTable<APIUserMatchmakingStatistics>
    {
        public MatchmakingTable(int page, IReadOnlyList<APIUserMatchmakingStatistics> rankings)
            : base(page, rankings)
        {
        }

        protected override RankingsTableColumn[] CreateAdditionalHeaders() => new[]
        {
            new RankingsTableColumn(RankingsStrings.MatchmakingWins, Anchor.CentreLeft, new Dimension(GridSizeMode.AutoSize)),
            new RankingsTableColumn(RankingsStrings.MatchmakingPlays, Anchor.CentreLeft, new Dimension(GridSizeMode.AutoSize)),
            new RankingsTableColumn(RankingsStrings.MatchmakingRating, Anchor.CentreLeft, new Dimension(GridSizeMode.AutoSize), true),
        };

        protected override Drawable[] CreateAdditionalContent(APIUserMatchmakingStatistics item) => new Drawable[]
        {
            new ColouredRowText
            {
                Text = item.FirstPlacements.ToLocalisableString(@"N0"),
            },
            new ColouredRowText
            {
                Text = item.Plays.ToLocalisableString(@"N0"),
            },
            new RowText
            {
                Text = item.Rating.ToLocalisableString(@"N0"),
            },
        };

        protected override CountryCode GetCountryCode(APIUserMatchmakingStatistics item) => item.User.CountryCode;

        protected override Drawable[] CreateFlagContent(APIUserMatchmakingStatistics item)
        {
            var username = new LinkFlowContainer(t => t.Font = OsuFont.GetFont(size: TEXT_SIZE, italics: true))
            {
                AutoSizeAxes = Axes.X,
                RelativeSizeAxes = Axes.Y,
                TextAnchor = Anchor.CentreLeft
            };
            username.AddUserLink(item.User);
            return [new UpdateableTeamFlag(item.User.Team) { Size = new Vector2(40, 20) }, username];
        }
    }
}
