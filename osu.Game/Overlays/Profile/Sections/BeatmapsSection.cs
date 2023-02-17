// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Profile.Sections.Beatmaps;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Profile.Sections
{
    public partial class BeatmapsSection : ProfileSection
    {
        public override LocalisableString Title => UsersStrings.ShowExtraBeatmapsTitle;

        public override string Identifier => @"beatmaps";

        public BeatmapsSection()
        {
            Children = new[]
            {
                new PaginatedBeatmapContainer(BeatmapSetType.Favourite, User, UsersStrings.ShowExtraBeatmapsFavouriteTitle),
                new PaginatedBeatmapContainer(BeatmapSetType.Ranked, User, UsersStrings.ShowExtraBeatmapsRankedTitle),
                new PaginatedBeatmapContainer(BeatmapSetType.Loved, User, UsersStrings.ShowExtraBeatmapsLovedTitle),
                new PaginatedBeatmapContainer(BeatmapSetType.Guest, User, UsersStrings.ShowExtraBeatmapsGuestTitle),
                new PaginatedBeatmapContainer(BeatmapSetType.Pending, User, UsersStrings.ShowExtraBeatmapsPendingTitle),
                new PaginatedBeatmapContainer(BeatmapSetType.Graveyard, User, UsersStrings.ShowExtraBeatmapsGraveyardTitle),
                new PaginatedBeatmapContainer(BeatmapSetType.Nominated, User, UsersStrings.ShowExtraBeatmapsNominatedTitle),
            };
        }
    }
}
