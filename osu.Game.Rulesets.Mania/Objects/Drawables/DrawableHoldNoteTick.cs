// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    public class DrawableHoldNoteTick : DrawableManiaHitObject<HoldNoteTick>
    {
        public Func<double> HoldStartTime;
        public Func<bool> IsHolding;

        public DrawableHoldNoteTick(HoldNoteTick hitObject)
            : base(hitObject, null)
        {
            RelativeSizeAxes = Axes.X;

            Children = new[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 1
                }
            };
        }

        protected override ManiaJudgement CreateJudgement() => new HoldNoteTickJudgement();

        protected override void CheckJudgement(bool userTriggered)
        {
            if (!userTriggered)
                return;

            if (Time.Current < HitObject.StartTime)
                return;


            if (HoldStartTime?.Invoke() > HitObject.StartTime)
                return;

            Judgement.ManiaResult = ManiaHitResult.Perfect;
            Judgement.Result = HitResult.Hit;
        }

        protected override void UpdateState(ArmedState state)
        {
            switch (State)
            {
                case ArmedState.Hit:
                    Colour = Color4.Green;
                    break;
            }
        }

        protected override void Update()
        {
            if (Judgement.Result != HitResult.None)
                return;

            if (IsHolding?.Invoke() != true)
                return;

            UpdateJudgement(true);
        }
    }
}