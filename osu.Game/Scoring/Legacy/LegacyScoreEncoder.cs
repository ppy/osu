// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;

namespace osu.Game.Scoring.Legacy
{
    public class LegacyScoreEncoder
    {
        public const int LATEST_VERSION = 128;

        private readonly Score score;

        public LegacyScoreEncoder(Score score)
        {
            this.score = score;

            if (score.ScoreInfo.Beatmap.RulesetID < 0 || score.ScoreInfo.Beatmap.RulesetID > 3)
                throw new ArgumentException("Only scores in the osu, taiko, catch, or mania rulesets can be encoded to the legacy score format.", nameof(score));
        }

        public void Encode(TextWriter writer)
        {
            writer.WriteLine($"osu file format v{LATEST_VERSION}");

            writer.WriteLine();
            handleGeneral(writer);
        }

        private void handleGeneral(TextWriter writer)
        {
        }
    }
}
