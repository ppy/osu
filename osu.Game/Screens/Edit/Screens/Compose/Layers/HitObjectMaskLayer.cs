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
        private readonly Playfield playfield;
        private readonly HitObjectComposer composer;

        private MaskContainer maskContainer;
        private SelectionBox selectionBox;

        public HitObjectMaskLayer(Playfield playfield, HitObjectComposer composer)
        {
            this.playfield = playfield;
            this.composer = composer;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            maskContainer = new MaskContainer();

            selectionBox = composer.CreateSelectionBox(maskContainer);

            dragLayer.DragEnd += () => selectionBox.UpdateVisibility();
            var dragLayer = new DragLayer(maskContainer.Select);

            InternalChildren = new Drawable[]
            {
                dragLayer,
                maskContainer,
                selectionBox,
                dragLayer.CreateProxy()
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

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            selectionBox.DeselectAll();
            return true;
        }
    }
}
