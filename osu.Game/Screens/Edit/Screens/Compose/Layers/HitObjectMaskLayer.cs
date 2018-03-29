// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Screens.Edit.Screens.Compose.Layers
{
    public class HitObjectMaskLayer : CompositeDrawable
    {
        private readonly Playfield playfield;
        private readonly HitObjectComposer composer;
        private readonly Container<HitObjectMask> overlayContainer;

        private readonly SelectionBox selectionBox;

        private readonly HashSet<HitObjectMask> selectedObjects = new HashSet<HitObjectMask>();

        public HitObjectMaskLayer(Playfield playfield, HitObjectComposer composer)
        {
            this.playfield = playfield;
            this.composer = composer;

            RelativeSizeAxes = Axes.Both;

            overlayContainer = new Container<HitObjectMask>();
            selectionBox = composer.CreateSelectionBox();

            var dragBox = new DragBox(overlayContainer);
            dragBox.DragEnd += () => selectionBox.FinishSelection();

            InternalChildren = new Drawable[]
            {
                dragBox,
                overlayContainer,
                selectionBox,
                dragBox.CreateProxy()
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (var obj in playfield.HitObjects.Objects)
                addOverlay(obj);
        }

        /// <summary>
        /// Adds an overlay for a <see cref="DrawableHitObject"/> which adds movement support.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to create an overlay for.</param>
        private void addOverlay(DrawableHitObject hitObject)
        {
            var overlay = composer.CreateMaskFor(hitObject);
            if (overlay == null)
                return;

            overlay.Selected += onSelected;
            overlay.Deselected += onDeselected;
            overlay.SingleSelectionRequested += onSingleSelectionRequested;

            overlayContainer.Add(overlay);
            selectionBox.AddMask(overlay);
        }

        /// <summary>
        /// Removes the overlay for a <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to remove the overlay for.</param>
        private void removeOverlay(DrawableHitObject hitObject)
        {
            var existing = overlayContainer.FirstOrDefault(h => h.HitObject == hitObject);
            if (existing == null)
                return;

            existing.Selected -= onSelected;
            existing.Deselected -= onDeselected;
            existing.SingleSelectionRequested -= onSingleSelectionRequested;

            overlayContainer.Remove(existing);
            selectionBox.RemoveMask(existing);
        }

        private void onSelected(HitObjectMask mask) => selectedObjects.Add(mask);

        private void onDeselected(HitObjectMask mask) => selectedObjects.Remove(mask);

        private void onSingleSelectionRequested(HitObjectMask mask) => DeselectAll();

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            DeselectAll();
            return true;
        }

        /// <summary>
        /// Deselects all selected <see cref="DrawableHitObject"/>s.
        /// </summary>
        public void DeselectAll() => overlayContainer.ToList().ForEach(m => m.Deselect());
    }
}
