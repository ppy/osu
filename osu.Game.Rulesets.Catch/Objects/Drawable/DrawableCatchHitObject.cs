﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public abstract class DrawableCatchHitObject<TObject> : DrawableCatchHitObject
        where TObject : CatchBaseHit
    {
        public new TObject HitObject;

        protected DrawableCatchHitObject(TObject hitObject)
            : base(hitObject)
        {
            HitObject = hitObject;
        }
    }

    public abstract class DrawableCatchHitObject : DrawableScrollingHitObject<CatchBaseHit>
    {
        protected DrawableCatchHitObject(CatchBaseHit hitObject)
            : base(hitObject)
        {
            RelativePositionAxes = Axes.Both;
            X = hitObject.X;
            Y = (float)HitObject.StartTime;
        }

        public Func<CatchBaseHit, bool> CheckPosition;

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (timeOffset > 0)
                AddJudgement(new Judgement { Result = CheckPosition?.Invoke(HitObject) ?? false ? HitResult.Perfect : HitResult.Miss });
        }

        private const float preempt = 1000;

        protected override void UpdateState(ArmedState state)
        {
            using (BeginAbsoluteSequence(HitObject.StartTime - preempt))
            {
                // animation
                this.FadeIn(200);
            }

            switch (state)
            {
                case ArmedState.Miss:
                    using (BeginAbsoluteSequence(HitObject.StartTime, true))
                        this.FadeOut(250).RotateTo(Rotation * 2, 250, Easing.Out);
                    break;
            }
        }
    }
}
