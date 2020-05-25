// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mania.Objects;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Input;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Mania.Edit
{
    [Cached(Type = typeof(IManiaHitObjectComposer))]
    public class ManiaHitObjectComposer : HitObjectComposer<ManiaHitObject>, IManiaHitObjectComposer
    {
        private DrawableManiaEditRuleset drawableRuleset;
        private ManiaBeatSnapGrid beatSnapGrid;
        private InputManager inputManager;

        public ManiaHitObjectComposer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(beatSnapGrid = new ManiaBeatSnapGrid());
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();
        }

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        public ManiaPlayfield Playfield => ((ManiaPlayfield)drawableRuleset.Playfield);

        public IScrollingInfo ScrollingInfo => drawableRuleset.ScrollingInfo;

        public override SnapResult SnapScreenSpacePositionToValidTime(Vector2 screenSpacePosition)
        {
            var column = Playfield.GetColumnByPosition(screenSpacePosition);

            if (column == null)
                return new SnapResult(screenSpacePosition, null);

            double targetTime = column.TimeAtScreenSpacePosition(screenSpacePosition);

            // apply beat snapping
            targetTime = BeatSnapProvider.SnapTime(targetTime);

            // convert back to screen space
            screenSpacePosition = column.ScreenSpacePositionAtTime(targetTime);

            return new ManiaSnapResult(screenSpacePosition, targetTime, column);
        }

        protected override DrawableRuleset<ManiaHitObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
        {
            drawableRuleset = new DrawableManiaEditRuleset(ruleset, beatmap, mods);

            // This is the earliest we can cache the scrolling info to ourselves, before masks are added to the hierarchy and inject it
            dependencies.CacheAs(drawableRuleset.ScrollingInfo);

            return drawableRuleset;
        }

        protected override ComposeBlueprintContainer CreateBlueprintContainer() => new ManiaBlueprintContainer(drawableRuleset.Playfield.AllHitObjects);

        protected override IReadOnlyList<HitObjectCompositionTool> CompositionTools => new HitObjectCompositionTool[]
        {
            new NoteCompositionTool(),
            new HoldNoteCompositionTool()
        };

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (BlueprintContainer.CurrentTool is SelectTool)
            {
                if (EditorBeatmap.SelectedHitObjects.Any())
                {
                    beatSnapGrid.SelectionTimeRange = (EditorBeatmap.SelectedHitObjects.Min(h => h.StartTime), EditorBeatmap.SelectedHitObjects.Max(h => h.GetEndTime()));
                }
                else
                    beatSnapGrid.SelectionTimeRange = null;
            }
            else
            {
                var result = SnapScreenSpacePositionToValidTime(inputManager.CurrentState.Mouse.Position);
                if (result.Time is double time)
                    beatSnapGrid.SelectionTimeRange = (time, time);
                else
                    beatSnapGrid.SelectionTimeRange = null;
            }
        }
    }
}
