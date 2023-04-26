// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Gameplay
{
    /// <summary>
    /// Static class providing a <see cref="Create"/> convenience method to retrieve a correctly-initialised <see cref="GameplayState"/> instance in testing scenarios.
    /// </summary>
    public static class TestGameplayState
    {
        /// <summary>
        /// Creates a correctly-initialised <see cref="GameplayState"/> instance for use in testing.
        /// </summary>
        public static GameplayState Create(Ruleset ruleset, IReadOnlyList<Mod>? mods = null, Score? score = null)
        {
            var beatmap = new TestBeatmap(ruleset.RulesetInfo);
            var workingBeatmap = new TestWorkingBeatmap(beatmap);
            var playableBeatmap = workingBeatmap.GetPlayableBeatmap(ruleset.RulesetInfo, mods);

            var scoreProcessor = ruleset.CreateScoreProcessor();
            scoreProcessor.ApplyBeatmap(playableBeatmap);

            return new GameplayState(playableBeatmap, ruleset, mods, score, scoreProcessor);
        }
    }
}
