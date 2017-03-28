// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
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

        protected override Container<Framework.Graphics.Drawable> Content => bodyContainer;

        private readonly Hit hit;

        /// <summary>
        /// Whether the last key pressed is a valid hit key.
        /// </summary>
        private bool validKeyPressed;

        private Container bodyContainer;

        protected DrawableHit(Hit hit)
            : base(hit)
        {
            this.hit = hit;

            AddInternal(bodyContainer = new Container
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
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
            if (Judgement.Result.HasValue)
                return false;

            validKeyPressed = HitKeys.Contains(key);

            return UpdateJudgement(true);
        }

        protected override void UpdateState(ArmedState state)
        {
            switch (State)
            {
                case ArmedState.Idle:
                    break;
                case ArmedState.Miss:
                    bodyContainer.FadeOut(100);
                    break;
                case ArmedState.Hit:
                    bodyContainer.ScaleTo(0.8f, 400, EasingTypes.OutQuad);
                    bodyContainer.FadeOut(600, EasingTypes.OutQuint);
                    bodyContainer.MoveToY(-200, 250, EasingTypes.Out);

                    bodyContainer.Delay(250);

                    bodyContainer.MoveToY(0, 500, EasingTypes.In);
                    break;
            }
        }
    }
}
