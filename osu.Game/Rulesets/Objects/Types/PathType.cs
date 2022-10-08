// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable
using System.ComponentModel;

namespace osu.Game.Rulesets.Objects.Types
{
    public enum PathType
    {
        Catmull,
        [Description("贝塞尔曲线")]
        Bezier,
        [Description("线性曲线")]
        Linear,
        [Description("完美曲线")]
        PerfectCurve
    }
}
