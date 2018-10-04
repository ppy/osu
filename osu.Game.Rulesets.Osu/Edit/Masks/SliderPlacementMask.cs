// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Edit.Masks
{
    public partial class SliderPlacementMask : PlacementMask
    {
        public new Slider HitObject => (Slider)base.HitObject;

        private readonly CirclePlacementMask headMask;
        private readonly CirclePlacementMask tailMask;

        private readonly List<Vector2> controlPoints = new List<Vector2>();

        private PlacementState state = PlacementState.Head;

        public SliderPlacementMask()
            : base(new Slider())
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                headMask = new CirclePlacementMask(),
                tailMask = new CirclePlacementMask(),
            };

            setState(PlacementState.Head);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            switch (state)
            {
                case PlacementState.Head:
                    headMask.Position = e.MousePosition;
                    return true;
                case PlacementState.Tail:
                    tailMask.Position = ToLocalSpace(e.ScreenSpaceMousePosition);
                    return true;
            }

            return false;
        }

        protected override bool OnClick(ClickEvent e)
        {
            switch (state)
            {
                case PlacementState.Head:
                    setState(PlacementState.Tail);
                    controlPoints.Add(Vector2.Zero);
                    break;
                case PlacementState.Tail:
                    controlPoints.Add(tailMask.Position - headMask.Position);
                    HitObject.Position = headMask.Position;
                    HitObject.ControlPoints = controlPoints.ToList();
                    HitObject.CurveType = CurveType.Linear;
                    HitObject.Distance = Vector2.Distance(controlPoints.First(), controlPoints.Last());
                    Finish();
                    break;
            }

            return base.OnClick(e);
        }

        private void setState(PlacementState newState)
        {
            switch (newState)
            {
                case PlacementState.Head:
                    tailMask.Alpha = 0;
                    break;
                case PlacementState.Tail:
                    tailMask.Alpha = 1;
                    break;
            }

            state = newState;
        }

        private enum PlacementState
        {
            Head,
            Body,
            Tail
        }
    }
}
