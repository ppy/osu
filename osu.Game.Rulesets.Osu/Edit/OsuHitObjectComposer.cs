// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Components.TernaryButtons;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class OsuHitObjectComposer : DistancedHitObjectComposer<OsuHitObject>
    {
        public OsuHitObjectComposer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        protected override DrawableRuleset<OsuHitObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods)
            => new DrawableOsuEditorRuleset(ruleset, beatmap, mods);

        protected override IReadOnlyList<HitObjectCompositionTool> CompositionTools => new HitObjectCompositionTool[]
        {
            new HitCircleCompositionTool(),
            new SliderCompositionTool(),
            new SpinnerCompositionTool()
        };

        private readonly Bindable<TernaryState> rectangularGridSnapToggle = new Bindable<TernaryState>();

        protected override IEnumerable<TernaryButton> CreateTernaryButtons() => base.CreateTernaryButtons().Concat(new[]
        {
            new TernaryButton(rectangularGridSnapToggle, "Grid Snap", () => new SpriteIcon { Icon = FontAwesome.Solid.Th })
        });

        private BindableList<HitObject> selectedHitObjects;

        private Bindable<HitObject> placementObject;

        [BackgroundDependencyLoader]
        private void load()
        {
            // Give a bit of breathing room around the playfield content.
            PlayfieldContentContainer.Padding = new MarginPadding(10);

            LayerBelowRuleset.AddRange(new Drawable[]
            {
                distanceSnapGridContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both
                },
                rectangularPositionSnapGrid = new OsuRectangularPositionSnapGrid
                {
                    RelativeSizeAxes = Axes.Both
                }
            });

            selectedHitObjects = EditorBeatmap.SelectedHitObjects.GetBoundCopy();
            selectedHitObjects.CollectionChanged += (_, _) => updateDistanceSnapGrid();

            placementObject = EditorBeatmap.PlacementObject.GetBoundCopy();
            placementObject.ValueChanged += _ => updateDistanceSnapGrid();
            DistanceSnapToggle.ValueChanged += _ => updateDistanceSnapGrid();

            // we may be entering the screen with a selection already active
            updateDistanceSnapGrid();

            RightToolbox.Add(new TransformToolboxGroup
            {
                RotationHandler = BlueprintContainer.SelectionHandler.RotationHandler
            });
        }

        protected override ComposeBlueprintContainer CreateBlueprintContainer()
            => new OsuBlueprintContainer(this);

        public override string ConvertSelectionToString()
            => string.Join(',', selectedHitObjects.Cast<OsuHitObject>().OrderBy(h => h.StartTime).Select(h => (h.IndexInCurrentCombo + 1).ToString()));

        private DistanceSnapGrid distanceSnapGrid;
        private Container distanceSnapGridContainer;

        private readonly Cached distanceSnapGridCache = new Cached();
        private double? lastDistanceSnapGridTime;

        private RectangularPositionSnapGrid rectangularPositionSnapGrid;

        protected override double ReadCurrentDistanceSnap(HitObject before, HitObject after)
        {
            float expectedDistance = DurationToDistance(before, after.StartTime - before.GetEndTime());
            float actualDistance = Vector2.Distance(((OsuHitObject)before).EndPosition, ((OsuHitObject)after).Position);

            return actualDistance / expectedDistance;
        }

        protected override void Update()
        {
            base.Update();

            if (!(BlueprintContainer.CurrentTool is SelectTool))
            {
                if (EditorClock.CurrentTime != lastDistanceSnapGridTime)
                {
                    distanceSnapGridCache.Invalidate();
                    lastDistanceSnapGridTime = EditorClock.CurrentTime;
                }

                if (!distanceSnapGridCache.IsValid)
                    updateDistanceSnapGrid();
            }
        }

        public override SnapResult FindSnappedPositionAndTime(Vector2 screenSpacePosition, SnapType snapType = SnapType.All)
        {
            if (snapType.HasFlagFast(SnapType.NearbyObjects) && snapToVisibleBlueprints(screenSpacePosition, out var snapResult))
            {
                // In the case of snapping to nearby objects, a time value is not provided.
                // This matches the stable editor (which also uses current time), but with the introduction of time-snapping distance snap
                // this could result in unexpected behaviour when distance snapping is turned on and a user attempts to place an object that is
                // BOTH on a valid distance snap ring, and also at the same position as a previous object.
                //
                // We want to ensure that in this particular case, the time-snapping component of distance snap is still applied.
                // The easiest way to ensure this is to attempt application of distance snap after a nearby object is found, and copy over
                // the time value if the proposed positions are roughly the same.
                if (snapType.HasFlagFast(SnapType.RelativeGrids) && DistanceSnapToggle.Value == TernaryState.True && distanceSnapGrid != null)
                {
                    (Vector2 distanceSnappedPosition, double distanceSnappedTime) = distanceSnapGrid.GetSnappedPosition(distanceSnapGrid.ToLocalSpace(snapResult.ScreenSpacePosition));
                    if (Precision.AlmostEquals(distanceSnapGrid.ToScreenSpace(distanceSnappedPosition), snapResult.ScreenSpacePosition, 1))
                        snapResult.Time = distanceSnappedTime;
                }

                return snapResult;
            }

            SnapResult result = base.FindSnappedPositionAndTime(screenSpacePosition, snapType);

            if (snapType.HasFlagFast(SnapType.RelativeGrids))
            {
                if (DistanceSnapToggle.Value == TernaryState.True && distanceSnapGrid != null)
                {
                    (Vector2 pos, double time) = distanceSnapGrid.GetSnappedPosition(distanceSnapGrid.ToLocalSpace(screenSpacePosition));

                    result.ScreenSpacePosition = distanceSnapGrid.ToScreenSpace(pos);
                    result.Time = time;
                }
            }

            if (snapType.HasFlagFast(SnapType.GlobalGrids))
            {
                if (rectangularGridSnapToggle.Value == TernaryState.True)
                {
                    Vector2 pos = rectangularPositionSnapGrid.GetSnappedPosition(rectangularPositionSnapGrid.ToLocalSpace(result.ScreenSpacePosition));

                    result.ScreenSpacePosition = rectangularPositionSnapGrid.ToScreenSpace(pos);
                }
            }

            return result;
        }

        private bool snapToVisibleBlueprints(Vector2 screenSpacePosition, out SnapResult snapResult)
        {
            // check other on-screen objects for snapping/stacking
            var blueprints = BlueprintContainer.SelectionBlueprints.AliveChildren;

            var playfield = PlayfieldAtScreenSpacePosition(screenSpacePosition);

            float snapRadius =
                playfield.GamefieldToScreenSpace(new Vector2(OsuHitObject.OBJECT_RADIUS * 0.10f)).X -
                playfield.GamefieldToScreenSpace(Vector2.Zero).X;

            foreach (var b in blueprints)
            {
                if (b.IsSelected)
                    continue;

                var snapPositions = b.ScreenSpaceSnapPoints;

                if (!snapPositions.Any())
                    continue;

                var closestSnapPosition = snapPositions.MinBy(p => Vector2.Distance(p, screenSpacePosition));

                if (Vector2.Distance(closestSnapPosition, screenSpacePosition) < snapRadius)
                {
                    // only return distance portion, since time is not really valid
                    snapResult = new SnapResult(closestSnapPosition, null, playfield);
                    return true;
                }
            }

            snapResult = null;
            return false;
        }

        private void updateDistanceSnapGrid()
        {
            distanceSnapGridContainer.Clear();
            distanceSnapGridCache.Invalidate();
            distanceSnapGrid = null;

            if (DistanceSnapToggle.Value != TernaryState.True)
                return;

            switch (BlueprintContainer.CurrentTool)
            {
                case SelectTool:
                    if (!EditorBeatmap.SelectedHitObjects.Any())
                        return;

                    distanceSnapGrid = createDistanceSnapGrid(EditorBeatmap.SelectedHitObjects);
                    break;

                default:
                    if (!CursorInPlacementArea)
                        return;

                    distanceSnapGrid = createDistanceSnapGrid(Enumerable.Empty<HitObject>());
                    break;
            }

            if (distanceSnapGrid != null)
            {
                distanceSnapGridContainer.Add(distanceSnapGrid);
                distanceSnapGridCache.Validate();
            }
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat)
                return false;

            handleToggleViaKey(e);
            return base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyUpEvent e)
        {
            handleToggleViaKey(e);
            base.OnKeyUp(e);
        }

        protected override bool AdjustDistanceSpacing(GlobalAction action, float amount)
        {
            // To allow better visualisation, ensure that the spacing grid is visible before adjusting.
            DistanceSnapToggle.Value = TernaryState.True;

            return base.AdjustDistanceSpacing(action, amount);
        }

        private bool gridSnapMomentary;

        private void handleToggleViaKey(KeyboardEvent key)
        {
            bool shiftPressed = key.ShiftPressed;

            if (shiftPressed != gridSnapMomentary)
            {
                gridSnapMomentary = shiftPressed;
                rectangularGridSnapToggle.Value = rectangularGridSnapToggle.Value == TernaryState.False ? TernaryState.True : TernaryState.False;
            }
        }

        private DistanceSnapGrid createDistanceSnapGrid(IEnumerable<HitObject> selectedHitObjects)
        {
            if (BlueprintContainer.CurrentTool is SpinnerCompositionTool)
                return null;

            var objects = selectedHitObjects.ToList();

            if (objects.Count == 0)
                // use accurate time value to give more instantaneous feedback to the user.
                return createGrid(h => h.StartTime <= EditorClock.CurrentTimeAccurate);

            double minTime = objects.Min(h => h.StartTime);
            return createGrid(h => h.StartTime < minTime, objects.Count + 1);
        }

        /// <summary>
        /// Creates a grid from the last <see cref="HitObject"/> matching a predicate to a target <see cref="HitObject"/>.
        /// </summary>
        /// <param name="sourceSelector">A predicate that matches <see cref="HitObject"/>s where the grid can start from.
        /// Only the last <see cref="HitObject"/> matching the predicate is used.</param>
        /// <param name="targetOffset">An offset from the <see cref="HitObject"/> selected via <paramref name="sourceSelector"/> at which the grid should stop.</param>
        /// <returns>The <see cref="OsuDistanceSnapGrid"/> from a selected <see cref="HitObject"/> to a target <see cref="HitObject"/>.</returns>
        private OsuDistanceSnapGrid createGrid(Func<HitObject, bool> sourceSelector, int targetOffset = 1)
        {
            if (targetOffset < 1) throw new ArgumentOutOfRangeException(nameof(targetOffset));

            int sourceIndex = -1;

            for (int i = 0; i < EditorBeatmap.HitObjects.Count; i++)
            {
                if (!sourceSelector(EditorBeatmap.HitObjects[i]))
                    break;

                sourceIndex = i;
            }

            if (sourceIndex == -1)
                return null;

            HitObject sourceObject = EditorBeatmap.HitObjects[sourceIndex];

            int targetIndex = sourceIndex + targetOffset;
            HitObject targetObject = null;

            // Keep advancing the target object while its start time falls before the end time of the source object
            while (true)
            {
                if (targetIndex >= EditorBeatmap.HitObjects.Count)
                    break;

                if (EditorBeatmap.HitObjects[targetIndex].StartTime >= sourceObject.GetEndTime())
                {
                    targetObject = EditorBeatmap.HitObjects[targetIndex];
                    break;
                }

                targetIndex++;
            }

            if (sourceObject is Spinner)
                return null;

            return new OsuDistanceSnapGrid((OsuHitObject)sourceObject, (OsuHitObject)targetObject);
        }
    }
}
