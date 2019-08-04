// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Extensions.Color4Extensions;

namespace osu.Game.Graphics.UserInterface
{
    public class DimmedLoadingLayer : VisibilityContainer
    {
        private const float transition_duration = 250;

        private readonly LoadingAnimation loading;

        public DimmedLoadingLayer()
        {
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.5f),
                },
                loading = new LoadingAnimation(),
            };
        }

        protected override void PopIn()
        {
            this.FadeIn(transition_duration, Easing.OutQuint);
            loading.Show();
        }

        protected override void PopOut()
        {
            this.FadeOut(transition_duration, Easing.OutQuint);
            loading.Hide();
        }
    }
}
