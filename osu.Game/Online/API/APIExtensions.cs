// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API
{
    public static class APIExtensions
    {
        /// <summary>
        /// Returns the current favourite state of the given beatmap.
        /// This should be called whenever a change in <see cref="IAPIProvider.BeatmapFavourites"/> has occurred.
        /// </summary>
        // todo: this should eventually be moved to a separate component, away from IAPIProvider / APIAccess.
        public static BeatmapSetFavouriteState GetFavouriteState(this IAPIProvider api, APIBeatmapSet beatmapSet)
        {
            bool favourited = api.BeatmapFavourites.Any(s => s.OnlineID == beatmapSet.OnlineID);
            int favouriteCount = beatmapSet.FavouriteCount;

            if (beatmapSet.HasFavourited != favourited)
                favouriteCount += favourited ? 1 : -1;

            return new BeatmapSetFavouriteState(favourited, favouriteCount);
        }
    }
}
