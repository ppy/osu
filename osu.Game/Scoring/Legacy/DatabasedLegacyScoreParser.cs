// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets;

namespace osu.Game.Scoring.Legacy
{
    /// <summary>
    /// A <see cref="LegacyScoreParser"/> which retrieves the applicable <see cref="Beatmap"/> and <see cref="Ruleset"/>
    /// for the score from the database.
    /// </summary>
    public class DatabasedLegacyScoreParser : LegacyScoreParser
    {
        private readonly RulesetStore rulesets;
        private readonly BeatmapManager beatmaps;

        public DatabasedLegacyScoreParser(RulesetStore rulesets, BeatmapManager beatmaps)
        {
            this.rulesets = rulesets;
            this.beatmaps = beatmaps;
        }

        protected override Ruleset GetRuleset(int rulesetId) => rulesets.GetRuleset(rulesetId).CreateInstance();
        protected override WorkingBeatmap GetBeatmap(string md5Hash) => beatmaps.GetWorkingBeatmap(beatmaps.QueryBeatmap(b => !b.BeatmapSet.DeletePending && b.MD5Hash == md5Hash));
    }
}
