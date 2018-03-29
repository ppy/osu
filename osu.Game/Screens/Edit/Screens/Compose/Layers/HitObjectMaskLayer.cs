// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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

        public HitObjectMaskLayer(Playfield playfield, HitObjectComposer composer)
        {
            this.playfield = playfield;
            this.composer = composer;

            RelativeSizeAxes = Axes.Both;

            InternalChild = overlayContainer = new Container<HitObjectMask> { RelativeSizeAxes = Axes.Both };
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

            overlayContainer.Add(overlay);
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

            existing.Hide();
            existing.Expire();
        }

        private SelectionBox currentSelectionBox;

        private void addSelectionBox()
        {
            if (overlayContainer.Count > 0)
                AddInternal(currentSelectionBox = composer.CreateSelectionBox(overlayContainer));
        }

        private void removeSelectionBox()
        {
            currentSelectionBox?.Hide();
            currentSelectionBox?.Expire();
        }
    }
}
