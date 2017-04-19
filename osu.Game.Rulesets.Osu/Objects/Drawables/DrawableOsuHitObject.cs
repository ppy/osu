// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Judgements;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableOsuHitObject : DrawableHitObject<OsuHitObject, OsuJudgement>
    {
        public const float TIME_PREEMPT = 600;
        public const float TIME_FADEIN = 400;
        public const float TIME_FADEOUT = 500;

        protected DrawableOsuHitObject(OsuHitObject hitObject)
            : base(hitObject)
        {
            AccentColour = HitObject.ComboColour;
        }

        protected override OsuJudgement CreateJudgement() => new OsuJudgement { MaxScore = OsuScoreResult.Hit300 };

        protected override void UpdateState(ArmedState state)
        {
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
