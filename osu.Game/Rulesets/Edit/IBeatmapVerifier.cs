// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A class which can run against a beatmap and surface issues to the user which could go against known criteria or hinder gameplay.
    /// </summary>
    public interface IBeatmapVerifier
    {
        public class Context
        {
            /// <summary>
            /// The working beatmap instance of the current beatmap.
            /// </summary>
            public readonly IWorkingBeatmap WorkingBeatmap;

            /// <summary>
            /// The difficulty level which the current beatmap is considered to be.
            /// </summary>
            public readonly Bindable<DifficultyRating> InterpretedDifficulty;

            public Context(IWorkingBeatmap workingBeatmap)
            {
                WorkingBeatmap = workingBeatmap;
                InterpretedDifficulty = new Bindable<DifficultyRating>();
            }
        }

        public IEnumerable<Issue> Run(IBeatmap beatmap, Context context);
    }
}
