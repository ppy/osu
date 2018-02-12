// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using OpenTK;

namespace osu.Game.Rulesets.Edit.Layers.Selection
{
    public class SelectionLayer : CompositeDrawable
    {
        private readonly Playfield playfield;

        public SelectionLayer(Playfield playfield)
        {
            this.playfield = playfield;

            RelativeSizeAxes = Axes.Both;
        }

        private SelectionBox selectionBox;
        private CaptureBox captureBox;

        private readonly List<DrawableHitObject> capturedHitObjects = new List<DrawableHitObject>();

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            capturedHitObjects.Clear();
            captureBox?.Hide();
            return true;
        }

        protected override bool OnDragStart(InputState state)
        {
            AddInternal(selectionBox = new SelectionBox());
            return true;
        }

        protected override bool OnDrag(InputState state)
        {
            var dragPosition = state.Mouse.NativeState.Position;
            var dragStartPosition = state.Mouse.NativeState.PositionMouseDown ?? dragPosition;

            var screenSpaceDragQuad = new Quad(dragStartPosition.X, dragStartPosition.Y, dragPosition.X - dragStartPosition.X, dragPosition.Y - dragStartPosition.Y);

            selectionBox.SetDragRectangle(screenSpaceDragQuad.AABBFloat);
            captureQuad(screenSpaceDragQuad);

            return true;
        }

        protected override bool OnDragEnd(InputState state)
        {
            selectionBox.Hide();
            finishCapture();

            return true;
        }

        protected override bool OnClick(InputState state)
        {
            capturePoint(state.Mouse.NativeState.Position);
            finishCapture();

            return true;
        }

        /// <summary>
        /// Captures all hitobjects that are present within the area of a <see cref="Quad"/>.
        /// </summary>
        /// <param name="screenSpaceQuad">The capture <see cref="Quad"/>.</param>
        private void captureQuad(Quad screenSpaceQuad)
        {
            foreach (var obj in playfield.HitObjects.Objects.Where(h => h.IsAlive && h.IsPresent && screenSpaceQuad.Contains(h.SelectionPoint)))
                capturedHitObjects.Add(obj);
        }

        /// <summary>
        /// Captures the top-most hitobject that is present under a specific point.
        /// </summary>
        /// <param name="screenSpacePoint">The <see cref="Vector2"/> to capture at.</param>
        private void capturePoint(Vector2 screenSpacePoint)
        {
            var captured = playfield.HitObjects.Objects.Reverse().Where(h => h.IsAlive && h.IsPresent).FirstOrDefault(h => h.ReceiveMouseInputAt(screenSpacePoint));
            if (captured == null)
                return;

            capturedHitObjects.Add(captured);
        }

        private void finishCapture()
        {
            if (capturedHitObjects.Count == 0)
                return;

            // Due to https://github.com/ppy/osu-framework/issues/1382, we may get here through both
            // OnDragEnd and OnClick methods within a single frame, OnMouseDown doesn't help us here
            captureBox?.Hide();
            AddInternal(captureBox = new CaptureBox(this, capturedHitObjects.ToList()));
        }
    }
}
