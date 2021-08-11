// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Catch.Edit
{
    public class CatchHitObjectComposer : HitObjectComposer<CatchHitObject>
    {
        public CatchHitObjectComposer(CatchRuleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            LayerBelowRuleset.Add(new PlayfieldBorder
            {
                RelativeSizeAxes = Axes.Both,
                PlayfieldBorderStyle = { Value = PlayfieldBorderStyle.Corners }
            });
        }

        protected override DrawableRuleset<CatchHitObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null) =>
            new DrawableCatchEditorRuleset(ruleset, beatmap, mods);

        protected override IReadOnlyList<HitObjectCompositionTool> CompositionTools => new HitObjectCompositionTool[]
        {
            new FruitCompositionTool(),
            new JuiceStreamCompositionTool(),
            new BananaShowerCompositionTool()
        };

        public override SnapResult SnapScreenSpacePositionToValidTime(Vector2 screenSpacePosition)
        {
            var result = base.SnapScreenSpacePositionToValidTime(screenSpacePosition);
            // TODO: implement position snap
            result.ScreenSpacePosition.X = screenSpacePosition.X;
            return result;
        }

        protected override ComposeBlueprintContainer CreateBlueprintContainer() => new CatchBlueprintContainer(this);
    }
}
