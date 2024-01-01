// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
    public partial class TestSceneUserDimContainer : OsuTestScene
    {
        private TestUserDimContainer userDimContainer;

        private readonly BindableBool isBreakTime = new BindableBool();

        private Bindable<bool> lightenDuringBreaks = new Bindable<bool>();

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            lightenDuringBreaks = config.GetBindable<bool>(OsuSetting.LightenDuringBreaks);
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = userDimContainer = new TestUserDimContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new Box
                {
                    Colour = Color4.White,
                    RelativeSizeAxes = Axes.Both,
                },
            };

            userDimContainer.IsBreakTime.BindTo(isBreakTime);
            isBreakTime.Value = false;

            lightenDuringBreaks.Value = false;
        });

        private const float test_user_dim = 0.6f;
        private const float test_user_dim_lightened = test_user_dim - UserDimContainer.LIGHTEN_AMOUNT;

        [TestCase(test_user_dim, test_user_dim_lightened)]
        [TestCase(0.2f, 0.0f)]
        [TestCase(0.0f, 0.0f)]
        public void TestBreakLightening(float userDim, float expectedBreakDim)
        {
            AddStep($"set dim level {userDim}", () => userDimContainer.UserDimLevel.Value = userDim);
            AddStep("set lighten during break", () => lightenDuringBreaks.Value = true);

            AddStep("set break", () => isBreakTime.Value = true);
            AddUntilStep("has lightened", () => userDimContainer.DimEqual(expectedBreakDim));
            AddStep("clear break", () => isBreakTime.Value = false);
            AddUntilStep("not lightened", () => userDimContainer.DimEqual(userDim));
        }

        [Test]
        public void TestEnableSettingDuringBreak()
        {
            AddStep("set dim level 0.6", () => userDimContainer.UserDimLevel.Value = test_user_dim);

            AddStep("set break", () => isBreakTime.Value = true);
            AddUntilStep("not lightened", () => userDimContainer.DimEqual(test_user_dim));
            AddStep("set lighten during break", () => lightenDuringBreaks.Value = true);
            AddUntilStep("has lightened", () => userDimContainer.DimEqual(test_user_dim_lightened));
        }

        [Test]
        public void TestDisableSettingDuringBreak()
        {
            AddStep("set dim level 0.6", () => userDimContainer.UserDimLevel.Value = test_user_dim);
            AddStep("set lighten during break", () => lightenDuringBreaks.Value = true);

            AddStep("set break", () => isBreakTime.Value = true);
            AddUntilStep("has lightened", () => userDimContainer.DimEqual(test_user_dim_lightened));
            AddStep("clear lighten during break", () => lightenDuringBreaks.Value = false);
            AddUntilStep("not lightened", () => userDimContainer.DimEqual(test_user_dim));
            AddStep("clear break", () => isBreakTime.Value = false);
            AddUntilStep("not lightened", () => userDimContainer.DimEqual(test_user_dim));
        }

        [Test]
        public void TestIgnoreUserSettings()
        {
            AddStep("set dim level 0.6", () => userDimContainer.UserDimLevel.Value = test_user_dim);
            AddUntilStep("dim reached", () => userDimContainer.DimEqual(test_user_dim));

            AddStep("ignore settings", () => userDimContainer.IgnoreUserSettings.Value = true);
            AddUntilStep("no dim", () => userDimContainer.DimEqual(0));
            AddStep("set break", () => isBreakTime.Value = true);
            AddAssert("no dim", () => userDimContainer.DimEqual(0));
            AddStep("clear break", () => isBreakTime.Value = false);
            AddAssert("no dim", () => userDimContainer.DimEqual(0));
        }

        private partial class TestUserDimContainer : UserDimContainer
        {
            public bool DimEqual(float expectedDimLevel) => Content.Colour == OsuColour.Gray(1f - expectedDimLevel);

            public new Bindable<double> UserDimLevel => base.UserDimLevel;
        }
    }
}
