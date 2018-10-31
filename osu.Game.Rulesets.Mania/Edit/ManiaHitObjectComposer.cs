// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.Edit.Masks;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaHitObjectComposer : HitObjectComposer<ManiaHitObject>
    {
        protected new ManiaConfigManager Config => (ManiaConfigManager)base.Config;

        public ManiaHitObjectComposer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.CacheAs<IScrollingInfo>(new ManiaScrollingInfo(Config));
            return dependencies;
        }

        protected override RulesetContainer<ManiaHitObject> CreateRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap)
            => new ManiaEditRulesetContainer(ruleset, beatmap);

        protected override IReadOnlyList<HitObjectCompositionTool> CompositionTools => Array.Empty<HitObjectCompositionTool>();

        public override SelectionMask CreateMaskFor(DrawableHitObject hitObject)
        {
            switch (hitObject)
            {
                case DrawableNote note:
                    return new NoteSelectionMask(note);
                case DrawableHoldNote holdNote:
                    return new HoldNoteSelectionMask(holdNote);
            }

            return base.CreateMaskFor(hitObject);
        }
    }
}
