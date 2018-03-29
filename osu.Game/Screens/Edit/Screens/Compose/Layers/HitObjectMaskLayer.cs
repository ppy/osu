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

        private Container<HitObjectMask> maskContainer;
        private SelectionBox selectionBox;

        private readonly HashSet<HitObjectMask> selectedMasks = new HashSet<HitObjectMask>();

        public HitObjectMaskLayer(Playfield playfield, HitObjectComposer composer)
        {
            this.playfield = playfield;
            this.composer = composer;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            maskContainer = new Container<HitObjectMask>();
            selectionBox = composer.CreateSelectionBox();

            var dragBox = new DragBox(maskContainer);
            dragBox.DragEnd += () => selectionBox.UpdateVisibility();

            InternalChildren = new Drawable[]
            {
                dragBox,
                maskContainer,
                selectionBox,
                dragBox.CreateProxy()
            };

            foreach (var obj in playfield.HitObjects.Objects)
                addMask(obj);
        }

        /// <summary>
        /// Adds a mask for a <see cref="DrawableHitObject"/> which adds movement support.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to create a mask for.</param>
        private void addMask(DrawableHitObject hitObject)
        {
            var mask = composer.CreateMaskFor(hitObject);
            if (mask == null)
                return;

            mask.Selected += onSelected;
            mask.Deselected += onDeselected;
            mask.SingleSelectionRequested += onSingleSelectionRequested;

            maskContainer.Add(mask);
            selectionBox.AddMask(mask);
        }

        /// <summary>
        /// Removes the mask for a <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to remove the mask for.</param>
        private void removeMask(DrawableHitObject hitObject)
        {
            var mask = maskContainer.FirstOrDefault(h => h.HitObject == hitObject);
            if (mask == null)
                return;

            mask.Selected -= onSelected;
            mask.Deselected -= onDeselected;
            mask.SingleSelectionRequested -= onSingleSelectionRequested;

            maskContainer.Remove(mask);
            selectionBox.RemoveMask(mask);
        }

        private void onSelected(HitObjectMask mask) => selectedMasks.Add(mask);

        private void onDeselected(HitObjectMask mask) => selectedMasks.Remove(mask);

        private void onSingleSelectionRequested(HitObjectMask mask) => DeselectAll();

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            DeselectAll();
            return true;
        }

        /// <summary>
        /// Deselects all selected <see cref="DrawableHitObject"/>s.
        /// </summary>
        public void DeselectAll() => selectedMasks.ToList().ForEach(m => m.Deselect());
    }
}
