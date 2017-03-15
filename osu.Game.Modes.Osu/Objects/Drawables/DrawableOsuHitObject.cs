﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Judgements;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    public class DrawableOsuHitObject : DrawableHitObject<OsuHitObject, OsuJudgementInfo>
    {
        public const float TIME_PREEMPT = 600;
        public const float TIME_FADEIN = 400;
        public const float TIME_FADEOUT = 500;

        public DrawableOsuHitObject(OsuHitObject hitObject)
            : base(hitObject)
        {
        }

        protected override OsuJudgementInfo CreateJudgementInfo() => new OsuJudgementInfo { MaxScore = OsuScoreResult.Hit300 };

        protected override void UpdateState(ArmedState state)
        {
            if (!IsLoaded) return;

            Flush();

            UpdateInitialState();

            Delay(HitObject.StartTime - Time.Current - TIME_PREEMPT + Judgement.TimeOffset, true);

            UpdatePreemptState();

            Delay(TIME_PREEMPT, true);
        }

        protected virtual void UpdatePreemptState()
        {
            FadeIn(TIME_FADEIN);
        }

        protected virtual void UpdateInitialState()
        {
            Alpha = 0;
        }
    }

    public enum ComboResult
    {
        [Description(@"")]
        None,
        [Description(@"Good")]
        Good,
        [Description(@"Amazing")]
        Perfect
    }

    public enum OsuScoreResult
    {
        [Description(@"Miss")]
        Miss,
        [Description(@"50")]
        Hit50,
        [Description(@"100")]
        Hit100,
        [Description(@"300")]
        Hit300,
        [Description(@"10")]
        SliderTick
    }
}
