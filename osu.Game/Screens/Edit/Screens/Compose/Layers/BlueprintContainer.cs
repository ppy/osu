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
        private SelectionBox selectionBox;

        private IEnumerable<SelectionBlueprint> selections => selectionBlueprints.Children.Where(c => c.IsAlive);

        [Resolved]
        private HitObjectComposer composer { get; set; }

        public BlueprintContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            selectionBox = composer.CreateSelectionBox();
            selectionBox.DeselectAll = deselectAll;

            var dragBox = new DragBox(select);
            dragBox.DragEnd += () => selectionBox.UpdateVisibility();

            InternalChildren = new[]
            {
                dragBox,
                selectionBox,
                selectionBlueprints = new SelectionBlueprintContainer { RelativeSizeAxes = Axes.Both },
                dragBox.CreateProxy()
            };

            foreach (var obj in composer.HitObjects)
                AddBlueprintFor(obj);
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
        public void AddBlueprintFor(DrawableHitObject hitObject)
        {
            var mask = composer.CreateMaskFor(hitObject);
            if (mask == null)
                return;

            mask.Selected += onBlueprintSelected;
            mask.Deselected += onBlueprintDeselected;
            mask.SelectionRequested += onSelectionRequested;
            mask.DragRequested += onDragRequested;

            selectionBlueprints.Add(mask);
        }

        /// <summary>
        /// Removes a mask for a <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> for which to remove the mask.</param>
        public void RemoveBlueprintFor(DrawableHitObject hitObject)
        {
            var maskToRemove = selectionBlueprints.Single(m => m.HitObject == hitObject);
            if (maskToRemove == null)
                return;

            maskToRemove.Deselect();

            maskToRemove.Selected -= onBlueprintSelected;
            maskToRemove.Deselected -= onBlueprintDeselected;
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
            foreach (var mask in selections.ToList())
            {
                if (mask.IsPresent && rect.Contains(mask.SelectionPoint))
                    mask.Select();
                else
                    mask.Deselect();
            }
        }

        /// <summary>
        /// Deselects all selected <see cref="SelectionBlueprint"/>s.
        /// </summary>
        private void deselectAll() => selections.ToList().ForEach(m => m.Deselect());

        private void onBlueprintSelected(SelectionBlueprint blueprint)
        {
            selectionBox.HandleSelected(blueprint);
            selectionBlueprints.ChangeChildDepth(blueprint, 1);
        }

        private void onBlueprintDeselected(SelectionBlueprint blueprint)
        {
            selectionBox.HandleDeselected(blueprint);
            selectionBlueprints.ChangeChildDepth(blueprint, 0);
        }

        private void onSelectionRequested(SelectionBlueprint blueprint, InputState state) => selectionBox.HandleSelectionRequested(blueprint, state);

        private void onDragRequested(SelectionBlueprint blueprint, Vector2 delta, InputState state) => selectionBox.HandleDrag(blueprint, delta, state);

        private class SelectionBlueprintContainer : Container<SelectionBlueprint>
        {
            protected override int Compare(Drawable x, Drawable y)
            {
                if (!(x is SelectionBlueprint xBlueprint) || !(y is SelectionBlueprint yBlueprint))
                    return base.Compare(x, y);
                return Compare(xBlueprint, yBlueprint);
            }

            public int Compare(SelectionBlueprint x, SelectionBlueprint y)
            {
                // dpeth is used to denote selected status (we always want selected blueprints to handle input first).
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
