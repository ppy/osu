// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;

namespace osu.Game.Rulesets.Edit.Layers.Selection
{
    /// <summary>
    /// A box which encapsulates captured <see cref="DrawableHitObject"/>s.
    /// </summary>
    public abstract class CaptureBox : VisibilityContainer
    {
        /// <summary>
        /// Top-left corner of the rectangle that encloses the <see cref="DrawableHitObject"/>s.
        /// </summary>
        protected Vector2 FinalPosition { get; private set; }

        /// <summary>
        /// Size of the rectangle that encloses the <see cref="DrawableHitObject"/>s.
        /// </summary>
        protected Vector2 FinalSize { get; private set; }

        private readonly IDrawable captureArea;
        private readonly IReadOnlyList<DrawableHitObject> capturedObjects;

        protected CaptureBox(IDrawable captureArea, IReadOnlyList<DrawableHitObject> capturedObjects)
        {
            this.captureArea = captureArea;
            this.capturedObjects = capturedObjects;

            Masking = true;
            BorderThickness = 3;

            InternalChild = new Box
            {
                RelativeSizeAxes = Axes.Both,
                AlwaysPresent = true,
                Alpha = 0
            };

            State = Visibility.Visible;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BorderColour = colours.Yellow;

            // Move the rectangle to cover the hitobjects
            var topLeft = new Vector2(float.MaxValue, float.MaxValue);
            var bottomRight = new Vector2(float.MinValue, float.MinValue);

            foreach (var obj in capturedObjects)
            {
                topLeft = Vector2.ComponentMin(topLeft, captureArea.ToLocalSpace(obj.SelectionQuad.TopLeft));
                bottomRight = Vector2.ComponentMax(bottomRight, captureArea.ToLocalSpace(obj.SelectionQuad.BottomRight));
            }

            topLeft -= new Vector2(5);
            bottomRight += new Vector2(5);

            FinalSize = bottomRight - topLeft;
            FinalPosition = topLeft;
        }

        protected override void PopIn() => this.MoveTo(FinalPosition).ResizeTo(FinalSize).FadeIn();
        protected override void PopOut() => this.FadeOut();
    }

    /// <summary>
    /// A <see cref="CaptureBox"/> which fully encloses the <see cref="DrawableHitObject"/>s from the start.
    /// </summary>
    public class InstantCaptureBox : CaptureBox
    {
        public InstantCaptureBox(IDrawable captureArea, IReadOnlyList<DrawableHitObject> capturedObjects)
            : base(captureArea, capturedObjects)
        {
            Origin = Anchor.Centre;
        }

        protected override void PopIn()
            => this.MoveTo(FinalPosition + FinalSize / 2f).ResizeTo(FinalSize).ScaleTo(1.1f)
                   .Then()
                   .ScaleTo(1f, 300, Easing.OutQuint).FadeIn(300, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(300, Easing.OutQuint);
    }

    /// <summary>
    /// A <see cref="CaptureBox"/> which moves from an initial position + size to enclose <see cref="DrawableHitObject"/>s.
    /// </summary>
    public class DragCaptureBox : CaptureBox
    {
        public DragCaptureBox(IDrawable captureArea, IReadOnlyList<DrawableHitObject> capturedObjects, Vector2 initialPosition, Vector2 initialSize)
            : base(captureArea, capturedObjects)
        {
            Position = initialPosition;
            Size = initialSize;
        }

        protected override void PopIn()
            => this.MoveTo(FinalPosition, 300, Easing.OutQuint).ResizeTo(FinalSize, 300, Easing.OutQuint).FadeIn(300, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(300, Easing.OutQuint);
    }
}
