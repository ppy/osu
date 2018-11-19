// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Rulesets.Mania.Edit.Blueprints;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI;
using OpenTK;

namespace osu.Game.Rulesets.Mania.Edit
{
    [Cached(Type = typeof(IManiaHitObjectComposer))]
    public class ManiaHitObjectComposer : HitObjectComposer<ManiaHitObject>, IManiaHitObjectComposer
    {
        public ManiaHitObjectComposer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        public Column ColumnAt(Vector2 screenSpacePosition) => ((ManiaPlayfield)RulesetContainer.Playfield).GetColumnByPosition(screenSpacePosition);

        protected override RulesetContainer<ManiaHitObject> CreateRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap)
        {
            var rulesetContainer = new ManiaEditRulesetContainer(ruleset, beatmap);

            // This is the earliest we can cache the scrolling info to ourselves, before masks are added to the hierarchy and inject it
            dependencies.CacheAs(rulesetContainer.ScrollingInfo);

            return rulesetContainer;
        }

        protected override IReadOnlyList<HitObjectCompositionTool> CompositionTools => new HitObjectCompositionTool[]
        {
            new NoteCompositionTool(),
            new HoldNoteCompositionTool()
        };

        public override SelectionBlueprint CreateBlueprintFor(DrawableHitObject hitObject)
        {
            switch (hitObject)
            {
                case DrawableNote note:
                    return new NoteSelectionBlueprint(note);
                case DrawableHoldNote holdNote:
                    return new HoldNoteSelectionBlueprint(holdNote);
            }

            return base.CreateBlueprintFor(hitObject);
        }
    }
}
