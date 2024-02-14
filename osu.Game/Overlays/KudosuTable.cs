// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Rankings.Tables;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Users;

namespace osu.Game.Overlays
{
    public partial class KudosuTable : RankingsTable<APIUser>
    {
        public KudosuTable(int page, List<APIUser> users)
            : base(page, users)
        {
        }

        protected override Drawable CreateRowBackground(APIUser item)
        {
            var background = base.CreateRowBackground(item);

            // see: https://github.com/ppy/osu-web/blob/9de00a0b874c56893d98261d558d78d76259d81b/resources/views/multiplayer/rooms/_rankings_table.blade.php#L23
            if (!item.Active)
                background.Alpha = 0.5f;

            return background;
        }

        protected override Drawable[] CreateRowContent(int index, APIUser item)
        {
            var content = base.CreateRowContent(index, item);

            // see: https://github.com/ppy/osu-web/blob/9de00a0b874c56893d98261d558d78d76259d81b/resources/views/multiplayer/rooms/_rankings_table.blade.php#L23
            if (!item.Active)
            {
                foreach (var d in content)
                    d.Alpha = 0.5f;
            }

            return content;
        }

        protected override RankingsTableColumn[] CreateAdditionalHeaders()
        {
            const int min_width = 120;
            return new[]
            {
                new RankingsTableColumn(RankingsStrings.KudosuTotal, Anchor.Centre, new Dimension(GridSizeMode.AutoSize, minSize: min_width), true),
                new RankingsTableColumn(RankingsStrings.KudosuAvailable, Anchor.Centre, new Dimension(GridSizeMode.AutoSize, minSize: min_width)),
                new RankingsTableColumn(RankingsStrings.KudosuUsed, Anchor.Centre, new Dimension(GridSizeMode.AutoSize, minSize: min_width)),
            };
        }

        protected override Drawable[] CreateAdditionalContent(APIUser item)
        {
            int kudosuTotal = item.Kudosu.Total;
            int kudosuAvailable = item.Kudosu.Available;
            return new Drawable[]
            {
                new RowText
                {
                    Text = kudosuTotal.ToLocalisableString(@"N0")
                },
                new ColouredRowText
                {
                    Text = kudosuAvailable.ToLocalisableString(@"N0")
                },
                new ColouredRowText
                {
                    Text = (kudosuTotal - kudosuAvailable).ToLocalisableString(@"N0")
                },
            };
        }

        protected override CountryCode GetCountryCode(APIUser item) => item.CountryCode;

        protected override Drawable CreateFlagContent(APIUser item)
        {
            var username = new LinkFlowContainer(t => t.Font = OsuFont.GetFont(size: TEXT_SIZE, italics: true))
            {
                AutoSizeAxes = Axes.X,
                RelativeSizeAxes = Axes.Y,
                TextAnchor = Anchor.CentreLeft
            };
            username.AddUserLink(item);
            return username;
        }
    }
}
