// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Configuration
{
    public enum IntroSequence
    {
        [Description("config.introSequence.circles圆周")]
        Circles,

        [Description("config.introSequence.welcome")]
        Welcome,

        [Description("config.introSequence.triangles")]
        Triangles,

        [Description("圆周(中文)")]
        CirclesCN,

        [Description("三角(中文)")]
        TrianglesCN,

        [Description("略过开场")]
        SkippedIntro,

        [Description("随机")]
        Random
    }
}
