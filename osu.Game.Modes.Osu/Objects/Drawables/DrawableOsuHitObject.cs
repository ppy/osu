// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Drawables;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    public class DrawableOsuHitObject : DrawableHitObject
    {
        public const float TIME_PREEMPT = 600;
        public const float TIME_FADEIN = 400;
        public const float TIME_FADEOUT = 500;

        public DrawableOsuHitObject(OsuHitObject hitObject)
            : base(hitObject)
        {
        }

        public override JudgementInfo CreateJudgementInfo() => new OsuJudgementInfo { MaxScore = OsuScoreResult.Hit300 };

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

    public class OsuJudgementInfo : PositionalJudgementInfo
    {
        /// <summary>
        /// The score the user achieved.
        /// </summary>
        public OsuScoreResult Score;

        /// <summary>
        /// The score which would be achievable on a perfect hit.
        /// </summary>
        public OsuScoreResult MaxScore = OsuScoreResult.Hit300;

        public int ScoreValue => scoreToInt(Score);

        public int MaxScoreValue => scoreToInt(MaxScore);

        private int scoreToInt(OsuScoreResult result)
        {
            switch (result)
            {
                default:
                    return 0;
                case OsuScoreResult.Hit50:
                    return 50;
                case OsuScoreResult.Hit100:
                    return 100;
                case OsuScoreResult.Hit300:
                    return 300;
                case OsuScoreResult.SliderTick:
                    return 10;
            }
        }

        public ComboResult Combo;
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
