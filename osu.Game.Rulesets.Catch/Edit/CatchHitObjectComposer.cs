// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
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
    public class CatchHitObjectComposer : HitObjectComposer<CatchHitObject>
    {
        private const float distance_snap_radius = 50;

        private CatchDistanceSnapGrid distanceSnapGrid;

        private readonly Bindable<TernaryState> distanceSnapToggle = new Bindable<TernaryState>();

        private InputManager inputManager;

        public CatchHitObjectComposer(CatchRuleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            LayerBelowRuleset.Add(new PlayfieldBorder
            {
                RelativeSizeAxes = Axes.Both,
                PlayfieldBorderStyle = { Value = PlayfieldBorderStyle.Corners }
            });

            LayerBelowRuleset.Add(distanceSnapGrid = new CatchDistanceSnapGrid(new[]
            {
                0.0,
                Catcher.BASE_DASH_SPEED, -Catcher.BASE_DASH_SPEED,
                Catcher.BASE_WALK_SPEED, -Catcher.BASE_WALK_SPEED,
            }));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();
        }

        protected override void Update()
        {
            base.Update();

            updateDistanceSnapGrid();
        }

        protected override DrawableRuleset<CatchHitObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null) =>
            new DrawableCatchEditorRuleset(ruleset, beatmap, mods);

        protected override IReadOnlyList<HitObjectCompositionTool> CompositionTools => new HitObjectCompositionTool[]
        {
            new FruitCompositionTool(),
            new JuiceStreamCompositionTool(),
            new BananaShowerCompositionTool()
        };

        protected override IEnumerable<TernaryButton> CreateTernaryButtons() => base.CreateTernaryButtons().Concat(new[]
        {
            new TernaryButton(distanceSnapToggle, "Distance Snap", () => new SpriteIcon { Icon = FontAwesome.Solid.Ruler })
        });

        public override SnapResult SnapScreenSpacePositionToValidTime(Vector2 screenSpacePosition)
        {
            var result = base.SnapScreenSpacePositionToValidTime(screenSpacePosition);
            result.ScreenSpacePosition.X = screenSpacePosition.X;

            if (distanceSnapGrid.IsPresent && distanceSnapGrid.GetSnappedPosition(result.ScreenSpacePosition) is SnapResult snapResult &&
                Vector2.Distance(snapResult.ScreenSpacePosition, result.ScreenSpacePosition) < distance_snap_radius)
            {
                result = snapResult;
            }

            return result;
        }

        protected override ComposeBlueprintContainer CreateBlueprintContainer() => new CatchBlueprintContainer(this);

        [CanBeNull]
        private PalpableCatchHitObject getLastSnappableHitObject(double time)
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

        [CanBeNull]
        private PalpableCatchHitObject getDistanceSnapGridSourceHitObject()
        {
            switch (BlueprintContainer.CurrentTool)
            {
                case SelectTool _:
                    if (EditorBeatmap.SelectedHitObjects.Count == 0)
                        return null;

                    double minTime = EditorBeatmap.SelectedHitObjects.Min(hitObject => hitObject.StartTime);
                    return getLastSnappableHitObject(minTime);

                case FruitCompositionTool _:
                case JuiceStreamCompositionTool _:
                    if (!CursorInPlacementArea)
                        return null;

                    if (EditorBeatmap.PlacementObject.Value is JuiceStream)
                    {
                        // Juice stream path is not subject to snapping.
                        return null;
                    }

                    double timeAtCursor = ((CatchPlayfield)Playfield).TimeAtScreenSpacePosition(inputManager.CurrentState.Mouse.Position);
                    return getLastSnappableHitObject(timeAtCursor);

                default:
                    return null;
            }
        }

        private void updateDistanceSnapGrid()
        {
            if (distanceSnapToggle.Value != TernaryState.True)
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
