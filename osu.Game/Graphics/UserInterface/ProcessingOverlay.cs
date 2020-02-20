// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// An overlay that will show a loading overlay and completely block input to an area.
    /// Also optionally dims target elements.
    /// Useful for disabling all elements in a form and showing we are waiting on a response, for instance.
    /// </summary>
    public class ProcessingOverlay : VisibilityContainer
    {
        private readonly Drawable dimTarget;

        private Container loadingBox;

        private const float transition_duration = 600;

        public ProcessingOverlay(Drawable dimTarget = null)
        {
            this.dimTarget = dimTarget;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                loadingBox = new Container
                {
                    Size = new Vector2(80),
                    Scale = new Vector2(0.8f),
                    Masking = true,
                    CornerRadius = 15,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4.Black,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new LoadingAnimation { State = { Value = Visibility.Visible } }
                    }
                },
            };
        }

        protected override bool Handle(UIEvent e) => true;

        protected override void PopIn()
        {
            this.FadeIn(transition_duration, Easing.OutQuint);
            loadingBox.ScaleTo(1, transition_duration, Easing.OutElastic);

            dimTarget?.FadeColour(OsuColour.Gray(0.5f), transition_duration, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.FadeOut(transition_duration, Easing.OutQuint);
            loadingBox.ScaleTo(0.8f, transition_duration / 2, Easing.In);

            dimTarget?.FadeColour(Color4.White, transition_duration, Easing.OutQuint);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (State.Value == Visibility.Visible)
            {
                // ensure we don't leave the targetin a bad state.
                dimTarget?.FadeColour(Color4.White, transition_duration);
            }
        }
    }
}
