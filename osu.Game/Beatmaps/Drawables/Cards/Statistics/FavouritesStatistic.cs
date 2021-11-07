// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Beatmaps.Drawables.Cards.Statistics
{
    /// <summary>
    /// Shows the number of favourites that a beatmap set has received.
    /// </summary>
    public class FavouritesStatistic : BeatmapCardStatistic, IHasCurrentValue<BeatmapSetFavouriteState>
    {
        private readonly BindableWithCurrent<BeatmapSetFavouriteState> current;

        public Bindable<BeatmapSetFavouriteState> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        public FavouritesStatistic(IBeatmapSetOnlineInfo onlineInfo)
        {
            current = new BindableWithCurrent<BeatmapSetFavouriteState>(new BeatmapSetFavouriteState(onlineInfo.HasFavourited, onlineInfo.FavouriteCount));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            current.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            Icon = current.Value.Favourited ? FontAwesome.Solid.Heart : FontAwesome.Regular.Heart;
            Text = current.Value.FavouriteCount.ToMetric(decimals: 1);
            TooltipText = BeatmapsStrings.PanelFavourites(current.Value.FavouriteCount.ToLocalisableString(@"N0"));
        }
    }
}
