// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
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
    public class OsuHitObjectComposer : DistancedHitObjectComposer<OsuHitObject>
    {
        public OsuHitObjectComposer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        protected override DrawableRuleset<OsuHitObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
            => new DrawableOsuEditorRuleset(ruleset, beatmap, mods);

        protected override IReadOnlyList<HitObjectCompositionTool> CompositionTools => new HitObjectCompositionTool[]
        {
            new HitCircleCompositionTool(),
            new SliderCompositionTool(),
            new SpinnerCompositionTool()
        };

        private readonly Bindable<TernaryState> distanceSnapToggle = new Bindable<TernaryState>();
        private readonly Bindable<TernaryState> rectangularGridSnapToggle = new Bindable<TernaryState>();

        protected override IEnumerable<TernaryButton> CreateTernaryButtons() => base.CreateTernaryButtons().Concat(new[]
        {
            new TernaryButton(distanceSnapToggle, "Distance Snap", () => new SpriteIcon { Icon = FontAwesome.Solid.Ruler }),
            new TernaryButton(rectangularGridSnapToggle, "Grid Snap", () => new SpriteIcon { Icon = FontAwesome.Solid.Th })
        });

        private BindableList<HitObject> selectedHitObjects;

        private Bindable<HitObject> placementObject;

        [BackgroundDependencyLoader]
        private void load()
        {
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
            selectedHitObjects.CollectionChanged += (_, __) => updateDistanceSnapGrid();

            placementObject = EditorBeatmap.PlacementObject.GetBoundCopy();
            placementObject.ValueChanged += _ => updateDistanceSnapGrid();
            distanceSnapToggle.ValueChanged += _ =>
            {
                updateDistanceSnapGrid();

                if (distanceSnapToggle.Value == TernaryState.True)
                    rectangularGridSnapToggle.Value = TernaryState.False;
            };

            rectangularGridSnapToggle.ValueChanged += _ =>
            {
                if (rectangularGridSnapToggle.Value == TernaryState.True)
                    distanceSnapToggle.Value = TernaryState.False;
            };

            // we may be entering the screen with a selection already active
            updateDistanceSnapGrid();
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
                return snapResult;

            if (snapType.HasFlagFast(SnapType.Grids))
            {
                if (distanceSnapToggle.Value == TernaryState.True && distanceSnapGrid != null)
                {
                    (Vector2 pos, double time) = distanceSnapGrid.GetSnappedPosition(distanceSnapGrid.ToLocalSpace(screenSpacePosition));
                    return new SnapResult(distanceSnapGrid.ToScreenSpace(pos), time, PlayfieldAtScreenSpacePosition(screenSpacePosition));
                }

                if (rectangularGridSnapToggle.Value == TernaryState.True)
                {
                    Vector2 pos = rectangularPositionSnapGrid.GetSnappedPosition(rectangularPositionSnapGrid.ToLocalSpace(screenSpacePosition));
                    return new SnapResult(rectangularPositionSnapGrid.ToScreenSpace(pos), null, PlayfieldAtScreenSpacePosition(screenSpacePosition));
                }
            }

            return base.FindSnappedPositionAndTime(screenSpacePosition, snapType);
        }

        private bool snapToVisibleBlueprints(Vector2 screenSpacePosition, out SnapResult snapResult)
        {
            // check other on-screen objects for snapping/stacking
            var blueprints = BlueprintContainer.SelectionBlueprints.AliveChildren;

            var playfield = PlayfieldAtScreenSpacePosition(screenSpacePosition);

            float snapRadius =
                playfield.GamefieldToScreenSpace(new Vector2(OsuHitObject.OBJECT_RADIUS / 5)).X -
                playfield.GamefieldToScreenSpace(Vector2.Zero).X;

            foreach (var b in blueprints)
            {
                if (b.IsSelected)
                    continue;

                var hitObject = (OsuHitObject)b.Item;

                Vector2? snap = checkSnap(hitObject.Position);
                if (snap == null && hitObject.Position != hitObject.EndPosition)
                    snap = checkSnap(hitObject.EndPosition);

                if (snap != null)
                {
                    // only return distance portion, since time is not really valid
                    snapResult = new SnapResult(snap.Value, null, playfield);
                    return true;
                }

                Vector2? checkSnap(Vector2 checkPos)
                {
                    Vector2 checkScreenPos = playfield.GamefieldToScreenSpace(checkPos);

                    if (Vector2.Distance(checkScreenPos, screenSpacePosition) < snapRadius)
                        return checkScreenPos;

                    return null;
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

            if (distanceSnapToggle.Value != TernaryState.True)
                return;

            switch (BlueprintContainer.CurrentTool)
            {
                case SelectTool _:
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
