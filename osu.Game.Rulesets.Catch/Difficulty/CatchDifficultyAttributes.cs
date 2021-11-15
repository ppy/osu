// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Catch.Difficulty
{
    public class CatchDifficultyAttributes : DifficultyAttributes
    {
        [JsonProperty("approach_rate")]
        public double ApproachRate { get; set; }
    }
}
