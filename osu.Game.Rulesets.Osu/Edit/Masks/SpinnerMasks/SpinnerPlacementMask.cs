// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Edit.Masks.SpinnerMasks.Components;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Masks.SpinnerMasks
{
    public class SpinnerPlacementMask : PlacementMask
    {
        public new Spinner HitObject => (Spinner)base.HitObject;

        private readonly SpinnerPiece piece;

        private bool isPlacingEnd;

        public SpinnerPlacementMask()
            : base(new Spinner())
        {
            InternalChild = piece = new SpinnerPiece(HitObject) { Alpha = 0.5f };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Fixes a 1-frame position discrpancy due to the first mouse move event happening in the next frame
            HitObject.Position = GetContainingInputManager().CurrentState.Mouse.Position;
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (isPlacingEnd)
            {
                HitObject.EndTime = EditorClock.CurrentTime;
                EndPlacement();
            }
            else
            {
                HitObject.StartTime = EditorClock.CurrentTime;

                isPlacingEnd = true;
                piece.FadeTo(1f, 150, Easing.OutQuint);
            }

            return true;
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (!isPlacingEnd)
                HitObject.Position = e.MousePosition;
            return true;
        }
    }
}
