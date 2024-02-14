// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneLabelledDropdown : OsuTestScene
    {
        [Test]
        public void TestLabelledDropdown()
            => AddStep(@"create dropdown", () => Child = new LabelledDropdown<string>
            {
                Label = @"Countdown speed",
                Items = new[]
                {
                    @"Half",
                    @"Normal",
                    @"Double"
                },
                Description = @"This is a description"
            });

        [Test]
        public void TestLabelledEnumDropdown()
            => AddStep(@"create dropdown", () => Child = new LabelledEnumDropdown<BeatmapOnlineStatus>
            {
                Label = @"Beatmap status",
                Description = @"This is a description"
            });
    }
}
