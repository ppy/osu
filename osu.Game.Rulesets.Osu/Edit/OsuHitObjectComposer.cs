// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Edit.Masks;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class OsuHitObjectComposer : HitObjectComposer
    {
        public OsuHitObjectComposer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        protected override EditRulesetContainer CreateRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap) => new OsuEditRulesetContainer(ruleset, beatmap);

        protected override IReadOnlyList<HitObjectCompositionTool> CompositionTools => new[]
        {
            new HitCircleCompositionTool(),
        };

        protected override ScalableContainer CreateLayerContainer() => new ScalableContainer(OsuPlayfield.BASE_SIZE.X) { RelativeSizeAxes = Axes.Both };

        public override SelectionMask CreateMaskFor(DrawableHitObject hitObject)
        {
            switch (hitObject)
            {
                case DrawableHitCircle circle:
                    return new HitCircleSelectionMask(circle);
                case DrawableSlider slider:
                    return new SliderSelectionMask(slider);
            }

            return base.CreateMaskFor(hitObject);
        }
    }
}
