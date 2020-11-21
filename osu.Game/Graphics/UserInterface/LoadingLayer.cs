// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A layer that will show a loading spinner and completely block input to an area.
    /// Also optionally dims target elements.
    /// Useful for disabling all elements in a form and showing we are waiting on a response, for instance.
    /// </summary>
    public class LoadingLayer : LoadingSpinner
    {
        private readonly Drawable dimTarget;

        /// <summary>
        /// Constuct a new loading spinner.
        /// </summary>
        /// <param name="dimTarget">An optional target to dim when displayed.</param>
        /// <param name="withBox">Whether the spinner should have a surrounding black box for visibility.</param>
        public LoadingLayer(Drawable dimTarget = null, bool withBox = true)
            : base(withBox)
        {
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(1);

            this.dimTarget = dimTarget;

            MainContents.RelativeSizeAxes = Axes.None;
        }

        public override bool HandleNonPositionalInput => false;

        protected override bool Handle(UIEvent e)
        {
            switch (e)
            {
                // blocking scroll can cause weird behaviour when this layer is used within a ScrollContainer.
                case ScrollEvent _:
                    return false;

                // blocking touch events causes the ISourcedFromTouch versions to not be fired, potentially impeding behaviour of drawables *above* the loading layer that may utilise these.
                // note that this will not work well if touch handling elements are beneath this loading layer (something to consider for the future).
                case TouchEvent _:
                    return false;
            }

            return true;
        }

        protected override void PopIn()
        {
            dimTarget?.FadeColour(OsuColour.Gray(0.5f), TRANSITION_DURATION, Easing.OutQuint);
            base.PopIn();
        }

        protected override void PopOut()
        {
            dimTarget?.FadeColour(Color4.White, TRANSITION_DURATION, Easing.OutQuint);
            base.PopOut();
        }

        protected override void Update()
        {
            base.Update();
            MainContents.Size = new Vector2(Math.Clamp(Math.Min(DrawWidth, DrawHeight) * 0.25f, 30, 100));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (State.Value == Visibility.Visible)
            {
                // ensure we don't leave the target in a bad state.
                dimTarget?.FadeColour(Color4.White, TRANSITION_DURATION, Easing.OutQuint);
            }
        }
    }
}
