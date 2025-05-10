// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Screens.Edit.Compose
{
    /// <summary>
    /// Buffers events from the many <see cref="HitObjectContainer"/>s in a nested <see cref="Playfield"/> hierarchy
    /// to ensure correct ordering of events.
    /// </summary>
    internal class HitObjectUsageEventBuffer : IDisposable
    {
        /// <summary>
        /// Invoked when a <see cref="HitObject"/> becomes used by a <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <remarks>
        /// If the ruleset uses pooled objects, this represents the time when the <see cref="HitObject"/>s become alive.
        /// </remarks>
        public event Action<HitObject> HitObjectUsageBegan;

        /// <summary>
        /// Invoked when a <see cref="HitObject"/> becomes unused by a <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <remarks>
        /// If the ruleset uses pooled objects, this represents the time when the <see cref="HitObject"/>s become dead.
        /// </remarks>
        public event Action<HitObject> HitObjectUsageFinished;

        /// <summary>
        /// Invoked when a <see cref="HitObject"/> has been transferred to another <see cref="DrawableHitObject"/>.
        /// </summary>
        public event Action<HitObject, DrawableHitObject> HitObjectUsageTransferred;

        private readonly Playfield playfield;

        /// <summary>
        /// Creates a new <see cref="HitObjectUsageEventBuffer"/>.
        /// </summary>
        /// <param name="playfield">The most top-level <see cref="Playfield"/>.</param>
        public HitObjectUsageEventBuffer([NotNull] Playfield playfield)
        {
            this.playfield = playfield;

            playfield.HitObjectUsageBegan += onHitObjectUsageBegan;
            playfield.HitObjectUsageFinished += onHitObjectUsageFinished;
        }

        private readonly List<HitObject> usageFinishedHitObjects = new List<HitObject>();

        private void onHitObjectUsageBegan(HitObject hitObject)
        {
            if (usageFinishedHitObjects.Remove(hitObject))
                HitObjectUsageTransferred?.Invoke(hitObject, playfield.AllHitObjects.Single(d => d.HitObject == hitObject));
            else
                HitObjectUsageBegan?.Invoke(hitObject);
        }

        private void onHitObjectUsageFinished(HitObject hitObject) => usageFinishedHitObjects.Add(hitObject);

        public void Update()
        {
            foreach (var hitObject in usageFinishedHitObjects)
                HitObjectUsageFinished?.Invoke(hitObject);
            usageFinishedHitObjects.Clear();
        }

        public void Dispose()
        {
            if (playfield != null)
            {
                playfield.HitObjectUsageBegan -= onHitObjectUsageBegan;
                playfield.HitObjectUsageFinished -= onHitObjectUsageFinished;
            }
        }
    }
}
