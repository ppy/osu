// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    /// <summary>
    /// The tail of a <see cref="DrawableHoldNote"/>.
    /// </summary>
    public class DrawableHoldNoteTail : DrawableNote
    {
        /// <summary>
        /// Lenience of release hit windows. This is to make cases where the hold note release
        /// is timed alongside presses of other hit objects less awkward.
        /// Todo: This shouldn't exist for non-LegacyBeatmapDecoder beatmaps
        /// </summary>
        private const double release_window_lenience = 1.5;

        private readonly DrawableHoldNote holdNote;

        public DrawableHoldNoteTail(DrawableHoldNote holdNote)
            : base(holdNote.HitObject.Tail)
        {
            this.holdNote = holdNote;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            // Factor in the release lenience
            timeOffset /= release_window_lenience;

            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(r => r.Type = HitResult.Miss);

                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None)
                return;

            ApplyResult(r =>
            {
                if (holdNote.HasBroken && (result == HitResult.Perfect || result == HitResult.Perfect))
                    result = HitResult.Good;

                r.Type = result;
            });
        }

        public override bool OnPressed(ManiaAction action) => false; // Tail doesn't handle key down

        public override bool OnReleased(ManiaAction action)
        {
            // Make sure that the user started holding the key during the hold note
            if (!holdNote.HoldStartTime.HasValue)
                return false;

            if (action != Action.Value)
                return false;

            UpdateResult(true);

            // Handled by the hold note, which will set holding = false
            return false;
        }
    }
}
