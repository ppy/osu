// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneOsuDropdown : ThemeComparisonTestScene
    {
        protected override Drawable CreateContent() =>
            new OsuEnumDropdown<TestEnum>
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 150
            };

        private enum TestEnum
        {
            [Description("Option")]
            Option,

            [Description("Really lonnnnnnng option")]
            ReallyLongOption,
        }
    }
}
