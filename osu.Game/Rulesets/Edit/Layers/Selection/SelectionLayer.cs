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

        private readonly List<DrawableHitObject> selectedHitObjects = new List<DrawableHitObject>();

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            clearSelection();
            return true;
        }

        protected override bool OnDragStart(InputState state)
        {
            AddInternal(selectionBox = new SelectionBox());
            return true;
        }

        protected override bool OnDrag(InputState state)
        {
            selectionBox.Show();

            var dragPosition = state.Mouse.NativeState.Position;
            var dragStartPosition = state.Mouse.NativeState.PositionMouseDown ?? dragPosition;

            var screenSpaceDragQuad = new Quad(dragStartPosition.X, dragStartPosition.Y, dragPosition.X - dragStartPosition.X, dragPosition.Y - dragStartPosition.Y);

            selectionBox.SetDragRectangle(screenSpaceDragQuad.AABBFloat);
            selectQuad(screenSpaceDragQuad);

            return true;
        }

        protected override bool OnDragEnd(InputState state)
        {
            selectionBox.Hide();
            selectionBox.Expire();

            finishSelection();

            return true;
        }

        protected override bool OnClick(InputState state)
        {
            selectPoint(state.Mouse.NativeState.Position);
            finishSelection();

            return true;
        }

        /// <summary>
        /// Deselects all selected <see cref="DrawableHitObject"/>s.
        /// </summary>
        private void clearSelection()
        {
            selectedHitObjects.Clear();
            captureBox?.Hide();
            captureBox?.Expire();
        }

        /// <summary>
        /// Selects all hitobjects that are present within the area of a <see cref="Quad"/>.
        /// </summary>
        /// <param name="screenSpaceQuad">The selection <see cref="Quad"/>.</param>
        private void selectQuad(Quad screenSpaceQuad)
        {
            foreach (var obj in playfield.HitObjects.Objects.Where(h => h.IsAlive && h.IsPresent && screenSpaceQuad.Contains(h.SelectionPoint)))
                selectedHitObjects.Add(obj);
        }

        /// <summary>
        /// Selects the top-most hitobject that is present under a specific point.
        /// </summary>
        /// <param name="screenSpacePoint">The <see cref="Vector2"/> to select at.</param>
        private void selectPoint(Vector2 screenSpacePoint)
        {
            var selected = playfield.HitObjects.Objects.Reverse().Where(h => h.IsAlive && h.IsPresent).FirstOrDefault(h => h.ReceiveMouseInputAt(screenSpacePoint));
            if (selected == null)
                return;

            selectedHitObjects.Add(selected);
        }

        private void finishSelection()
        {
            if (selectedHitObjects.Count == 0)
                return;

            AddInternal(captureBox = new CaptureBox(this, selectedHitObjects.ToList()));
        }
    }
}
