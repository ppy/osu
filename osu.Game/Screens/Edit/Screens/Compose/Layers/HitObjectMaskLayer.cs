// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Screens.Edit.Screens.Compose.Layers
{
    public class HitObjectMaskLayer : CompositeDrawable
    {
        private readonly HitObjectComposer composer;
        private readonly Container<HitObjectMask> overlayContainer;

        public HitObjectMaskLayer(HitObjectComposer composer)
        {
            this.composer = composer;
            RelativeSizeAxes = Axes.Both;

            InternalChild = overlayContainer = new Container<HitObjectMask> { RelativeSizeAxes = Axes.Both };
        }

        /// <summary>
        /// Adds an overlay for a <see cref="DrawableHitObject"/> which adds movement support.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to create an overlay for.</param>
        public void AddOverlay(DrawableHitObject hitObject)
        {
            var overlay = composer.CreateMaskFor(hitObject);
            if (overlay == null)
                return;

            overlayContainer.Add(overlay);
        }

        /// <summary>
        /// Removes the overlay for a <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to remove the overlay for.</param>
        public void RemoveOverlay(DrawableHitObject hitObject)
        {
            var existing = overlayContainer.FirstOrDefault(h => h.HitObject == hitObject);
            if (existing == null)
                return;

            existing.Hide();
            existing.Expire();
        }

        private SelectionBox currentSelectionBox;

        public void AddSelectionOverlay()
        {
            if (overlayContainer.Count > 0)
                AddInternal(currentSelectionBox = composer.CreateSelectionOverlay(overlayContainer));
        }

        public void RemoveSelectionOverlay()
        {
            currentSelectionBox?.Hide();
            currentSelectionBox?.Expire();
        }
    }
}
