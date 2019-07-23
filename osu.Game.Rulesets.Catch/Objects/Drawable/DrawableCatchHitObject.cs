// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Objects.Drawable
{
    public abstract class PalpableCatchHitObject<TObject> : DrawableCatchHitObject<TObject>
        where TObject : CatchHitObject
    {
        public override bool CanBePlated => true;

        protected PalpableCatchHitObject(TObject hitObject)
            : base(hitObject)
        {
            Scale = new Vector2(HitObject.Scale);
        }
    }

    public abstract class DrawableCatchHitObject<TObject> : DrawableCatchHitObject
        where TObject : CatchHitObject
    {
        public new TObject HitObject;

        protected DrawableCatchHitObject(TObject hitObject)
            : base(hitObject)
        {
            HitObject = hitObject;
            Anchor = Anchor.BottomLeft;
        }
    }

    public abstract class DrawableCatchHitObject : DrawableHitObject<CatchHitObject>
    {
        public virtual bool CanBePlated => false;

        public virtual bool StaysOnPlate => CanBePlated;

        protected DrawableCatchHitObject(CatchHitObject hitObject)
            : base(hitObject)
        {
            RelativePositionAxes = Axes.X;
            X = hitObject.X;
        }

        public Func<CatchHitObject, bool> CheckPosition;

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (CheckPosition == null) return;

            if (timeOffset >= 0 && Result != null)
                ApplyResult(r => r.Type = CheckPosition.Invoke(HitObject) ? HitResult.Perfect : HitResult.Miss);
        }

        protected override bool UseTransformStateManagement => false;

        protected override void UpdateState(ArmedState state)
        {
            // TODO: update to use new state management.
            using (BeginAbsoluteSequence(HitObject.StartTime - HitObject.TimePreempt))
                this.FadeIn(200);

            var endTime = (HitObject as IHasEndTime)?.EndTime ?? HitObject.StartTime;

            using (BeginAbsoluteSequence(endTime, true))
            {
                switch (state)
                {
                    case ArmedState.Miss:
                        this.FadeOut(250).RotateTo(Rotation * 2, 250, Easing.Out).Expire();
                        break;

                    case ArmedState.Hit:
                        this.FadeOut().Expire();
                        break;
                }
            }
        }
    }
}
