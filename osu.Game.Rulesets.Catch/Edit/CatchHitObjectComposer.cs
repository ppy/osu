// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Components.TernaryButtons;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Catch.Edit
{
    public partial class CatchHitObjectComposer : ScrollingHitObjectComposer<CatchHitObject>, IKeyBindingHandler<GlobalAction>
    {
        private const float distance_snap_radius = 50;

        private CatchDistanceSnapGrid distanceSnapGrid = null!;

        private readonly BindableDouble timeRangeMultiplier = new BindableDouble(1)
        {
            MinValue = 1,
            MaxValue = 10,
        };

        [Cached(typeof(IDistanceSnapProvider))]
        protected readonly CatchDistanceSnapProvider DistanceSnapProvider = new CatchDistanceSnapProvider();

        private BeatSnapGrid beatSnapGrid = null!;

        public CatchHitObjectComposer(CatchRuleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(DistanceSnapProvider);
            DistanceSnapProvider.AttachToToolbox(RightToolbox);

            // todo: enable distance spacing once catch supports applying it to its existing distance snap grid implementation.
            DistanceSnapProvider.DistanceSpacingMultiplier.Disabled = true;

            LayerBelowRuleset.Add(new PlayfieldBorder
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.X,
                Height = CatchPlayfield.HEIGHT,
                PlayfieldBorderStyle = { Value = PlayfieldBorderStyle.Corners }
            });

            LayerBelowRuleset.Add(distanceSnapGrid = new CatchDistanceSnapGrid(new[]
            {
                0.0,
                Catcher.BASE_DASH_SPEED, -Catcher.BASE_DASH_SPEED,
                Catcher.BASE_WALK_SPEED, -Catcher.BASE_WALK_SPEED,
            }));

            AddInternal(beatSnapGrid = new CatchBeatSnapGrid());
        }

        protected override IEnumerable<TernaryButton> CreateTernaryButtons()
            => base.CreateTernaryButtons()
                   .Concat(DistanceSnapProvider.CreateTernaryButtons());

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
                var result = FindSnappedPositionAndTime(InputManager.CurrentState.Mouse.Position);
                if (result.Time is double time)
                    beatSnapGrid.SelectionTimeRange = (time, time);
                else
                    beatSnapGrid.SelectionTimeRange = null;
            }
        }

        protected override void Update()
        {
            base.Update();

            updateDistanceSnapGrid();
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                // Note that right now these are hard to use as the default key bindings conflict with existing editor key bindings.
                // In the future we will want to expose this via UI and potentially change the key bindings to be editor-specific.
                // May be worth considering standardising "zoom" behaviour with what the timeline uses (ie. alt-wheel) but that may cause new conflicts.
                case GlobalAction.IncreaseScrollSpeed:
                    this.TransformBindableTo(timeRangeMultiplier, timeRangeMultiplier.Value - 1, 200, Easing.OutQuint);
                    return true;

                case GlobalAction.DecreaseScrollSpeed:
                    this.TransformBindableTo(timeRangeMultiplier, timeRangeMultiplier.Value + 1, 200, Easing.OutQuint);
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        protected override DrawableRuleset<CatchHitObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods) =>
            new DrawableCatchEditorRuleset(ruleset, beatmap, mods)
            {
                TimeRangeMultiplier = { BindTarget = timeRangeMultiplier, }
            };

        protected override IReadOnlyList<HitObjectCompositionTool> CompositionTools => new HitObjectCompositionTool[]
        {
            new FruitCompositionTool(),
            new JuiceStreamCompositionTool(),
            new BananaShowerCompositionTool()
        };

        public override SnapResult FindSnappedPositionAndTime(Vector2 screenSpacePosition, SnapType snapType = SnapType.All)
        {
            var result = base.FindSnappedPositionAndTime(screenSpacePosition, snapType);

            result.ScreenSpacePosition.X = screenSpacePosition.X;

            if (snapType.HasFlagFast(SnapType.RelativeGrids))
            {
                if (distanceSnapGrid.IsPresent && distanceSnapGrid.GetSnappedPosition(result.ScreenSpacePosition) is SnapResult snapResult &&
                    Vector2.Distance(snapResult.ScreenSpacePosition, result.ScreenSpacePosition) < distance_snap_radius)
                {
                    result = snapResult;
                }
            }

            return result;
        }

        protected override ComposeBlueprintContainer CreateBlueprintContainer() => new CatchBlueprintContainer(this);

        protected override BeatSnapGrid CreateBeatSnapGrid() => new CatchBeatSnapGrid();

        private PalpableCatchHitObject? getLastSnappableHitObject(double time)
        {
            var hitObject = EditorBeatmap.HitObjects.OfType<CatchHitObject>().LastOrDefault(h => h.GetEndTime() < time && !(h is BananaShower));

            switch (hitObject)
            {
                case Fruit fruit:
                    return fruit;

                case JuiceStream juiceStream:
                    return juiceStream.NestedHitObjects.OfType<PalpableCatchHitObject>().LastOrDefault(h => !(h is TinyDroplet));

                default:
                    return null;
            }
        }

        private PalpableCatchHitObject? getDistanceSnapGridSourceHitObject()
        {
            switch (BlueprintContainer.CurrentTool)
            {
                case SelectTool:
                    if (EditorBeatmap.SelectedHitObjects.Count == 0)
                        return null;

                    double minTime = EditorBeatmap.SelectedHitObjects.Min(hitObject => hitObject.StartTime);
                    return getLastSnappableHitObject(minTime);

                case FruitCompositionTool:
                case JuiceStreamCompositionTool:
                    if (!CursorInPlacementArea)
                        return null;

                    if (EditorBeatmap.PlacementObject.Value is JuiceStream)
                    {
                        // Juice stream path is not subject to snapping.
                        if (BlueprintContainer.CurrentPlacement.PlacementActive is PlacementBlueprint.PlacementState.Active)
                            return null;
                    }

                    double timeAtCursor = ((CatchPlayfield)Playfield).TimeAtScreenSpacePosition(InputManager.CurrentState.Mouse.Position);
                    return getLastSnappableHitObject(timeAtCursor);

                default:
                    return null;
            }
        }

        private void updateDistanceSnapGrid()
        {
            if (DistanceSnapProvider.DistanceSnapToggle.Value != TernaryState.True)
            {
                distanceSnapGrid.Hide();
                return;
            }

            var sourceHitObject = getDistanceSnapGridSourceHitObject();

            if (sourceHitObject == null)
            {
                distanceSnapGrid.Hide();
                return;
            }

            distanceSnapGrid.Show();
            distanceSnapGrid.StartTime = sourceHitObject.GetEndTime();
            distanceSnapGrid.StartX = sourceHitObject.EffectiveX;
        }
    }
}
