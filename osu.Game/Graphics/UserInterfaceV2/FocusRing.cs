// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class FocusRing : VisibilityContainer
    {
        public float Thickness { get; init; } = 3;

        public double TransitionDuration = 100;

        private readonly Container content;

        public FocusRing()
        {
            Alpha = 0.5f;

            Child = content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };
        }

        protected override void PopIn()
        {
            this.TransformTo(nameof(Padding), new MarginPadding(-Thickness), TransitionDuration, Easing.Out);

            content
                .FadeIn(TransitionDuration)
                .TransformTo(nameof(CornerRadius), CornerRadius + Thickness, TransitionDuration, Easing.Out);
        }

        protected override void PopOut()
        {
            this.TransformTo(nameof(Padding), new MarginPadding(0), TransitionDuration, Easing.Out);

            content
                .FadeOut(TransitionDuration)
                .TransformTo(nameof(CornerRadius), CornerRadius, TransitionDuration, Easing.Out);
        }
    }
}
