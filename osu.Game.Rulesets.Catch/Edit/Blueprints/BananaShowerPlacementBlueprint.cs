// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Events;
using osu.Game.Rulesets.Catch.Edit.Blueprints.Components;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Edit;
using osuTK.Input;

namespace osu.Game.Rulesets.Catch.Edit.Blueprints
{
    public class BananaShowerPlacementBlueprint : CatchPlacementBlueprint<BananaShower>
    {
        private readonly TimeSpanOutline outline;

        public BananaShowerPlacementBlueprint()
        {
            InternalChild = outline = new TimeSpanOutline();
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

                    // If the duration is negative, swap the start and the end time to make the duration positive.
                    if (HitObject.Duration < 0)
                    {
                        HitObject.StartTime = HitObject.EndTime;
                        HitObject.Duration = -HitObject.Duration;
                    }

                    EndPlacement(HitObject.Duration > 0);
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
                    HitObject.StartTime = time;
                    break;

                case PlacementState.Active:
                    HitObject.EndTime = time;
                    break;
            }
        }
    }
}
