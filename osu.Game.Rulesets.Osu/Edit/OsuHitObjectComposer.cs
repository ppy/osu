// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Edit.Masks.HitCircle;
using osu.Game.Rulesets.Osu.Edit.Masks.Slider;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class OsuHitObjectComposer : HitObjectComposer<OsuHitObject>
    {
        public OsuHitObjectComposer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        protected override RulesetContainer<OsuHitObject> CreateRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap)
            => new OsuEditRulesetContainer(ruleset, beatmap);

        protected override IReadOnlyList<HitObjectCompositionTool> CompositionTools => new[]
        {
            new HitCircleCompositionTool(),
        };

        protected override Container CreateLayerContainer() => new PlayfieldAdjustmentContainer { RelativeSizeAxes = Axes.Both };

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
