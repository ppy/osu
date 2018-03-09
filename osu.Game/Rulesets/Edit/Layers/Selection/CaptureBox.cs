// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;

namespace osu.Game.Rulesets.Edit.Layers.Selection
{
    /// <summary>
    /// A box which encloses <see cref="DrawableHitObject"/>s.
    /// </summary>
    public class CaptureBox : VisibilityContainer
    {
        /// <summary>
        /// Invoked when the captured <see cref="DrawableHitObject"/>s should be moved.
        /// </summary>
        public event Action<Vector2> MovementRequested;

        private readonly IDrawable captureArea;
        private readonly IReadOnlyList<DrawableHitObject> capturedObjects;

        public CaptureBox(IDrawable captureArea, IReadOnlyList<DrawableHitObject> capturedObjects)
        {
            this.captureArea = captureArea;
            this.capturedObjects = capturedObjects;

            Masking = true;
            BorderThickness = SelectionBox.BORDER_RADIUS;

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
        }

        protected override void Update()
        {
            base.Update();

            // Todo: We might need to optimise this

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

            Size = bottomRight - topLeft;
            Position = topLeft;
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => true;

        protected override bool OnDragStart(InputState state) => true;

        protected override bool OnDrag(InputState state)
        {
            MovementRequested?.Invoke(state.Mouse.Delta);
            return true;
        }

        protected override bool OnDragEnd(InputState state) => true;

        public override bool DisposeOnDeathRemoval => true;

        protected override void PopIn() => this.FadeIn();
        protected override void PopOut() => this.FadeOut();
    }
}
