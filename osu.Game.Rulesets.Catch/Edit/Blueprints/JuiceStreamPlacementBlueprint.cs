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

            inputManager = GetContainingInputManager();

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

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            switch (PlacementActive)
            {
                case PlacementState.Waiting:
                    if (!(result.Time is double snappedTime)) return;

                    HitObject.OriginalX = ToLocalSpace(result.ScreenSpacePosition).X;
                    HitObject.StartTime = snappedTime;
                    break;

                case PlacementState.Active:
                    Vector2 unsnappedPosition = inputManager.CurrentState.Mouse.Position;
                    editablePath.MoveLastVertex(unsnappedPosition);
                    break;

                default:
                    return;
            }

            // Make sure the up-to-date position is used for outlines.
            Vector2 startPosition = CatchHitObjectUtils.GetStartPosition(HitObjectContainer, HitObject);
            editablePath.Position = nestedOutlineContainer.Position = scrollingPath.Position = startPosition;

            updateHitObjectFromPath();
        }

        private void updateHitObjectFromPath()
        {
            if (lastEditablePathId == editablePath.PathId)
                return;

            editablePath.UpdateHitObjectFromPath(HitObject);
            ApplyDefaultsToHitObject();

            scrollingPath.UpdatePathFrom(HitObjectContainer, HitObject);
            nestedOutlineContainer.UpdateNestedObjectsFrom(HitObjectContainer, HitObject);

            lastEditablePathId = editablePath.PathId;
        }

        private double positionToTime(float relativeYPosition)
        {
            double time = HitObjectContainer.TimeAtPosition(relativeYPosition, HitObject.StartTime);
            return time - HitObject.StartTime;
        }
    }
}
