// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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

        private HitObjectSelectionBox selectionBox;
        private HitObjectCapturer capturer;

        [BackgroundDependencyLoader]
        private void load()
        {
            capturer = new HitObjectCapturer(playfield.HitObjects.Objects);
            capturer.HitObjectCaptured += hitObjectCaptured;
        }

        private void hitObjectCaptured(DrawableHitObject hitObject) => selectionBox.AddCaptured(hitObject);

        protected override bool OnDragStart(InputState state)
        {
            // Hide the previous drag box - we won't be working with it any longer
            selectionBox?.Hide();
            selectionBox?.Expire();

            AddInternal(selectionBox = new HitObjectSelectionBox());

            return true;
        }

        protected override bool OnDrag(InputState state)
        {
            var dragPosition = state.Mouse.NativeState.Position;
            var dragStartPosition = state.Mouse.NativeState.PositionMouseDown ?? dragPosition;

            var screenSpaceDragQuad = new Quad(dragStartPosition.X, dragStartPosition.Y, dragPosition.X - dragStartPosition.X, dragPosition.Y - dragStartPosition.Y);

            selectionBox.SetDragRectangle(screenSpaceDragQuad.AABBFloat);
            capturer.CaptureQuad(screenSpaceDragQuad);

            return true;
        }

        protected override bool OnDragEnd(InputState state)
        {
            // Due to https://github.com/ppy/osu-framework/issues/1382, we may get here after OnClick has set the selectionBox to null
            // In the case that the user dragged within the click distance out of an object
            if (selectionBox == null)
                return true;

            selectionBox.FinishCapture();

            // If there are no hitobjects, remove the selection box
            if (!selectionBox.HasCaptured)
            {
                selectionBox.Expire();
                selectionBox = null;
            }

            return true;
        }

        protected override bool OnClick(InputState state)
        {
            // We could be coming here without a previous selection box
            if (selectionBox == null)
                AddInternal(selectionBox = new HitObjectSelectionBox { Position = ToLocalSpace(state.Mouse.NativeState.Position), Alpha = 0 });

            // If we're coming here with a previous selection, unselect those hitobjects
            selectionBox.ClearCaptured();
            if (capturer.CapturePoint(state.Mouse.NativeState.Position))
            {
                selectionBox.Alpha = 1;
                selectionBox.FinishCapture(true);
            }
            else
            {
                selectionBox.Hide();
                selectionBox = null;
            }

            return true;
        }
    }
}
