// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Beatmaps.Drawables.Cards.Statistics
{
    public class BeatmapCardDateStatistic : BeatmapCardStatistic
    {
        private readonly DateTimeOffset dateTime;

        private BeatmapCardDateStatistic(DateTimeOffset dateTime)
        {
            this.dateTime = dateTime;

            Icon = FontAwesome.Regular.CheckCircle;
            Text = dateTime.ToLocalisableString(@"d MMM yyyy");
        }

        public override object TooltipContent => dateTime;
        public override ITooltip GetCustomTooltip() => new DateTooltip();

        public static BeatmapCardDateStatistic? CreateFor(IBeatmapSetOnlineInfo beatmapSetInfo)
        {
            var displayDate = displayDateFor(beatmapSetInfo);

            if (displayDate == null)
                return null;

            return new BeatmapCardDateStatistic(displayDate.Value);
        }

        private static DateTimeOffset? displayDateFor(IBeatmapSetOnlineInfo beatmapSetInfo)
        {
            // reference: https://github.com/ppy/osu-web/blob/ef432c11719fd1207bec5f9194b04f0033bdf02c/resources/assets/lib/beatmapset-panel.tsx#L36-L44
            switch (beatmapSetInfo.Status)
            {
                case BeatmapOnlineStatus.Ranked:
                case BeatmapOnlineStatus.Approved:
                case BeatmapOnlineStatus.Loved:
                case BeatmapOnlineStatus.Qualified:
                    return beatmapSetInfo.Ranked;

                default:
                    return beatmapSetInfo.LastUpdated;
            }
        }
    }
}
