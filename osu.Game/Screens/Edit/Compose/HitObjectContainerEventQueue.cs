// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Screens.Edit.Compose
{
    /// <summary>
    /// A queue which processes events from the many <see cref="HitObjectContainer"/>s in a nested <see cref="Playfield"/> hierarchy.
    /// </summary>
    internal class HitObjectContainerEventQueue : Component
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
        /// Creates a new <see cref="HitObjectContainerEventQueue"/>.
        /// </summary>
        /// <param name="playfield">The most top-level <see cref="Playfield"/>.</param>
        public HitObjectContainerEventQueue([NotNull] Playfield playfield)
        {
            this.playfield = playfield;

            playfield.HitObjectUsageBegan += onHitObjectUsageBegan;
            playfield.HitObjectUsageFinished += onHitObjectUsageFinished;
        }

        private readonly Dictionary<HitObject, int> pendingUsagesBegan = new Dictionary<HitObject, int>();
        private readonly Dictionary<HitObject, int> pendingUsagesFinished = new Dictionary<HitObject, int>();

        private void onHitObjectUsageBegan(HitObject hitObject) => pendingUsagesBegan[hitObject] = pendingUsagesBegan.GetValueOrDefault(hitObject, 0) + 1;

        private void onHitObjectUsageFinished(HitObject hitObject) => pendingUsagesFinished[hitObject] = pendingUsagesFinished.GetValueOrDefault(hitObject, 0) + 1;

        protected override void Update()
        {
            base.Update();

            foreach (var (hitObject, countBegan) in pendingUsagesBegan)
            {
                if (pendingUsagesFinished.TryGetValue(hitObject, out int countFinished))
                {
                    Debug.Assert(countFinished > 0);

                    if (countBegan > countFinished)
                    {
                        // The hitobject is still in use, but transferred to a different HOC.
                        HitObjectUsageTransferred?.Invoke(hitObject, playfield.AllHitObjects.Single(d => d.HitObject == hitObject));
                        pendingUsagesFinished.Remove(hitObject);
                    }
                }
                else
                {
                    // This is a new usage of the hitobject.
                    HitObjectUsageBegan?.Invoke(hitObject);
                }
            }

            // Go through any remaining pending finished usages.
            foreach (var (hitObject, _) in pendingUsagesFinished)
                HitObjectUsageFinished?.Invoke(hitObject);

            pendingUsagesBegan.Clear();
            pendingUsagesFinished.Clear();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            playfield.HitObjectUsageBegan -= onHitObjectUsageBegan;
            playfield.HitObjectUsageFinished -= onHitObjectUsageFinished;
        }
    }
}
