// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Profile.Sections.Beatmaps;

namespace osu.Game.Overlays.Profile.Sections
{
    public class BeatmapsSection : ProfileSection
    {
        public override string Title => "谱面";

        public override string Identifier => "beatmaps";

        public BeatmapsSection()
        {
            Children = new[]
            {
                new PaginatedBeatmapContainer(BeatmapSetType.Favourite, User, "喜欢的谱面"),
                new PaginatedBeatmapContainer(BeatmapSetType.RankedAndApproved, User, "计入排名的谱面"),
                new PaginatedBeatmapContainer(BeatmapSetType.Loved, User, "Loved谱面"),
                new PaginatedBeatmapContainer(BeatmapSetType.Unranked, User, "审核中/制作中的谱面"),
                new PaginatedBeatmapContainer(BeatmapSetType.Graveyard, User, "坟场中的谱面"),
            };
        }
    }
}
