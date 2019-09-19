// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Profile.Sections.Beatmaps;

namespace osu.Game.Overlays.Profile.Sections
{
    public class BeatmapsSection : ProfileSection
    {
        public override string Title => "Beatmaps";

        public override string Identifier => "beatmaps";

        public BeatmapsSection()
        {
            Children = new[]
            {
                new PaginatedBeatmapContainer(BeatmapSetType.Favourite, User, "Favourite Beatmaps"),
                new PaginatedBeatmapContainer(BeatmapSetType.RankedAndApproved, User, "Ranked & Approved Beatmaps"),
                new PaginatedBeatmapContainer(BeatmapSetType.Loved, User, "Loved Beatmaps"),
                new PaginatedBeatmapContainer(BeatmapSetType.Unranked, User, "Pending Beatmaps"),
                new PaginatedBeatmapContainer(BeatmapSetType.Graveyard, User, "Graveyarded Beatmaps"),
            };
        }
    }
}
