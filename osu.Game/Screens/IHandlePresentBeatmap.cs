// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets;

namespace osu.Game.Screens
{
    /// <summary>
    /// Denotes a screen which can handle beatmap / ruleset selection via local logic.
    /// This is used in the <see cref="OsuGame.PresentBeatmap"/> flow to handle cases which require custom logic,
    /// for instance, if a lease is held on the Beatmap.
    /// </summary>
    public interface IHandlePresentBeatmap
    {
        /// <summary>
        /// Invoked with a requested beatmap / ruleset for selection.
        /// </summary>
        /// <param name="beatmap">The beatmap to be selected.</param>
        /// <param name="ruleset">The ruleset to be selected.</param>
        void PresentBeatmap(WorkingBeatmap beatmap, RulesetInfo ruleset);
    }
}
