// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.States;
using osu.Game.Rulesets.Edit;
using OpenTK;
using RectangleF = osu.Framework.Graphics.Primitives.RectangleF;

namespace osu.Game.Screens.Edit.Screens.Compose.Layers
{
    public class MaskContainer : Container<SelectionMask>
    {
        /// <summary>
        /// Invoked when any <see cref="SelectionMask"/> is selected.
        /// </summary>
        public event Action<SelectionMask> MaskSelected;

        /// <summary>
        /// Invoked when any <see cref="SelectionMask"/> is deselected.
        /// </summary>
        public event Action<SelectionMask> MaskDeselected;

        /// <summary>
        /// Invoked when any <see cref="SelectionMask"/> requests selection.
        /// </summary>
        public event Action<SelectionMask, InputState> MaskSelectionRequested;

        /// <summary>
        /// Invoked when any <see cref="SelectionMask"/> requests drag.
        /// </summary>
        public event Action<SelectionMask, Vector2, InputState> MaskDragRequested;

        private IEnumerable<SelectionMask> aliveMasks => AliveInternalChildren.Cast<SelectionMask>();

        public MaskContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        public override void Add(SelectionMask drawable)
        {
            if (drawable == null) throw new ArgumentNullException(nameof(drawable));

            base.Add(drawable);

            drawable.Selected += onMaskSelected;
            drawable.Deselected += onMaskDeselected;
            drawable.SelectionRequested += onSelectionRequested;
            drawable.DragRequested += onDragRequested;
        }

        public override bool Remove(SelectionMask drawable)
        {
            if (drawable == null) throw new ArgumentNullException(nameof(drawable));

            var result = base.Remove(drawable);

            if (result)
            {
                drawable.Selected -= onMaskSelected;
                drawable.Deselected -= onMaskDeselected;
                drawable.SelectionRequested -= onSelectionRequested;
                drawable.DragRequested -= onDragRequested;
            }

            return result;
        }

        /// <summary>
        /// Select all masks in a given rectangle selection area.
        /// </summary>
        /// <param name="rect">The rectangle to perform a selection on in screen-space coordinates.</param>
        public void Select(RectangleF rect)
        {
            foreach (var mask in aliveMasks.ToList())
            {
                if (mask.IsPresent && rect.Contains(mask.SelectionPoint))
                    mask.Select();
                else
                    mask.Deselect();
            }
        }

        /// <summary>
        /// Deselects all selected <see cref="SelectionMask"/>s.
        /// </summary>
        public void DeselectAll() => aliveMasks.ToList().ForEach(m => m.Deselect());

        private void onMaskSelected(SelectionMask mask)
        {
            MaskSelected?.Invoke(mask);
            ChangeChildDepth(mask, 1);
        }

        private void onMaskDeselected(SelectionMask mask)
        {
            MaskDeselected?.Invoke(mask);
            ChangeChildDepth(mask, 0);
        }

        private void onSelectionRequested(SelectionMask mask, InputState state) => MaskSelectionRequested?.Invoke(mask, state);
        private void onDragRequested(SelectionMask mask, Vector2 delta, InputState state) => MaskDragRequested?.Invoke(mask, delta, state);

        protected override int Compare(Drawable x, Drawable y)
        {
            if (!(x is SelectionMask xMask) || !(y is SelectionMask yMask))
                return base.Compare(x, y);
            return Compare(xMask, yMask);
        }

        public int Compare(SelectionMask x, SelectionMask y)
        {
            // dpeth is used to denote selected status (we always want selected masks to handle input first).
            int d = x.Depth.CompareTo(y.Depth);
            if (d != 0)
                return d;

            // Put earlier hitobjects towards the end of the list, so they handle input first
            int i = y.HitObject.HitObject.StartTime.CompareTo(x.HitObject.HitObject.StartTime);
            return i == 0 ? CompareReverseChildID(x, y) : i;
        }
    }
}
