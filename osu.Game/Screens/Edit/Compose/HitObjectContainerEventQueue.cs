// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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

        private readonly Dictionary<HitObject, EventType> pendingEvents = new Dictionary<HitObject, EventType>();

        private void onHitObjectUsageBegan(HitObject hitObject) => updateEvent(hitObject, EventType.Began);

        private void onHitObjectUsageFinished(HitObject hitObject) => updateEvent(hitObject, EventType.Finished);

        private void updateEvent(HitObject hitObject, EventType newEvent)
        {
            if (!pendingEvents.TryGetValue(hitObject, out EventType existingEvent))
            {
                pendingEvents[hitObject] = newEvent;
                return;
            }

            switch (existingEvent, newEvent)
            {
                case (EventType.Transferred, EventType.Finished):
                    pendingEvents[hitObject] = EventType.Finished;
                    break;

                case (EventType.Began, EventType.Finished):
                case (EventType.Finished, EventType.Began):
                    pendingEvents[hitObject] = EventType.Transferred;
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"Unexpected event update ({existingEvent} => {newEvent}).");
            }
        }

        protected override void Update()
        {
            base.Update();

            foreach (var (hitObject, e) in pendingEvents)
            {
                switch (e)
                {
                    case EventType.Began:
                        HitObjectUsageBegan?.Invoke(hitObject);
                        break;

                    case EventType.Transferred:
                        HitObjectUsageTransferred?.Invoke(hitObject, playfield.AllHitObjects.Single(d => d.HitObject == hitObject));
                        break;

                    case EventType.Finished:
                        HitObjectUsageFinished?.Invoke(hitObject);
                        break;
                }
            }

            pendingEvents.Clear();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            playfield.HitObjectUsageBegan -= onHitObjectUsageBegan;
            playfield.HitObjectUsageFinished -= onHitObjectUsageFinished;
        }

        private enum EventType
        {
            Began,
            Finished,
            Transferred
        }
    }
}
