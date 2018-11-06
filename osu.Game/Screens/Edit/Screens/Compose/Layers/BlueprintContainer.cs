// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;

namespace osu.Game.Screens.Edit.Screens.Compose.Layers
{
    public class BlueprintContainer : CompositeDrawable
    {
        private SelectionBlueprintContainer selectionBlueprints;
        private MaskSelection maskSelection;

        private IEnumerable<SelectionMask> aliveMasks => selectionBlueprints.Children.Where(c => c.IsAlive);

        [Resolved]
        private HitObjectComposer composer { get; set; }

        public BlueprintContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            maskSelection = composer.CreateMaskSelection();
            maskSelection.DeselectAll = deselectAll;

            var dragBox = new DragBox(select);
            dragBox.DragEnd += () => maskSelection.UpdateVisibility();

            InternalChildren = new[]
            {
                dragBox,
                maskSelection,
                selectionBlueprints = new SelectionBlueprintContainer { RelativeSizeAxes = Axes.Both },
                dragBox.CreateProxy()
            };

            foreach (var obj in composer.HitObjects)
                AddMaskFor(obj);
        }

        protected override bool OnClick(ClickEvent e)
        {
            deselectAll();
            return true;
        }

        /// <summary>
        /// Adds a mask for a <see cref="DrawableHitObject"/> which adds movement support.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to create a mask for.</param>
        public void AddMaskFor(DrawableHitObject hitObject)
        {
            var mask = composer.CreateMaskFor(hitObject);
            if (mask == null)
                return;

            mask.Selected += onMaskSelected;
            mask.Deselected += onMaskDeselected;
            mask.SelectionRequested += onSelectionRequested;
            mask.DragRequested += onDragRequested;

            selectionBlueprints.Add(mask);
        }

        /// <summary>
        /// Removes a mask for a <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> for which to remove the mask.</param>
        public void RemoveMaskFor(DrawableHitObject hitObject)
        {
            var maskToRemove = selectionBlueprints.Single(m => m.HitObject == hitObject);
            if (maskToRemove == null)
                return;

            maskToRemove.Deselect();

            maskToRemove.Selected -= onMaskSelected;
            maskToRemove.Deselected -= onMaskDeselected;
            maskToRemove.SelectionRequested -= onSelectionRequested;
            maskToRemove.DragRequested -= onDragRequested;

            selectionBlueprints.Remove(maskToRemove);
        }

        /// <summary>
        /// Select all masks in a given rectangle selection area.
        /// </summary>
        /// <param name="rect">The rectangle to perform a selection on in screen-space coordinates.</param>
        private void select(RectangleF rect)
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
        private void deselectAll() => aliveMasks.ToList().ForEach(m => m.Deselect());

        private void onMaskSelected(SelectionMask mask)
        {
            maskSelection.HandleSelected(mask);
            selectionBlueprints.ChangeChildDepth(mask, 1);
        }

        private void onMaskDeselected(SelectionMask mask)
        {
            maskSelection.HandleDeselected(mask);
            selectionBlueprints.ChangeChildDepth(mask, 0);
        }

        private void onSelectionRequested(SelectionMask mask, InputState state) => maskSelection.HandleSelectionRequested(mask, state);

        private void onDragRequested(SelectionMask mask, Vector2 delta, InputState state) => maskSelection.HandleDrag(mask, delta, state);

        private class SelectionBlueprintContainer : Container<SelectionMask>
        {
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
}
