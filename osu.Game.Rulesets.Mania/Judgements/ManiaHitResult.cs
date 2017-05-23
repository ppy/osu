// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;

namespace osu.Game.Rulesets.Mania.Judgements
{
    public enum ManiaHitResult
    {
        [Description("PERFECT")]
        Perfect,
        [Description("GREAT")]
        Great,
        [Description("GOOD")]
        Good,
        [Description("OK")]
        Ok,
        [Description("BAD")]
        Bad
    }
}