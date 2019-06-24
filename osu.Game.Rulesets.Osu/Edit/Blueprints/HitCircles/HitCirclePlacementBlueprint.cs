// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles
{
    public class HitCirclePlacementBlueprint : PlacementBlueprint
    {
        public new HitCircle HitObject => (HitCircle)base.HitObject;

        public HitCirclePlacementBlueprint()
            : base(new HitCircle())
        {
            InternalChild = new HitCirclePiece(HitObject);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Fixes a 1-frame position discrepancy due to the first mouse move event happening in the next frame
            HitObject.Position = Parent?.ToLocalSpace(GetContainingInputManager().CurrentState.Mouse.Position) ?? Vector2.Zero;
        }

        protected override bool OnClick(ClickEvent e)
        {
            HitObject.StartTime = EditorClock.CurrentTime;
            EndPlacement();
            return true;
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            HitObject.Position = e.MousePosition;
            return true;
        }
    }
}
