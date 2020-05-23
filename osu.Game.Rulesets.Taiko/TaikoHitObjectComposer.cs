// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Taiko
{
    public class TaikoHitObjectComposer : HitObjectComposer<TaikoHitObject>
    {
        private DrawableTaikoRuleset drawableRuleset;

        public TaikoHitObjectComposer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        protected override IReadOnlyList<HitObjectCompositionTool> CompositionTools => new List<HitObjectCompositionTool>();

        protected override ComposeBlueprintContainer CreateBlueprintContainer() => new ComposeBlueprintContainer(drawableRuleset.Playfield.AllHitObjects);

        protected override DrawableRuleset<TaikoHitObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
        {
            return drawableRuleset = new DrawableTaikoRuleset(ruleset, beatmap, mods);
        }
    }
}
