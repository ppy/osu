// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Taiko.Edit
{
    public partial class DrawableTaikoEditorRuleset : DrawableTaikoRuleset, ISupportConstantAlgorithmToggle
    {
        public BindableBool ShowSpeedChanges { get; set; } = new BindableBool();

        public DrawableTaikoEditorRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(ruleset, beatmap, mods)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ShowSpeedChanges.BindValueChanged(showChanges => VisualisationMethod = showChanges.NewValue ? ScrollVisualisationMethod.Overlapping : ScrollVisualisationMethod.Constant, true);
        }

        protected override double ComputeTimeRange()
        {
            // Adjust when we're using constant algorithm to not be sluggish.
            double multiplier = ShowSpeedChanges.Value ? 1 : 4;
            return base.ComputeTimeRange() / multiplier;
        }
    }
}
