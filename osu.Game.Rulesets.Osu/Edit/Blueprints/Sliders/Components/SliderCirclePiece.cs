// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
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

            slider.PathChanged += _ => UpdatePosition();
        }

        protected override void UpdatePosition()
        {
            switch (position)
            {
                case SliderPosition.Start:
                    Position = slider.StackedPosition + slider.Path.PositionAt(0);
                    break;
                case SliderPosition.End:
                    Position = slider.StackedPosition + slider.Path.PositionAt(1);
                    break;
            }
        }
    }
}
