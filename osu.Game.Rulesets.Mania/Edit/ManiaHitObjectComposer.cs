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

        /// <summary>
        /// Retrieves the column that intersects a screen-space position.
        /// </summary>
        /// <param name="screenSpacePosition">The screen-space position.</param>
        /// <returns>The column which intersects with <paramref name="screenSpacePosition"/>.</returns>
        public Column ColumnAt(Vector2 screenSpacePosition) => drawableRuleset.GetColumnByPosition(screenSpacePosition);

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            => dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        public ManiaPlayfield Playfield => ((ManiaPlayfield)drawableRuleset.Playfield);

        public IScrollingInfo ScrollingInfo => drawableRuleset.ScrollingInfo;

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (BlueprintContainer.CurrentTool is SelectTool)
            {
                if (EditorBeatmap.SelectedHitObjects.Any())
                {
                    beatSnapGrid.SetRange(EditorBeatmap.SelectedHitObjects.Min(h => h.StartTime), EditorBeatmap.SelectedHitObjects.Max(h => h.GetEndTime()));
                    beatSnapGrid.Show();
                }
                else
                    beatSnapGrid.Hide();
            }
            else
            {
                var placementTime = GetSnappedPosition(ToLocalSpace(inputManager.CurrentState.Mouse.Position), 0).time;
                beatSnapGrid.SetRange(placementTime, placementTime);

                beatSnapGrid.Show();
            }
        }

        public override (Vector2 position, double time) GetSnappedPosition(Vector2 position, double time)
        {
            var beatSnapped = beatSnapGrid.GetSnappedPosition(position);

            if (beatSnapped != null)
                return beatSnapped.Value;

            return base.GetSnappedPosition(position, getTimeFromPosition(ToScreenSpace(position)));
        }

        private double getTimeFromPosition(Vector2 screenSpacePosition)
        {
            var hoc = Playfield.Stages[0].HitObjectContainer;
            float targetPosition = hoc.ToLocalSpace(screenSpacePosition).Y;

            if (drawableRuleset.ScrollingInfo.Direction.Value == ScrollingDirection.Down)
            {
                // We're dealing with screen coordinates in which the position decreases towards the centre of the screen resulting in an increase in start time.
                // The scrolling algorithm instead assumes a top anchor meaning an increase in time corresponds to an increase in position,
                // so when scrolling downwards the coordinates need to be flipped.
                targetPosition = hoc.DrawHeight - targetPosition;
            }

            return drawableRuleset.ScrollingInfo.Algorithm.TimeAt(targetPosition,
                EditorClock.CurrentTime,
                drawableRuleset.ScrollingInfo.TimeRange.Value,
                hoc.DrawHeight);
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
    }
}
