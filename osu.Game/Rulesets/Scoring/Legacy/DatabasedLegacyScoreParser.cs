// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Scoring.Legacy
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
        protected override WorkingBeatmap GetBeatmap(string md5Hash) => beatmaps.GetWorkingBeatmap(beatmaps.QueryBeatmap(b => b.MD5Hash == md5Hash));
    }
}
