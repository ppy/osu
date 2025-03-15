// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Rulesets.Catch.Edit.Blueprints.Components;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Catch.Edit.Blueprints
{
    public partial class JuiceStreamPlacementBlueprint : CatchPlacementBlueprint<JuiceStream>
    {
        private readonly ScrollingPath scrollingPath;

        private readonly NestedOutlineContainer nestedOutlineContainer;

        private readonly PlacementEditablePath editablePath;

        private int lastEditablePathId = -1;

        private InputManager inputManager = null!;

        protected override bool IsValidForPlacement => Precision.DefinitelyBigger(HitObject.Duration, 0);

        public JuiceStreamPlacementBlueprint()
        {
            InternalChildren = new Drawable[]
            {
                scrollingPath = new ScrollingPath(),
                nestedOutlineContainer = new NestedOutlineContainer(),
                editablePath = new PlacementEditablePath(positionToTime)
            };
        }

        protected override void Update()
        {
            base.Update();

            if (PlacementActive == PlacementState.Active)
                editablePath.UpdateFrom(HitObjectContainer, HitObject);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager()!;

            BeginPlacement();
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            switch (PlacementActive)
            {
                case PlacementState.Waiting:
                    if (e.Button != MouseButton.Left) break;

                    editablePath.AddNewVertex();
                    BeginPlacement(true);
                    return true;

                case PlacementState.Active:
                    switch (e.Button)
                    {
                        case MouseButton.Left:
                            editablePath.AddNewVertex();
                            return true;

                        case MouseButton.Right:
                            EndPlacement(true);
                            return true;
                    }

                    break;
            }

            return base.OnMouseDown(e);
        }

        public override SnapResult UpdateTimeAndPosition(Vector2 screenSpacePosition, double fallbackTime)
        {
            var gridSnapResult = Composer?.FindSnappedPositionAndTime(screenSpacePosition) ?? new SnapResult(screenSpacePosition, fallbackTime);
            gridSnapResult.ScreenSpacePosition.X = screenSpacePosition.X;
            var distanceSnapResult = Composer?.TryDistanceSnap(gridSnapResult.ScreenSpacePosition);

            var result = distanceSnapResult != null && Vector2.Distance(gridSnapResult.ScreenSpacePosition, distanceSnapResult.ScreenSpacePosition) < CatchHitObjectComposer.DISTANCE_SNAP_RADIUS
                ? distanceSnapResult
                : gridSnapResult;

            switch (PlacementActive)
            {
                case PlacementState.Waiting:
                    HitObject.OriginalX = ToLocalSpace(result.ScreenSpacePosition).X;
                    if (result.Time is double snappedTime)
                        HitObject.StartTime = snappedTime;
                    break;

                case PlacementState.Active:
                    Vector2 unsnappedPosition = inputManager.CurrentState.Mouse.Position;
                    editablePath.MoveLastVertex(unsnappedPosition);
                    break;

                default:
                    return result;
            }

            // Make sure the up-to-date position is used for outlines.
            Vector2 startPosition = CatchHitObjectUtils.GetStartPosition(HitObjectContainer, HitObject);
            editablePath.Position = nestedOutlineContainer.Position = scrollingPath.Position = startPosition;

            if (lastEditablePathId != editablePath.PathId)
                editablePath.UpdateHitObjectFromPath(HitObject);
            lastEditablePathId = editablePath.PathId;

            ApplyDefaultsToHitObject();
            scrollingPath.UpdatePathFrom(HitObjectContainer, HitObject);
            nestedOutlineContainer.UpdateNestedObjectsFrom(HitObjectContainer, HitObject);
            return result;
        }

        private double positionToTime(float relativeYPosition)
        {
            double time = HitObjectContainer.TimeAtPosition(relativeYPosition, HitObject.StartTime);
            return time - HitObject.StartTime;
        }
    }
}
