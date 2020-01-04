// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// An overlay that will consume all available space and block input when required.
    /// Useful for disabling all elements in a form and showing we are waiting on a response, for instance.
    /// </summary>
    public class ProcessingOverlay : VisibilityContainer
    {
        private const float transition_duration = 200;

        public ProcessingOverlay()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.9f,
                },
                new LoadingAnimation { State = { Value = Visibility.Visible } }
            };
        }

        protected override bool Handle(UIEvent e)
        {
            return true;
        }

        protected override void PopIn()
        {
            this.FadeIn(transition_duration * 2, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.FadeOut(transition_duration, Easing.OutQuint);
        }
    }
}
