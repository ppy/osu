// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Rulesets.Edit;
using RectangleF = osu.Framework.Graphics.Primitives.RectangleF;

namespace osu.Game.Screens.Edit.Screens.Compose.Layers
{
    public class MaskContainer : Container<HitObjectMask>
    {
        /// <summary>
        /// Invoked when any <see cref="HitObjectMask"/> is selected.
        /// </summary>
        public event Action<HitObjectMask> MaskSelected;

        /// <summary>
        /// Invoked when any <see cref="HitObjectMask"/> is deselected.
        /// </summary>
        public event Action<HitObjectMask> MaskDeselected;

        /// <summary>
        /// Invoked when any <see cref="HitObjectMask"/> requests selection.
        /// </summary>
        public event Action<HitObjectMask, InputState> MaskSelectionRequested;

        /// <summary>
        /// Invoked when any <see cref="HitObjectMask"/> requests drag.
        /// </summary>
        public event Action<HitObjectMask, InputState> MaskDragRequested;

        private IEnumerable<HitObjectMask> aliveMasks => AliveInternalChildren.Cast<HitObjectMask>();

        public MaskContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        public override void Add(HitObjectMask drawable)
        {
            base.Add(drawable);

            drawable.Selected += onMaskSelected;
            drawable.Deselected += onMaskDeselected;
            drawable.SelectionRequested += onSelectionRequested;
            drawable.DragRequested += onDragRequested;
        }

        public override bool Remove(HitObjectMask drawable)
        {
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
        /// Deselects all selected <see cref="HitObjectMask"/>s.
        /// </summary>
        public void DeselectAll() => aliveMasks.ToList().ForEach(m => m.Deselect());

        private void onMaskSelected(HitObjectMask mask)
        {
            MaskSelected?.Invoke(mask);
            ChangeChildDepth(mask, 1);
        }

        private void onMaskDeselected(HitObjectMask mask)
        {
            MaskDeselected?.Invoke(mask);
            ChangeChildDepth(mask, 0);
        }

        private void onSelectionRequested(HitObjectMask mask, InputState state) => MaskSelectionRequested?.Invoke(mask, state);
        private void onDragRequested(HitObjectMask mask, InputState state) => MaskDragRequested?.Invoke(mask, state);

        protected override int Compare(Drawable x, Drawable y)
        {
            if (!(x is HitObjectMask xMask) || !(y is HitObjectMask yMask))
                return base.Compare(x, y);
            return Compare(xMask, yMask);
        }

        public int Compare(HitObjectMask x, HitObjectMask y)
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
