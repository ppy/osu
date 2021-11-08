// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Beatmaps.Drawables.Cards.Statistics
{
    /// <summary>
    /// Shows the number of favourites that a beatmap set has received.
    /// </summary>
    public class FavouritesStatistic : BeatmapCardStatistic
    {
        public FavouritesStatistic(IBeatmapSetOnlineInfo onlineInfo)
        {
            Icon = onlineInfo.HasFavourited ? FontAwesome.Solid.Heart : FontAwesome.Regular.Heart;
            Text = onlineInfo.FavouriteCount.ToMetric(decimals: 1);
            TooltipText = BeatmapsStrings.PanelFavourites(onlineInfo.FavouriteCount.ToLocalisableString(@"N0"));
        }
    }
}
