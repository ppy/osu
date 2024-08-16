// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Scoring;
using Realms;

namespace osu.Game.Beatmaps
{
    public class BeatmapUserRank : EmbeddedObject
    {
        public int Rank { get; set; } = (int)ScoreRank.F - 1;

        [Ignored]
        public bool Updated { get; set; }
    }
}
