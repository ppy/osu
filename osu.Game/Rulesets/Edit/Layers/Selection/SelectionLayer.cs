// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

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

        private SelectionDragger selectionDragger;
        private CaptureBox captureBox;
        private HitObjectCapturer capturer;

        private readonly List<DrawableHitObject> capturedHitObjects = new List<DrawableHitObject>();

        [BackgroundDependencyLoader]
        private void load()
        {
            capturer = new HitObjectCapturer(playfield.HitObjects.Objects);
            capturer.HitObjectCaptured += h => capturedHitObjects.Add(h);
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            capturedHitObjects.Clear();
            captureBox?.Hide();
            return true;
        }

        protected override bool OnDragStart(InputState state)
        {
            AddInternal(selectionDragger = new SelectionDragger());
            return true;
        }

        protected override bool OnDrag(InputState state)
        {
            var dragPosition = state.Mouse.NativeState.Position;
            var dragStartPosition = state.Mouse.NativeState.PositionMouseDown ?? dragPosition;

            var screenSpaceDragQuad = new Quad(dragStartPosition.X, dragStartPosition.Y, dragPosition.X - dragStartPosition.X, dragPosition.Y - dragStartPosition.Y);

            selectionDragger.SetDragRectangle(screenSpaceDragQuad.AABBFloat);
            capturer.CaptureQuad(screenSpaceDragQuad);

            return true;
        }

        protected override bool OnDragEnd(InputState state)
        {
            selectionDragger.Hide();
            finishCapture();

            return true;
        }

        protected override bool OnClick(InputState state)
        {
            if (capturer.CapturePoint(state.Mouse.NativeState.Position))
                finishCapture();

            return true;
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
