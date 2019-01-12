// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Objects;

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

            // Fixes a 1-frame position discrpancy due to the first mouse move event happening in the next frame
            HitObject.Position = GetContainingInputManager().CurrentState.Mouse.Position;
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
