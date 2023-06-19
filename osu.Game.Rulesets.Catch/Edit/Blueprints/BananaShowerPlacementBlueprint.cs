// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Catch.Edit.Blueprints.Components;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Edit;
using osuTK.Input;

namespace osu.Game.Rulesets.Catch.Edit.Blueprints
{
    public partial class BananaShowerPlacementBlueprint : CatchPlacementBlueprint<BananaShower>
    {
        private readonly TimeSpanOutline outline;

        private double placementStartTime;
        private double placementEndTime;

        protected override bool IsValidForPlacement => HitObject.Duration > 0;

        public BananaShowerPlacementBlueprint()
        {
            InternalChild = outline = new TimeSpanOutline();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            BeginPlacement();
        }

        protected override void Update()
        {
            base.Update();

            outline.UpdateFrom(HitObjectContainer, HitObject);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            switch (PlacementActive)
            {
                case PlacementState.Waiting:
                    if (e.Button != MouseButton.Left) break;

                    BeginPlacement(true);
                    return true;

                case PlacementState.Active:
                    if (e.Button != MouseButton.Right) break;

                    EndPlacement(true);
                    return true;
            }

            return base.OnMouseDown(e);
        }

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            base.UpdateTimeAndPosition(result);

            if (!(result.Time is double time)) return;

            switch (PlacementActive)
            {
                case PlacementState.Waiting:
                    placementStartTime = placementEndTime = time;
                    break;

                case PlacementState.Active:
                    placementEndTime = time;
                    break;
            }

            HitObject.StartTime = Math.Min(placementStartTime, placementEndTime);
            HitObject.EndTime = Math.Max(placementStartTime, placementEndTime);
        }
    }
}
