// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mania.Edit.Layers.Selection.Overlays;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using System.Collections.Generic;
using osu.Game.Rulesets.Mania.Edit.Layers;
using osu.Game.Screens.Edit.Screens.Compose.Layers;

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
            new HitObjectCompositionTool<Note>("Note"),
            new HitObjectCompositionTool<HoldNote>("Hold"),
        };

        public override HitObjectMask CreateMaskFor(DrawableHitObject hitObject)
        {
            switch (hitObject)
            {
                case DrawableNote note:
                    return new NoteMask(note);
                case DrawableHoldNote holdNote:
                    return new HoldNoteMask(holdNote);
            }

            return base.CreateMaskFor(hitObject);
        }

        protected override HitObjectMaskLayer CreateHitObjectMaskLayer() => new ManiaHitObjectMaskLayer();
    }
}
