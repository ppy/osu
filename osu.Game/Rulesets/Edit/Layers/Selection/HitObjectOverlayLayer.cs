// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Edit.Layers.Selection
{
    public class HitObjectOverlayLayer : CompositeDrawable
    {
        private readonly Dictionary<DrawableHitObject, HitObjectOverlay> existingOverlays = new Dictionary<DrawableHitObject, HitObjectOverlay>();

        public HitObjectOverlayLayer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        /// <summary>
        /// Adds an overlay for a <see cref="DrawableHitObject"/> which adds movement support.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to create an overlay for.</param>
        public void AddOverlay(DrawableHitObject hitObject)
        {
            var overlay = CreateOverlayFor(hitObject);
            if (overlay == null)
                return;

            existingOverlays[hitObject] = overlay;
            AddInternal(overlay);
        }

        /// <summary>
        /// Removes the overlay for a <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to remove the overlay for.</param>
        public void RemoveOverlay(DrawableHitObject hitObject)
        {
            if (!existingOverlays.TryGetValue(hitObject, out var existing))
                return;

            existing.Hide();
            existing.Expire();
        }

        /// <summary>
        /// Creates a <see cref="HitObjectOverlay"/> for a specific <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to create the overlay for.</param>
        protected virtual HitObjectOverlay CreateOverlayFor(DrawableHitObject hitObject) => null;
    }
}
