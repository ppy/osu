// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    /// Buffers events from the many <see cref="HitObjectContainer"/>s in a nested <see cref="Playfield"/> hierarchy.
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
                // This exists as a safeguard to ensure that the sequence: { Began -> Finished }, where { ... } indicates a sequence within a single frame, does not trigger any events.
                // This is unlikely to occur in practice as it requires the usage to finish immediately after the HitObjectContainer updates hitobject lifetimes,
                // however, an Editor action scheduled somewhere between the lifetime update and this buffer's own Update() could cause this.
                case (EventType.Began, EventType.Finished):
                    pendingEvents.Remove(hitObject);
                    break;

                // This exists as a safeguard to ensure that the sequence: Began -> { Finished -> Began -> Finished }, where { ... } indicates a sequence within a single frame,
                // correctly leads into a final "finished" state rather than remaining in the intermediate "transferred" state.
                // As above, this is unlikely to occur in practice.
                case (EventType.Transferred, EventType.Finished):
                    pendingEvents[hitObject] = EventType.Finished;
                    break;

                case (EventType.Finished, EventType.Began):
                    pendingEvents[hitObject] = EventType.Transferred;
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"Unexpected event update ({existingEvent} => {newEvent}).");
            }
        }

        public void Update()
        {
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

        public void Dispose()
        {
            if (playfield != null)
            {
                playfield.HitObjectUsageBegan -= onHitObjectUsageBegan;
                playfield.HitObjectUsageFinished -= onHitObjectUsageFinished;
            }
        }

        private enum EventType
        {
            /// <summary>
            /// A <see cref="HitObject"/> has started being used by a <see cref="DrawableHitObject"/>.
            /// </summary>
            Began,

            /// <summary>
            /// A <see cref="HitObject"/> has finished being used by a <see cref="DrawableHitObject"/>.
            /// </summary>
            Finished,

            /// <summary>
            /// An internal intermediate state that occurs when a <see cref="HitObject"/> has finished being used by one <see cref="DrawableHitObject"/>
            /// and started being used by another <see cref="DrawableHitObject"/> in the same frame. The <see cref="DrawableHitObject"/> may be the same instance in both cases.
            /// </summary>
            /// <remarks>
            /// This usually occurs when a <see cref="HitObject"/> is transferred between <see cref="HitObjectContainer"/>s,
            /// but also occurs if the <see cref="HitObject"/> dies and becomes alive again in the same frame within the same <see cref="HitObjectContainer"/>.
            /// </remarks>
            Transferred
        }
    }
}
