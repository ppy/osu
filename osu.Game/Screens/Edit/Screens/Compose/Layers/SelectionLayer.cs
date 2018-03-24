// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using OpenTK;
using OpenTK.Graphics;
using RectangleF = osu.Framework.Graphics.Primitives.RectangleF;

namespace osu.Game.Screens.Edit.Screens.Compose.Layers
{
    public class SelectionLayer : CompositeDrawable
    {
        /// <summary>
        /// Invoked when a <see cref="DrawableHitObject"/> is selected.
        /// </summary>
        public event Action<DrawableHitObject> ObjectSelected;

        /// <summary>
        /// Invoked when a <see cref="DrawableHitObject"/> is deselected.
        /// </summary>
        public event Action<DrawableHitObject> ObjectDeselected;

        /// <summary>
        /// Invoked when the selection has been cleared.
        /// </summary>
        public event Action SelectionCleared;

        /// <summary>
        /// Invoked when the user has finished selecting all <see cref="DrawableHitObject"/>s.
        /// </summary>
        public event Action SelectionFinished;

        private readonly Playfield playfield;

        public SelectionLayer(Playfield playfield)
        {
            this.playfield = playfield;

            RelativeSizeAxes = Axes.Both;
        }

        private DragBox dragBox;

        private readonly HashSet<DrawableHitObject> selectedHitObjects = new HashSet<DrawableHitObject>();

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            DeselectAll();
            return true;
        }

        protected override bool OnDragStart(InputState state)
        {
            AddInternal(dragBox = new DragBox());
            return true;
        }

        protected override bool OnDrag(InputState state)
        {
            dragBox.Show();

            var dragPosition = state.Mouse.NativeState.Position;
            var dragStartPosition = state.Mouse.NativeState.PositionMouseDown ?? dragPosition;

            var screenSpaceDragQuad = new Quad(dragStartPosition.X, dragStartPosition.Y, dragPosition.X - dragStartPosition.X, dragPosition.Y - dragStartPosition.Y);

            dragBox.SetDragRectangle(screenSpaceDragQuad.AABBFloat);
            selectQuad(screenSpaceDragQuad);

            return true;
        }

        protected override bool OnDragEnd(InputState state)
        {
            dragBox.Hide();
            dragBox.Expire();

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
        /// Selects a <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to select.</param>
        public void Select(DrawableHitObject hitObject)
        {
            if (!select(hitObject))
                return;

            clearSelection();
            finishSelection();
        }

        /// <summary>
        /// Selects a <see cref="DrawableHitObject"/> without performing capture updates.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to select.</param>
        /// <returns>Whether <paramref name="hitObject"/> was selected.</returns>
        private bool select(DrawableHitObject hitObject)
        {
            if (!selectedHitObjects.Add(hitObject))
                return false;

            ObjectSelected?.Invoke(hitObject);
            return true;
        }

        /// <summary>
        /// Deselects a <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to deselect.</param>
        public void Deselect(DrawableHitObject hitObject)
        {
            if (!deselect(hitObject))
                return;

            clearSelection();
            finishSelection();
        }

        /// <summary>
        /// Deselects a <see cref="DrawableHitObject"/> without performing capture updates.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to deselect.</param>
        /// <returns>Whether the <see cref="DrawableHitObject"/> was deselected.</returns>
        private bool deselect(DrawableHitObject hitObject)
        {
            if (!selectedHitObjects.Remove(hitObject))
                return false;

            ObjectDeselected?.Invoke(hitObject);
            return true;
        }

        /// <summary>
        /// Deselects all selected <see cref="DrawableHitObject"/>s.
        /// </summary>
        public void DeselectAll()
        {
            selectedHitObjects.ForEach(h => ObjectDeselected?.Invoke(h));
            selectedHitObjects.Clear();

            clearSelection();
        }

        /// <summary>
        /// Selects all hitobjects that are present within the area of a <see cref="Quad"/>.
        /// </summary>
        /// <param name="screenSpaceQuad">The selection <see cref="Quad"/>.</param>
        // Todo: If needed we can severely reduce allocations in this method
        private void selectQuad(Quad screenSpaceQuad)
        {
            var expectedSelection = playfield.HitObjects.Objects.Where(h => h.IsAlive && h.IsPresent && screenSpaceQuad.Contains(h.SelectionPoint)).ToList();

            var toRemove = selectedHitObjects.Except(expectedSelection).ToList();
            foreach (var obj in toRemove)
                deselect(obj);

            expectedSelection.ForEach(h => select(h));
        }

        /// <summary>
        /// Selects the top-most hitobject that is present under a specific point.
        /// </summary>
        /// <param name="screenSpacePoint">The <see cref="Vector2"/> to select at.</param>
        private void selectPoint(Vector2 screenSpacePoint)
        {
            var target = playfield.HitObjects.Objects.Reverse().Where(h => h.IsAlive && h.IsPresent).FirstOrDefault(h => h.ReceiveMouseInputAt(screenSpacePoint));
            if (target == null)
                return;

            select(target);
        }

        private void clearSelection() => SelectionCleared?.Invoke();

        private void finishSelection()
        {
            if (selectedHitObjects.Count == 0)
                return;
            SelectionFinished?.Invoke();
        }

        /// <summary>
        /// A box that represents a drag selection.
        /// </summary>
        private class DragBox : VisibilityContainer
        {
            /// <summary>
            /// Creates a new <see cref="DragBox"/>.
            /// </summary>
            public DragBox()
            {
                Masking = true;
                BorderColour = Color4.White;
                BorderThickness = SelectionBox.BORDER_RADIUS;

                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.1f
                };
            }

            public void SetDragRectangle(RectangleF rectangle)
            {
                var topLeft = Parent.ToLocalSpace(rectangle.TopLeft);
                var bottomRight = Parent.ToLocalSpace(rectangle.BottomRight);

                Position = topLeft;
                Size = bottomRight - topLeft;
            }

            public override bool DisposeOnDeathRemoval => true;

            protected override void PopIn() => this.FadeIn(250, Easing.OutQuint);
            protected override void PopOut() => this.FadeOut(250, Easing.OutQuint);
        }
    }
}
