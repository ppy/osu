﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces;
using OpenTK.Input;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public abstract class DrawableHit : DrawableTaikoHitObject<Hit>
    {
        /// <summary>
        /// A list of keys which can result in hits for this HitObject.
        /// </summary>
        protected abstract Key[] HitKeys { get; }

        /// <summary>
        /// Whether the last key pressed is a valid hit key.
        /// </summary>
        private bool validKeyPressed;

        protected DrawableHit(Hit hit)
            : base(hit)
        {
        }

        protected override void CheckJudgement(bool userTriggered)
        {
            if (!userTriggered)
            {
                if (Judgement.TimeOffset > HitObject.HitWindowGood)
                    Judgement.Result = HitResult.Miss;
                return;
            }

            double hitOffset = Math.Abs(Judgement.TimeOffset);

            if (hitOffset > HitObject.HitWindowMiss)
                return;

            if (!validKeyPressed)
                Judgement.Result = HitResult.Miss;
            else if (hitOffset < HitObject.HitWindowGood)
            {
                Judgement.Result = HitResult.Hit;
                Judgement.TaikoResult = hitOffset < HitObject.HitWindowGreat ? TaikoHitResult.Great : TaikoHitResult.Good;
            }
            else
                Judgement.Result = HitResult.Miss;
        }

        protected override bool HandleKeyPress(Key key)
        {
            if (Judgement.Result != HitResult.None)
                return false;

            validKeyPressed = HitKeys.Contains(key);

            return UpdateJudgement(true);
        }

        protected override void UpdateState(ArmedState state)
        {
            var circlePiece = MainPiece as CirclePiece;
            circlePiece?.FlashBox.FinishTransforms();

            using (BeginDelayedSequence(HitObject.StartTime - Time.Current + Judgement.TimeOffset, true))
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

                        this.FadeOut(800)
                            .MoveToY(-gravity_travel_height, gravity_time, Easing.Out)
                            .Then()
                            .MoveToY(gravity_travel_height * 2, gravity_time * 2, Easing.In);

                        Expire();
                        break;
                }
            }
        }
    }
}
