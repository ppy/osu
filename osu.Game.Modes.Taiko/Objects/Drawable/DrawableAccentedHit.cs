// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using System;
using System.Linq;
using osu.Framework.Input;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    public abstract class DrawableAccentedHit : DrawableHit
    {
        /// <summary>
        /// The lenience for the second key press.
        /// This does not adjust by map difficulty in ScoreV2 yet.
        /// </summary>
        private const double second_hit_window = 30;

        private double firstHitTime;
        private bool firstKeyHeld;
        private Key firstHitKey;

        protected DrawableAccentedHit(Hit hit)
            : base(hit)
        {
        }

        protected override void CheckJudgement(bool userTriggered)
        {
            if (!Judgement.Result.HasValue)
            {
                base.CheckJudgement(userTriggered);
                return;
            }

            if (!userTriggered)
                return;

            // If we get here, we're assured that the key pressed is the correct secondary key

            if (Math.Abs(firstHitTime - Time.Current) < second_hit_window)
                Judgement.SecondHit = true;
        }

        protected override bool HandleKeyPress(Key key)
        {
            // Check if we've handled the first key
            if (!Judgement.Result.HasValue)
            {
                // First key hasn't been handled yet, attempt to handle it
                bool handled = base.HandleKeyPress(key);

                if (handled)
                {
                    firstHitTime = Time.Current;
                    firstHitKey = key;
                }

                return handled;
            }

            // If we've already hit the second key, don't handle this object any further
            if (Judgement.SecondHit)
                return false;

            // Don't handle represses of the first key
            if (firstHitKey == key)
                return false;

            // Don't handle invalid hit key presses
            if (!HitKeys.Contains(key))
                return false;

            // Assume the intention was to hit the accented hit with both keys only if the first key is still being held down
            return firstKeyHeld && UpdateJudgement(true);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            firstKeyHeld = state.Keyboard.Keys.Contains(firstHitKey);

            return base.OnKeyDown(state, args);
        }
    }
}
