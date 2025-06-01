// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Catch.Edit
{
    [Cached]
    public partial class CatchHitObjectComposer : ScrollingHitObjectComposer<CatchHitObject>, IKeyBindingHandler<GlobalAction>
    {
        public const float DISTANCE_SNAP_RADIUS = 50;

        private CatchDistanceSnapGrid distanceSnapGrid = null!;

        private readonly BindableDouble timeRangeMultiplier = new BindableDouble(1)
        {
            MinValue = 1,
            MaxValue = 10,
        };

        [Cached(typeof(IDistanceSnapProvider))]
        protected readonly CatchDistanceSnapProvider DistanceSnapProvider = new CatchDistanceSnapProvider();

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
        }

        protected override Drawable CreateHitObjectInspector() => new CatchHitObjectInspector(DistanceSnapProvider);

        protected override IEnumerable<Drawable> CreateTernaryButtons()
            => base.CreateTernaryButtons()
                   .Concat(DistanceSnapProvider.CreateTernaryButtons());

        protected override DrawableRuleset<CatchHitObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods) =>
            new DrawableCatchEditorRuleset(ruleset, beatmap, mods)
            {
                TimeRangeMultiplier = { BindTarget = timeRangeMultiplier, }
            };

        protected override ComposeBlueprintContainer CreateBlueprintContainer() => new CatchBlueprintContainer(this);

        protected override BeatSnapGrid CreateBeatSnapGrid() => new CatchBeatSnapGrid();

        protected override IReadOnlyList<CompositionTool> CompositionTools => new CompositionTool[]
        {
            new FruitCompositionTool(),
            new JuiceStreamCompositionTool(),
            new BananaShowerCompositionTool()
        };

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

        private void handleToggleViaKey(KeyboardEvent key)
        {
            DistanceSnapProvider.HandleToggleViaKey(key);
        }

        public SnapResult? TryDistanceSnap(Vector2 screenSpacePosition)
        {
            if (distanceSnapGrid.IsPresent && distanceSnapGrid.GetSnappedPosition(screenSpacePosition) is SnapResult snapResult)
                return snapResult;

            return null;
        }

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

        protected override void Update()
        {
            base.Update();

            updateDistanceSnapGrid();
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
