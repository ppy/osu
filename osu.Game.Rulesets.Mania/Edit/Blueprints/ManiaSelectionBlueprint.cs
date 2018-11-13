// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Timing;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using OpenTK;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public class ManiaSelectionBlueprint : SelectionBlueprint
    {
        protected IClock EditorClock { get; private set; }

        [Resolved]
        private IScrollingInfo scrollingInfo { get; set; }

        public ManiaSelectionBlueprint(DrawableHitObject hitObject)
            : base(hitObject)
        {
            RelativeSizeAxes = Axes.None;
        }

        [BackgroundDependencyLoader]
        private void load(IAdjustableClock clock)
        {
            EditorClock = clock;
        }

        protected override void Update()
        {
            base.Update();

            Position = Parent.ToLocalSpace(HitObject.ToScreenSpace(Vector2.Zero));
        }

        public override void AdjustPosition(DragEvent dragEvent)
        {
            var objectParent = HitObject.Parent;

            // Using the hitobject position is required since AdjustPosition can be invoked multiple times per frame
            // without the position having been updated by the parenting ScrollingHitObjectContainer
            HitObject.Y += dragEvent.Delta.Y;

            float targetPosition;

            // If we're scrolling downwards, a position of 0 is actually further away from the hit target
            // so we need to flip the vertical coordinate in the hitobject container's space
            if (scrollingInfo.Direction.Value == ScrollingDirection.Down)
                targetPosition = -HitObject.Position.Y;
            else
                targetPosition = HitObject.Position.Y;

            HitObject.HitObject.StartTime = scrollingInfo.Algorithm.TimeAt(targetPosition,
                EditorClock.CurrentTime,
                scrollingInfo.TimeRange.Value,
                objectParent.DrawHeight);
        }
    }
}
