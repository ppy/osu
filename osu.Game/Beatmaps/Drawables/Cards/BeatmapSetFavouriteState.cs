// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps.Drawables.Cards.Buttons;
using osu.Game.Beatmaps.Drawables.Cards.Statistics;

namespace osu.Game.Beatmaps.Drawables.Cards
{
    /// <summary>
    /// Stores the current favourite state of a beatmap set.
    /// Used to coordinate between <see cref="FavouriteButton"/> and <see cref="FavouritesStatistic"/>.
    /// </summary>
    public readonly struct BeatmapSetFavouriteState
    {
        /// <summary>
        /// Whether the currently logged-in user has favourited this beatmap.
        /// </summary>
        public bool Favourited { get; }

        /// <summary>
        /// The number of favourites that the beatmap set has received, including the currently logged-in user.
        /// </summary>
        public int FavouriteCount { get; }

        public BeatmapSetFavouriteState(bool favourited, int favouriteCount)
        {
            Favourited = favourited;
            FavouriteCount = favouriteCount;
        }
    }
}
