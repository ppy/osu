// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints
{
    /// <summary>
    /// A piece of a blueprint which responds to changes in the state of a <see cref="Slider"/>.
    /// </summary>
    public abstract class SliderPiece : HitObjectPiece
    {
        protected readonly IBindable<SliderPath> PathBindable = new Bindable<SliderPath>();

        private readonly Slider slider;

        protected SliderPiece(Slider slider)
            : base(slider)
        {
            this.slider = slider;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            PathBindable.BindTo(slider.PathBindable);
        }
    }
}
