// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Events;
using osu.Game.Rulesets.Catch.Edit.Blueprints.Components;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Catch.Edit.Blueprints
{
    public partial class FruitPlacementBlueprint : CatchPlacementBlueprint<Fruit>
    {
        private readonly FruitOutline outline;

        public FruitPlacementBlueprint()
        {
            InternalChild = outline = new FruitOutline();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            BeginPlacement();
        }

        protected override void Update()
        {
            base.Update();

            outline.Position = CatchHitObjectUtils.GetStartPosition(HitObjectContainer, HitObject);
            outline.UpdateFrom(HitObject);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button != MouseButton.Left) return base.OnMouseDown(e);

            EndPlacement(true);
            return true;
        }

        public override SnapResult UpdateTimeAndPosition(Vector2 screenSpacePosition, double fallbackTime)
        {
            var gridSnapResult = Composer?.FindSnappedPositionAndTime(screenSpacePosition) ?? new SnapResult(screenSpacePosition, fallbackTime);
            gridSnapResult.ScreenSpacePosition.X = screenSpacePosition.X;
            var distanceSnapResult = Composer?.TryDistanceSnap(gridSnapResult.ScreenSpacePosition);

            var result = distanceSnapResult != null && Vector2.Distance(gridSnapResult.ScreenSpacePosition, distanceSnapResult.ScreenSpacePosition) < CatchHitObjectComposer.DISTANCE_SNAP_RADIUS
                ? distanceSnapResult
                : gridSnapResult;

            base.UpdateTimeAndPosition(result.ScreenSpacePosition, result.Time ?? fallbackTime);

            HitObject.X = ToLocalSpace(result.ScreenSpacePosition).X;
            return result;
        }
    }
}
