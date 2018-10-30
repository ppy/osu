// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Osu.Edit.Masks.HitCircleMasks.Components;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Masks.SliderMasks.Components
{
    public class SliderCirclePiece : HitCirclePiece
    {
        private readonly Slider slider;
        private readonly SliderPosition position;

        public SliderCirclePiece(Slider slider, SliderPosition position)
            : base(slider.HeadCircle)
        {
            this.slider = slider;
            this.position = position;
        }

        protected override void UpdatePosition()
        {
            switch (position)
            {
                case SliderPosition.Start:
                    Position = slider.StackedPosition + slider.Curve.PositionAt(0);
                    break;
                case SliderPosition.End:
                    Position = slider.StackedPosition + slider.Curve.PositionAt(1);
                    break;
            }
        }
    }
}
