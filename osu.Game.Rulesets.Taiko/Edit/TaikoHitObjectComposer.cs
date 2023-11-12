// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Taiko.Edit
{
    public partial class TaikoHitObjectComposer : ScrollingHitObjectComposer<TaikoHitObject>
    {
        protected override bool ApplyHorizontalCentering => false;

        public TaikoHitObjectComposer(TaikoRuleset ruleset)
            : base(ruleset)
        {
        }

        protected override IReadOnlyList<HitObjectCompositionTool> CompositionTools => new HitObjectCompositionTool[]
        {
            new HitCompositionTool(),
            new DrumRollCompositionTool(),
            new SwellCompositionTool()
        };

        protected override DrawableRuleset<TaikoHitObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods) =>
            new DrawableTaikoEditorRuleset(ruleset, beatmap, mods);

        protected override ComposeBlueprintContainer CreateBlueprintContainer()
            => new TaikoBlueprintContainer(this);

        protected override BeatSnapGrid CreateBeatSnapGrid() => new TaikoBeatSnapGrid();
    }
}
