// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable
using System.ComponentModel;

namespace osu.Game.Configuration
{
    public enum IntroSequence
    {
        [Description("圆周")]
        Circles,

        [Description("怀旧")]
        Welcome,

        [Description("三角")]
        Triangles,

        [Description("圆周（中文）")]
        CirclesCN,

        [Description("三角（中文）")]
        TrianglesCN,

        [Description("略过开场")]
        SkippedIntro,

        [Description("随机")]
        Random
    }
}
