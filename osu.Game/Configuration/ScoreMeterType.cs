// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Configuration
{
    public enum ScoreMeterType
    {
        [Description("无")]
        None,

        [Description("打击偏差 (左)")]
        HitErrorLeft,

        [Description("打击偏差 (右)")]
        HitErrorRight,

        [Description("打击偏差 (左右)")]
        HitErrorBoth,
    }
}
