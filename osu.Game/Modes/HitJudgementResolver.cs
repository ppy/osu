//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Modes.Objects;

namespace osu.Game.Modes
{
    public class HitJudgementResolver
    {
        public JudgementResult CheckJudgement(HitObject h) => new JudgementResult { Combo = ComboJudgement.None, Judgement = Judgement.Hit300 };
    }

    public struct JudgementResult
    {
        public ComboJudgement Combo;
        public Judgement Judgement;
    }

    public enum ComboJudgement
    {
        [Description(@"")]
        None,
        [Description(@"Good")]
        Good,
        [Description(@"Amazing")]
        Perfect
    }

    public enum Judgement
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
