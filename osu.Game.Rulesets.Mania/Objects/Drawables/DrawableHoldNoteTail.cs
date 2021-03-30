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

        protected override ManiaSkinComponents Component => ManiaSkinComponents.HoldNoteTail;

        private readonly DrawableHoldNote holdNote;

        public DrawableHoldNoteTail(DrawableHoldNote holdNote)
            : base(holdNote.HitObject.Tail)
        {
            this.holdNote = holdNote;
        }

        public void UpdateResult() => base.UpdateResult(true);

        protected override double MaximumJudgementOffset => base.MaximumJudgementOffset * release_window_lenience;

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            // Factor in the release lenience
            timeOffset /= release_window_lenience;

            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(r => r.Type = r.Judgement.MinResult);

                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None)
                return;

            ApplyResult(r =>
            {
                // If the head wasn't hit or the hold note was broken, cap the max score to Meh.
                if (result > HitResult.Meh && (!holdNote.Head.IsHit || holdNote.HoldBrokenTime != null))
                    result = HitResult.Meh;

                r.Type = result;
            });
        }

        public override bool OnPressed(ManiaAction action) => false; // Handled by the hold note

        public override void OnReleased(ManiaAction action)
        {
        }
    }
}
