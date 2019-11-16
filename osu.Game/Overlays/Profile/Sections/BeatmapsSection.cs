// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Profile.Sections.Beatmaps;

namespace osu.Game.Overlays.Profile.Sections
{
    public class BeatmapsSection : ProfileSection
    {
        public override string Title => "谱面";

        public override string Identifier => "谱面";

        public BeatmapsSection()
        {
            Children = new[]
            {
                new PaginatedBeatmapContainer(BeatmapSetType.Favourite, User, "收藏的谱面"),
                new PaginatedBeatmapContainer(BeatmapSetType.RankedAndApproved, User, "Ranked & Approved的谱面"),
                new PaginatedBeatmapContainer(BeatmapSetType.Loved, User, "Loved的谱面"),
                new PaginatedBeatmapContainer(BeatmapSetType.Unranked, User, "Pending的谱面"),
                new PaginatedBeatmapContainer(BeatmapSetType.Graveyard, User, "坟场里的谱面"),
            };
        }
    }
}
