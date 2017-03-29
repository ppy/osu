// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Judgements;
using System;
using System.Collections.Generic;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    public abstract class DrawableHit : DrawableTaikoHitObject
    {
        /// <summary>
        /// A list of keys which can result in hits for this HitObject.
        /// </summary>
        protected abstract List<Key> HitKeys { get; }

        private readonly Hit hit;

        /// <summary>
        /// Whether the last key pressed is a valid hit key.
        /// </summary>
        private bool validKeyPressed;

        protected DrawableHit(Hit hit)
            : base(hit)
        {
            this.hit = hit;
        }

        protected override void CheckJudgement(bool userTriggered)
        {
            if (!userTriggered)
            {
                if (Judgement.TimeOffset > hit.HitWindowGood)
                    Judgement.Result = HitResult.Miss;
                return;
            }

            double hitOffset = Math.Abs(Judgement.TimeOffset);

            if (hitOffset > hit.HitWindowMiss)
                return;

            if (!validKeyPressed)
                Judgement.Result = HitResult.Miss;
            else if (hitOffset < hit.HitWindowGood)
            {
                Judgement.Result = HitResult.Hit;
                Judgement.TaikoResult = hitOffset < hit.HitWindowGreat ? TaikoHitResult.Great : TaikoHitResult.Good;
            }
            else
                Judgement.Result = HitResult.Miss;
        }

        protected override bool HandleKeyPress(Key key)
        {
            if (Judgement.Result.HasValue)
                return false;

            validKeyPressed = HitKeys.Contains(key);

            return UpdateJudgement(true);
        }
    }
}
