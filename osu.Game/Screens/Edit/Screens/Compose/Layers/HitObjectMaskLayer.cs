// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
        protected readonly Playfield Playfield;
        protected readonly HitObjectComposer Composer;

        private MaskContainer maskContainer;

        public HitObjectMaskLayer(Playfield playfield, HitObjectComposer composer)
        {
            // we need the playfield as HitObjects may not be initialised until its BDL.
            Playfield = playfield;

            Composer = composer;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            maskContainer = new MaskContainer();

            var maskSelection = Composer.CreateMaskSelection();

            maskContainer.MaskSelected += maskSelection.HandleSelected;
            maskContainer.MaskDeselected += maskSelection.HandleDeselected;
            maskContainer.MaskSelectionRequested += maskSelection.HandleSelectionRequested;
            maskContainer.MaskDragStarted += maskSelection.HandleDragStart;
            maskContainer.MaskDragRequested += maskSelection.HandleDrag;
            maskContainer.MaskDragEnded += maskSelection.HandleDragEnd;

            maskSelection.DeselectAll = maskContainer.DeselectAll;

            var dragLayer = new DragLayer(maskContainer.Select);
            dragLayer.DragEnd += () => maskSelection.UpdateVisibility();

            InternalChildren = new[]
            {
                dragLayer,
                maskSelection,
                maskContainer,
                dragLayer.CreateProxy()
            };

            addMasks(Playfield);
        }

        private void addMasks(Playfield playfield)
        {
            foreach (var obj in playfield.HitObjects.Objects)
                addMask(obj);

            if (playfield.NestedPlayfields != null)
                foreach (var p in playfield.NestedPlayfields)
                    addMasks(p);
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            maskContainer.DeselectAll();
            return true;
        }

        /// <summary>
        /// Adds a mask for a <see cref="DrawableHitObject"/> which adds movement support.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to create a mask for.</param>
        private void addMask(DrawableHitObject hitObject)
        {
            var mask = Composer.CreateMaskFor(hitObject);
            if (mask == null)
                return;

            maskContainer.Add(mask);
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

            maskContainer.Remove(mask);
        }
    }
}
