// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Rulesets.Edit.Types;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;

namespace osu.Game.Rulesets.Edit.Layers.Selection
{
    public class HitObjectOverlayLayer : CompositeDrawable
    {
        private readonly Container<HitObjectOverlay> overlayContainer;

        public HitObjectOverlayLayer()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChild = overlayContainer = new Container<HitObjectOverlay> { RelativeSizeAxes = Axes.Both };
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

        public void MoveObjects(Vector2 offset)
        {
            // Todo: Various forms of snapping
            foreach (var hitObject in overlayContainer.Select(o => o.HitObject.HitObject))
            {
                switch (hitObject)
                {
                    case IHasEditablePosition editablePosition:
                        editablePosition.SetPosition(offset);
                        break;
                }
            }
        }

        /// <summary>
        /// Creates a <see cref="HitObjectOverlay"/> for a specific <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to create the overlay for.</param>
        protected virtual HitObjectOverlay CreateOverlayFor(DrawableHitObject hitObject) => null;
    }
}
