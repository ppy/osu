// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.Modes.Objects.Drawables;
using OpenTK.Input;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Hits
{
    public abstract class DrawableHitFinisher : DrawableHit
    {
        private const double second_hit_window = 30;

        private List<Key> pressedKeys = new List<Key>();

        protected DrawableHitFinisher(TaikoHitObject hitObject)
            : base(hitObject)
        {
            Size *= 1.5f;
        }

        protected override JudgementInfo CreateJudgementInfo() => new TaikoJudgementInfo { MaxScore = TaikoScoreResult.Great, SecondHit = true };

        protected override void CheckJudgement(bool userTriggered)
        {
            TaikoJudgementInfo taikoJudgement = (TaikoJudgementInfo)Judgement;

            if (!taikoJudgement.Result.HasValue)
            {
                base.CheckJudgement(userTriggered);
                return;
            }

            double timeOffset = Time.Current - HitObject.EndTime;
            double hitOffset = Math.Abs(timeOffset - taikoJudgement.TimeOffset);

            if (!userTriggered)
                return;

            if (hitOffset < 30)
                taikoJudgement.SecondHit = true;
        }

        protected override bool HandleKeyPress(Key key)
        {
            // Don't handle re-presses of the same key
            if (pressedKeys.Contains(key))
                return false;

            bool handled = base.HandleKeyPress(key);

            // Only add to list if this HitObject handled the keypress
            if (handled)
                pressedKeys.Add(key);

            return handled;
        }
    }
}
