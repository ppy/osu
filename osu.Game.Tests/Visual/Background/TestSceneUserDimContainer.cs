// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Background
{
    public class TestSceneUserDimContainer : OsuTestScene
    {
        private readonly TestUserDimContainer container;
        private readonly BindableBool isBreakTime = new BindableBool();
        private readonly Bindable<bool> lightenDuringBreaks = new Bindable<bool>();

        public TestSceneUserDimContainer()
        {
            Add(container = new TestUserDimContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new Box
                {
                    Colour = Color4.White,
                    RelativeSizeAxes = Axes.Both,
                },
            });

            container.IsBreakTime.BindTo(isBreakTime);
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.LightenDuringBreaks, lightenDuringBreaks);
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            isBreakTime.Value = false;
            lightenDuringBreaks.Value = false;
        });

        [TestCase(0.6f, 0.3f)]
        [TestCase(0.2f, 0.0f)]
        [TestCase(0.0f, 0.0f)]
        public void TestBreakLightening(float userDim, float expectedBreakDim)
        {
            AddStep($"set dim level {userDim}", () => container.UserDimLevel.Value = userDim);

            AddStep("set break", () => isBreakTime.Value = true);
            AddWaitStep("wait for dim", 5);
            AddAssert($"is current dim {userDim}", () => container.DimEqual(userDim));

            AddStep("set lighten during break", () => lightenDuringBreaks.Value = true);
            AddWaitStep("wait for dim", 5);
            AddAssert($"is current dim {expectedBreakDim}", () => container.DimEqual(expectedBreakDim));

            AddStep("clear lighten during break", () => lightenDuringBreaks.Value = false);
            AddWaitStep("wait for dim", 5);
            AddAssert($"is current dim {userDim}", () => container.DimEqual(userDim));

            AddStep("clear break", () => isBreakTime.Value = false);
            AddStep("set lighten during break", () => lightenDuringBreaks.Value = true);
            AddWaitStep("wait for dim", 5);
            AddAssert($"is current dim {userDim}", () => container.DimEqual(userDim));
        }

        private class TestUserDimContainer : UserDimContainer
        {
            public bool DimEqual(float expectedDimLevel) => Content.Colour == OsuColour.Gray(1f - expectedDimLevel);

            public new Bindable<double> UserDimLevel => base.UserDimLevel;
        }
    }
}
