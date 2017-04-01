// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Judgements;
using System;
using System.Linq;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    public abstract class DrawableHit : DrawableTaikoHitObject
    {
        /// <summary>
        /// A list of keys which can result in hits for this HitObject.
        /// </summary>
        protected abstract Key[] HitKeys { get; }

        protected override Container<Framework.Graphics.Drawable> Content => bodyContainer;

        private readonly Hit hit;

        /// <summary>
        /// Whether the last key pressed is a valid hit key.
        /// </summary>
        private bool validKeyPressed;

        private readonly Container bodyContainer;

        protected DrawableHit(Hit hit)
            : base(hit)
        {
            this.hit = hit;

            AddInternal(bodyContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
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
            if (Judgement.Result != HitResult.None)
                return false;

            validKeyPressed = HitKeys.Contains(key);

            return UpdateJudgement(true);
        }

        protected override void UpdateState(ArmedState state)
        {
            Delay(HitObject.StartTime - Time.Current + Judgement.TimeOffset, true);

            switch (State)
            {
                case ArmedState.Idle:
                    Delay(hit.HitWindowMiss);
                    break;
                case ArmedState.Miss:
                    FadeOut(100);
                    break;
                case ArmedState.Hit:
                    bodyContainer.ScaleTo(0.8f, 400, EasingTypes.OutQuad);
                    bodyContainer.MoveToY(-200, 250, EasingTypes.Out);
                    bodyContainer.Delay(250);
                    bodyContainer.MoveToY(0, 500, EasingTypes.In);

                    FadeOut(600);
                    break;
            }

            Expire();
        }
    }
}
