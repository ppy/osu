// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Game.Rulesets.Taiko.Judgements;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public abstract class DrawableHitStrong : DrawableHit
    {
        /// <summary>
        /// The lenience for the second key press.
        /// This does not adjust by map difficulty in ScoreV2 yet.
        /// </summary>
        private const double second_hit_window = 30;

        private double firstHitTime;
        private bool firstKeyHeld;
        private TaikoAction firstHitAction;

        protected DrawableHitStrong(Hit hit)
            : base(hit)
        {
        }

        private bool processedSecondHit;
        public override bool AllJudged => processedSecondHit && base.AllJudged;

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (!base.AllJudged)
            {
                base.CheckForJudgements(userTriggered, timeOffset);
                return;
            }

            if (!userTriggered)
                return;

            // If we get here, we're assured that the key pressed is the correct secondary key

            if (Math.Abs(firstHitTime - Time.Current) < second_hit_window)
            {
                AddJudgement(new TaikoStrongHitJudgement());
                processedSecondHit = true;
            }
        }

        public override bool OnReleased(TaikoAction action)
        {
            if (action == firstHitAction)
                firstKeyHeld = false;
            return base.OnReleased(action);
        }

        public override bool OnPressed(TaikoAction action)
        {
            // Check if we've handled the first key
            if (!base.AllJudged)
            {
                // First key hasn't been handled yet, attempt to handle it
                bool handled = base.OnPressed(action);

                if (handled)
                {
                    firstHitTime = Time.Current;
                    firstHitAction = action;
                    firstKeyHeld = true;
                }

                return handled;
            }

            // If we've already hit the second key, don't handle this object any further
            if (processedSecondHit)
                return false;

            // Don't handle represses of the first key
            if (firstHitAction == action)
                return false;

            // Don't handle invalid hit action presses
            if (!HitActions.Contains(action))
                return false;

            // Assume the intention was to hit the strong hit with both keys only if the first key is still being held down
            return firstKeyHeld && UpdateJudgement(true);
        }
    }
}
