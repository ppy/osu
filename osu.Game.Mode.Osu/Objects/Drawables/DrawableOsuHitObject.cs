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
        public DrawableOsuHitObject(OsuHitObject hitObject)
            : base(hitObject)
        {
        }

        public override JudgementInfo CreateJudgementInfo() => new OsuJudgementInfo();

        protected override void UpdateState(ArmedState state)
        {
            throw new NotImplementedException();
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
