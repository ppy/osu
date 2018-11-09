// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints
{
    /// <summary>
    /// A piece of a blueprint which responds to changes in the state of a <see cref="Slider"/>.
    /// </summary>
    public abstract class SliderPiece : HitObjectPiece
    {
        protected readonly IBindable<Vector2[]> ControlPointsBindable = new Bindable<Vector2[]>();

        private readonly Slider slider;

        protected SliderPiece(Slider slider)
            : base(slider)
        {
            this.slider = slider;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ControlPointsBindable.BindTo(slider.ControlPointsBindable);
        }
    }
}
