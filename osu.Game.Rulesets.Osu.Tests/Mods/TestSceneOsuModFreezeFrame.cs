// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public partial class TestSceneOsuModFreezeFrame : OsuModTestScene
    {
        [Test]
        public void TestFreezeFrame()
        {
            CreateModTest(new ModTestData
            {
                Mod = new OsuModFreezeFrame(),
                PassCondition = () => true,
                Autoplay = false,
            });
        }

        [Test]
        public void TestSkipToFirstCircleNotSuppressed()
        {
            CreateModTest(new ModTestData
            {
                Mod = new OsuModFreezeFrame(),
                CreateBeatmap = () => new OsuBeatmap
                {
                    HitObjects =
                    {
                        new HitCircle { StartTime = 5000, Position = OsuPlayfield.BASE_SIZE / 2 }
                    }
                },
                PassCondition = () => Player.GameplayClockContainer.GameplayStartTime > 0
            });
        }

        [Test]
        public void TestSkipToFirstSpinnerNotSuppressed()
        {
            CreateModTest(new ModTestData
            {
                Mod = new OsuModFreezeFrame(),
                CreateBeatmap = () => new OsuBeatmap
                {
                    HitObjects =
                    {
                        new Spinner { StartTime = 5000, Position = OsuPlayfield.BASE_SIZE / 2 }
                    }
                },
                PassCondition = () => Player.GameplayClockContainer.GameplayStartTime > 0
            });
        }
    }
}
