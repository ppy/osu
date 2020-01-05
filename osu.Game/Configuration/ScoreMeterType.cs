// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Configuration
{
    public enum ScoreMeterType
    {
        [Description("不显示")]
        None,

        [Description("打击误差 (左侧)")]
        HitErrorLeft,

        [Description("打击误差 (右侧)")]
        HitErrorRight,

        [Description("打击误差 (左右侧)")]
        HitErrorBoth,
    }
}
