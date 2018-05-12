// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Objects.Drawables;
//using osu.Game.Rulesets.Mania.Edit.Layers.Selection.Overlays;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaHitObjectComposer : HitObjectComposer
    {
        public ManiaHitObjectComposer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        protected override RulesetContainer CreateRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap) => new ManiaEditRulesetContainer(ruleset, beatmap);

        protected override IReadOnlyList<ICompositionTool> CompositionTools => new ICompositionTool[]
        {
            new HitObjectCompositionTool<Note>(),
            new HitObjectCompositionTool<HoldNote>(),
        };

        // TODO: According to another proposal, extend this to support multiple layers for mania maps
        // The logic could be moving all the layers that the beatmap has simultaneously
        // To avoid using too many resources, this could be changed to simply changing the Alpha to something
        // between 0.25f to 0.5f for notes that are in other layers (and may be also not selected)
        // Will also need a tool to navigate through layers
        // Please ignore the comment above, I just wanted to write my thoughts down so that I do not forget in 2 months when I get around to it
        // Uncomment?
        //protected override ScalableContainer CreateLayerContainer() => new ScalableContainer(1) { RelativeSizeAxes = Axes.Both };

        //public override HitObjectMask CreateMaskFor(DrawableHitObject hitObject)
        //{
        //    switch (hitObject)
        //    {
        //        case DrawableNote circle:
        //            return new NoteMask(circle);
        //        case DrawableHoldNote slider:
        //            return new HoldNoteMask(slider);
        //    }

        //    return base.CreateMaskFor(hitObject);
        //}
    }
}
