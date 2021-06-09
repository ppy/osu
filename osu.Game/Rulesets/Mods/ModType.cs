// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Rulesets.Mods
{
    public enum ModType
    {
        [Description("降低难度")]
        DifficultyReduction,

        [Description("增加难度")]
        DifficultyIncrease,

        [Description("自定义")]
        Conversion,

        [Description("自动化")]
        Automation,

        [Description("娱乐")]
        Fun,

        [Description("系统")]
        System
    }
}
