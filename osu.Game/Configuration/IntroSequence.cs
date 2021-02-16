// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Configuration
{
    public enum IntroSequence
    {
        [Description("config.introSequence.circles")]
        Circles,

        [Description("config.introSequence.welcome")]
        Welcome,

        [Description("config.introSequence.triangles")]
        Triangles,

        [Description("config.introSequence.circles.simplifiedChinese")]
        CirclesCN,

        [Description("config.introSequence.triangles.simplifiedChinese")]
        TrianglesCN,

        [Description("config.introSequence.skipped")]
        SkippedIntro,

        [Description("config.introSequence.random")]
        Random
    }
}
