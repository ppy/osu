// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
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
    public partial class LoadingLayer : LoadingSpinner
    {
        private readonly bool blockInput;

        [CanBeNull]
        protected Box BackgroundDimLayer { get; }

        /// <summary>
        /// Construct a new loading spinner.
        /// </summary>
        /// <param name="dimBackground">Whether the full background area should be dimmed while loading.</param>
        /// <param name="withBox">Whether the spinner should have a surrounding black box for visibility.</param>
        /// <param name="blockInput">Whether to block input of components behind the loading layer.</param>
        public LoadingLayer(bool dimBackground = false, bool withBox = true, bool blockInput = true)
            : base(withBox)
        {
            this.blockInput = blockInput;
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(1);

            MainContents.RelativeSizeAxes = Axes.None;

            if (dimBackground)
            {
                AddInternal(BackgroundDimLayer = new Box
                {
                    Depth = float.MaxValue,
                    Colour = Color4.Black,
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both,
                });
            }
        }

        public override bool HandleNonPositionalInput => false;

        protected override bool Handle(UIEvent e)
        {
            if (!blockInput)
                return false;

            switch (e)
            {
                // blocking scroll can cause weird behaviour when this layer is used within a ScrollContainer.
                case ScrollEvent:
                    return false;

                // blocking touch events causes the ISourcedFromTouch versions to not be fired, potentially impeding behaviour of drawables *above* the loading layer that may utilise these.
                // note that this will not work well if touch handling elements are beneath this loading layer (something to consider for the future).
                case TouchEvent:
                    return false;
            }

            return true;
        }

        protected override void PopIn()
        {
            BackgroundDimLayer?.FadeTo(0.5f, TRANSITION_DURATION * 2, Easing.OutQuint);
            base.PopIn();
        }

        protected override void PopOut()
        {
            BackgroundDimLayer?.FadeOut(TRANSITION_DURATION, Easing.OutQuint);
            base.PopOut();
        }

        protected override void Update()
        {
            base.Update();

            MainContents.Size = new Vector2(Math.Clamp(Math.Min(DrawWidth, DrawHeight) * 0.25f, 20, 100));
        }
    }
}
