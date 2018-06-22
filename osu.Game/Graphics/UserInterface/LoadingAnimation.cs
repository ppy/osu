// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public class LoadingAnimation : VisibilityContainer
    {
        private readonly SpriteIcon spinner;
        private readonly SpriteIcon spinnerShadow;

        private const float spin_duration = 600;
        private const float transition_duration = 200;

        public LoadingAnimation()
        {
            Size = new Vector2(20);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                spinnerShadow = new SpriteIcon
                {
                    RelativeSizeAxes = Axes.Both,
                    Position = new Vector2(1, 1),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Color4.Black,
                    Alpha = 0.4f,
                    Icon = FontAwesome.fa_circle_o_notch
                },
                spinner = new SpriteIcon
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.fa_circle_o_notch
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            spinner.Spin(spin_duration, RotationDirection.Clockwise);
            spinnerShadow.Spin(spin_duration, RotationDirection.Clockwise);
        }


        protected override void PopIn() => this.FadeIn(transition_duration, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(transition_duration, Easing.OutQuint);
    }
}
