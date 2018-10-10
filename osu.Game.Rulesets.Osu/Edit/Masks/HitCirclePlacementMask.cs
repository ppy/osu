// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Masks
{
    public class HitCirclePlacementMask : PlacementMask
    {
        public new HitCircle HitObject => (HitCircle)base.HitObject;

        public HitCirclePlacementMask()
            : base(new HitCircle())
        {
            Origin = Anchor.Centre;
            AutoSizeAxes = Axes.Both;

            InternalChild = new HitCircleMask(HitObject);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Fixes a 1-frame position discrpancy due to the first mouse move event happening in the next frame
            Position = GetContainingInputManager().CurrentState.Mouse.Position;
        }

        protected override bool OnClick(ClickEvent e)
        {
            HitObject.StartTime = EditorClock.CurrentTime;
            HitObject.Position = e.MousePosition;
            Finish();
            return true;
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            Position = e.MousePosition;
            return true;
        }
    }
}
