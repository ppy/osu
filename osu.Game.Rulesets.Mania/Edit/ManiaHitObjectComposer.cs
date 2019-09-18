// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Mania.Edit
{
    [Cached(Type = typeof(IManiaHitObjectComposer))]
    public class ManiaHitObjectComposer : HitObjectComposer<ManiaHitObject>, IManiaHitObjectComposer
    {
        private DrawableManiaEditRuleset drawableRuleset;

        public ManiaHitObjectComposer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        /// <summary>
        /// Retrieves the column that intersects a screen-space position.
        /// </summary>
        /// <param name="screenSpacePosition">The screen-space position.</param>
        /// <returns>The column which intersects with <paramref name="screenSpacePosition"/>.</returns>
        public Column ColumnAt(Vector2 screenSpacePosition) => drawableRuleset.GetColumnByPosition(screenSpacePosition);

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        public int TotalColumns => ((ManiaPlayfield)drawableRuleset.Playfield).TotalColumns;

        protected override DrawableRuleset<ManiaHitObject> CreateDrawableRuleset(Ruleset ruleset, IWorkingBeatmap beatmap, IReadOnlyList<Mod> mods)
        {
            drawableRuleset = new DrawableManiaEditRuleset(ruleset, beatmap, mods);

            // This is the earliest we can cache the scrolling info to ourselves, before masks are added to the hierarchy and inject it
            dependencies.CacheAs(drawableRuleset.ScrollingInfo);

            return drawableRuleset;
        }

        protected override IReadOnlyList<HitObjectCompositionTool> CompositionTools => new HitObjectCompositionTool[]
        {
            new NoteCompositionTool(),
            new HoldNoteCompositionTool()
        };

        public override SelectionHandler CreateSelectionHandler() => new ManiaSelectionHandler();

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
