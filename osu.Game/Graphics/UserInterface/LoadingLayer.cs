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
    /// An overlay that will show a loading overlay and completely block input to an area.
    /// Also optionally dims target elements.
    /// Useful for disabling all elements in a form and showing we are waiting on a response, for instance.
    /// </summary>
    public class LoadingLayer : LoadingAnimation
    {
        private readonly Drawable dimTarget;

        public LoadingLayer(Drawable dimTarget = null, bool withBox = true)
            : base(withBox)
        {
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(1);

            this.dimTarget = dimTarget;

            MainContents.RelativeSizeAxes = Axes.None;
        }

        protected override bool Handle(UIEvent e)
        {
            switch (e)
            {
                // blocking scroll can cause weird behaviour when this layer is used within a ScrollContainer.
                case ScrollEvent _:
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
            MainContents.Size = new Vector2(Math.Min(DrawWidth, DrawHeight) * 0.25f);
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
