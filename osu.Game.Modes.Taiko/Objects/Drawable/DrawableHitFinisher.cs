// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using System;
using System.Linq;
using osu.Framework.Input;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    public abstract class DrawableHitFinisher : DrawableHit
    {
        /// <summary>
        /// The lenience for the second key press.
        /// This does not adjust by map difficulty in ScoreV2 yet.
        /// </summary>
        private const double second_hit_window = 30;

        private double firstHitTime;
        private Key firstHitKey;

        protected DrawableHitFinisher(Hit hit)
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

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            // Check if we've handled the initial key
            if (!Judgement.Result.HasValue)
            {
                bool result = base.OnKeyDown(state, args);

                if (result)
                {
                    firstHitTime = Time.Current;
                    firstHitKey = args.Key;
                }

                return result;
            }

            // If we've already hit the second key, don't handle this object any further
            if (Judgement.SecondHit)
                return false;

            // Don't handle represses of the same key
            if (firstHitKey == args.Key)
                return false;

            // Don't handle invalid hit key presses
            if (!HitKeys.Contains(args.Key))
                return false;

            // If we're not holding the first key down still, assume the intention
            // was not to hit the finisher with both keys simultaneously
            if (!state.Keyboard.Keys.Contains(firstHitKey))
                return false;

            return UpdateJudgement(true);
        }
    }
}
