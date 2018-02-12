// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;

namespace osu.Game.Rulesets.Edit.Layers.Selection
{
    public class HitObjectCapturer
    {
        public event Action<DrawableHitObject> HitObjectCaptured;

        private readonly IEnumerable<DrawableHitObject> capturableHitObjects;

        public HitObjectCapturer(IEnumerable<DrawableHitObject> capturableHitObjects)
        {
            this.capturableHitObjects = capturableHitObjects;
        }

        /// <summary>
        /// Captures all hitobjects that are present within the area of a <see cref="Quad"/>.
        /// </summary>
        /// <param name="screenSpaceQuad">The capture <see cref="Quad"/>.</param>
        /// <returns>If any <see cref="DrawableHitObject"/>s were captured.</returns>
        public bool CaptureQuad(Quad screenSpaceQuad)
        {
            bool anyCaptured = false;
            foreach (var obj in capturableHitObjects.Where(h => h.IsAlive && h.IsPresent && screenSpaceQuad.Contains(h.SelectionPoint)))
            {
                HitObjectCaptured?.Invoke(obj);
                anyCaptured = true;
            }

            return anyCaptured;
        }

        /// <summary>
        /// Captures the top-most hitobject that is present under a specific point.
        /// </summary>
        /// <param name="screenSpacePoint">The <see cref="Vector2"/> to capture at.</param>
        /// <returns>Whether a <see cref="DrawableHitObject"/> was captured.</returns>
        public bool CapturePoint(Vector2 screenSpacePoint)
        {
            var captured = capturableHitObjects.Reverse().Where(h => h.IsAlive && h.IsPresent).FirstOrDefault(h => h.ReceiveMouseInputAt(screenSpacePoint));
            if (captured == null)
                return false;

            HitObjectCaptured?.Invoke(captured);
            return true;
        }
    }
}
