// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects.Drawables;

namespace osu.Game.Modes.Judgements
{
    public class JudgementInfo
    {
        public ulong? ComboAtHit;
        public HitResult? Result;
        public double TimeOffset;
    }
}