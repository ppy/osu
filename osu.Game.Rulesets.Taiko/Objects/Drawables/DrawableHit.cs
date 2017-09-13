// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public abstract class DrawableHit : DrawableTaikoHitObject<Hit>
    {
        /// <summary>
        /// A list of keys which can result in hits for this HitObject.
        /// </summary>
        protected abstract TaikoAction[] HitActions { get; }

        /// <summary>
        /// Whether the last key pressed is a valid hit key.
        /// </summary>
        private bool validKeyPressed;

        protected DrawableHit(Hit hit)
            : base(hit)
        {
            FillMode = FillMode.Fit;
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (!userTriggered)
            {
                if (timeOffset > HitObject.HitWindowGood)
                    AddJudgement(new TaikoJudgement { Result = HitResult.Miss });
                return;
            }

            double hitOffset = Math.Abs(timeOffset);

            if (hitOffset > HitObject.HitWindowMiss)
                return;

            if (!validKeyPressed)
                AddJudgement(new TaikoJudgement { Result = HitResult.Miss });
            else if (hitOffset < HitObject.HitWindowGood)
                AddJudgement(new TaikoJudgement { Result = hitOffset < HitObject.HitWindowGreat ? HitResult.Great : HitResult.Good });
            else
                AddJudgement(new TaikoJudgement { Result = HitResult.Miss });
        }

        public override bool OnPressed(TaikoAction action)
        {
            validKeyPressed = HitActions.Contains(action);

            return UpdateJudgement(true);
        }

        protected override void Update()
        {
            base.Update();

            Size = BaseSize * Parent.RelativeChildSize;
        }

        protected override void UpdateState(ArmedState state)
        {
            var circlePiece = MainPiece as CirclePiece;
            circlePiece?.FlashBox.FinishTransforms();

            var offset = !AllJudged ? 0 : Time.Current - HitObject.StartTime;
            using (BeginDelayedSequence(HitObject.StartTime - Time.Current + offset, true))
            {
                switch (State)
                {
                    case ArmedState.Idle:
                        this.Delay(HitObject.HitWindowMiss).Expire();
                        break;
                    case ArmedState.Miss:
                        this.FadeOut(100)
                            .Expire();
                        break;
                    case ArmedState.Hit:
                        var flash = circlePiece?.FlashBox;
                        if (flash != null)
                        {
                            flash.FadeTo(0.9f);
                            flash.FadeOut(300);
                        }

                        const float gravity_time = 300;
                        const float gravity_travel_height = 200;

                        Content.ScaleTo(0.8f, gravity_time * 2, Easing.OutQuad);

                        this.MoveToY(-gravity_travel_height, gravity_time, Easing.Out)
                            .Then()
                            .MoveToY(gravity_travel_height * 2, gravity_time * 2, Easing.In);

                        this.FadeOut(800)
                            .Expire();

                        break;
                }
            }
        }
    }
}
