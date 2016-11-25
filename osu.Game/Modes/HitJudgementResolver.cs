//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Drawables;
using OpenTK;

namespace osu.Game.Modes
{
    public class HitJudgementResolver
    {
        public virtual void CheckJudgement(DrawableHitObject h, JudgementInfo info)
        {
            info.Result = HitResult.Hit300;
        }
    }

    public class JudgementInfo
    {
        public bool UserTriggered;
        public ComboResult Combo;
        public HitResult Result;
        public double TimeOffset;
        public Vector2 PositionOffset;
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

    public enum HitResult
    {
        Ignore,
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
