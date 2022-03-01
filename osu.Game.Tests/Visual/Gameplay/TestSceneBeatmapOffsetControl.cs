// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Screens.Play.PlayerSettings;
using osu.Game.Tests.Visual.Ranking;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneBeatmapOffsetControl : OsuTestScene
    {
        [Test]
        public void TestDisplay()
        {
            Child = new PlayerSettingsGroup("Some settings")
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new Drawable[]
                {
                    new BeatmapOffsetControl(TestSceneHitEventTimingDistributionGraph.CreateDistributedHitEvents(-4.5))
                }
            };
        }
    }
}
