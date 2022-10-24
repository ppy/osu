// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Screens.Select;
using osuTK;

namespace osu.Game.Tests.Visual.SongSelect
{
    public class TestSceneDifficultyRangeFilterControl : OsuTestScene
    {
        [Test]
        public void TestBasic()
        {
            AddStep("create control", () =>
            {
                Child = new DifficultyRangeFilterControl
                {
                    Width = 200,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(3),
                };
            });
        }
    }
}
