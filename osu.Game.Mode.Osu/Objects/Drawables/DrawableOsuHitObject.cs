using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Drawables;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    public class DrawableOsuHitObject : DrawableHitObject
    {
        protected const float TIME_PREEMPT = 600;
        protected const float TIME_FADEIN = 400;
        protected const float TIME_FADEOUT = 500;

        public DrawableOsuHitObject(OsuHitObject hitObject)
            : base(hitObject)
        {
        }

        public override JudgementInfo CreateJudgementInfo() => new OsuJudgementInfo();

        protected override void UpdateState(ArmedState state)
        {
            if (!IsLoaded) return;

            Flush(true);

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
        public OsuScoreResult Score;
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
        [Description(@"500")]
        Hit500
    }
}
