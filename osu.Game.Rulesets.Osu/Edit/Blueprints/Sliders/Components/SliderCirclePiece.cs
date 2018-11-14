// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public class SliderCirclePiece : HitCirclePiece
    {
        private readonly IBindable<SliderPath> pathBindable = new Bindable<SliderPath>();

        private readonly Slider slider;
        private readonly SliderPosition position;

        public SliderCirclePiece(Slider slider, SliderPosition position)
            : base(slider.HeadCircle)
        {
            this.slider = slider;
            this.position = position;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            pathBindable.BindTo(slider.PathBindable);
            pathBindable.BindValueChanged(_ => UpdatePosition(), true);
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
