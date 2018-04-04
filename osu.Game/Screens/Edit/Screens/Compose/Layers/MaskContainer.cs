// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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

        public event Action<HitObjectMask> MaskSelectionRequested;

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
        }

        public override bool Remove(HitObjectMask drawable)
        {
            var result = base.Remove(drawable);

            if (result)
            {
                drawable.Selected -= onMaskSelected;
                drawable.Deselected -= onMaskDeselected;
                drawable.SelectionRequested -= onSelectionRequested;
            }

            return result;
        }

        /// <summary>
        /// Select all masks in a given rectangle selection area.
        /// </summary>
        /// <param name="rect">The rectangle to perform a selection on in screen-space coordinates.</param>
        public void Select(RectangleF rect)
        {
            foreach (var mask in aliveMasks)
            {
                if (mask.IsPresent && rect.Contains(mask.SelectionPoint))
                    mask.Select();
            }
        }

        /// <summary>
        /// Deselects all selected <see cref="HitObjectMask"/>s.
        /// </summary>
        public void DeselectAll() => aliveMasks.ToList().ForEach(m => m.Deselect());

        private void onMaskSelected(HitObjectMask mask) => MaskSelected?.Invoke(mask);
        private void onMaskDeselected(HitObjectMask mask) => MaskDeselected?.Invoke(mask);
        private void onSelectionRequested(HitObjectMask mask) => MaskSelectionRequested?.Invoke(mask);

        protected override int Compare(Drawable x, Drawable y)
        {
            if (!(x is HitObjectMask xMask) || !(y is HitObjectMask yMask))
                return base.Compare(x, y);
            return Compare(xMask, yMask);
        }

        public int Compare(HitObjectMask x, HitObjectMask y)
        {
            // Put earlier hitobjects towards the end of the list, so they handle input first
            int i = y.HitObject.HitObject.StartTime.CompareTo(x.HitObject.HitObject.StartTime);
            return i == 0 ? CompareReverseChildID(x, y) : i;
        }
    }
}
