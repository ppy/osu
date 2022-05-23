// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets;

namespace osu.Game.Scoring.Legacy
{
    /// <summary>
    /// A <see cref="LegacyScoreDecoder"/> which retrieves the applicable <see cref="Beatmap"/> and <see cref="Ruleset"/>
    /// for the score from the database.
    /// </summary>
    public class DatabasedLegacyScoreDecoder : LegacyScoreDecoder
    {
        private readonly IRulesetStore rulesets;
        private readonly BeatmapManager beatmaps;

        public DatabasedLegacyScoreDecoder(IRulesetStore rulesets, BeatmapManager beatmaps)
        {
            this.rulesets = rulesets;
            this.beatmaps = beatmaps;
        }

        protected override Ruleset GetRuleset(int rulesetId) => rulesets.GetRuleset(rulesetId)?.CreateInstance();
        protected override WorkingBeatmap GetBeatmap(string md5Hash) => beatmaps.GetWorkingBeatmap(beatmaps.QueryBeatmap(b => b.MD5Hash == md5Hash));
    }
}
