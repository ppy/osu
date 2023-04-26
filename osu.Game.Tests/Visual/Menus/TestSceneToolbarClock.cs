// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Timing;
using osu.Game.Configuration;
using osu.Game.Overlays.Toolbar;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Menus
{
    [TestFixture]
    public partial class TestSceneToolbarClock : OsuManualInputManagerTestScene
    {
        private Bindable<ToolbarClockDisplayMode> clockDisplayMode;

        private readonly Container mainContainer;
        private readonly ToolbarClock toolbarClock;

        public TestSceneToolbarClock()
        {
            Children = new Drawable[]
            {
                mainContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Height = Toolbar.HEIGHT,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4.Black,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Y,
                            AutoSizeAxes = Axes.X,
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = Color4.DarkRed,
                                    RelativeSizeAxes = Axes.Y,
                                    Width = 2,
                                },
                                toolbarClock = new ToolbarClock(),
                                new Box
                                {
                                    Colour = Color4.DarkRed,
                                    RelativeSizeAxes = Axes.Y,
                                    Width = 2,
                                },
                            }
                        },
                    }
                },
            };

            AddSliderStep("scale", 0.5, 4, 1, scale => mainContainer.Scale = new Vector2((float)scale));
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            clockDisplayMode = config.GetBindable<ToolbarClockDisplayMode>(OsuSetting.ToolbarClockDisplayMode);
        }

        [Test]
        public void TestRealGameTime()
        {
            AddStep("Set game time real", () => mainContainer.Clock = Clock);
        }

        [Test]
        public void TestLongGameTime()
        {
            AddStep("Set game time long", () => mainContainer.Clock = new FramedOffsetClock(Clock, false) { Offset = 3600.0 * 24 * 1000 * 98 });
        }

        [Test]
        public void TestDisplayModeChange()
        {
            AddStep("Set clock display mode", () => clockDisplayMode.Value = ToolbarClockDisplayMode.Full);

            AddStep("Trigger click", () => toolbarClock.TriggerClick());
            AddAssert("State is digital with runtime", () => clockDisplayMode.Value == ToolbarClockDisplayMode.DigitalWithRuntime);
            AddStep("Trigger click", () => toolbarClock.TriggerClick());
            AddAssert("State is digital", () => clockDisplayMode.Value == ToolbarClockDisplayMode.Digital);
            AddStep("Trigger click", () => toolbarClock.TriggerClick());
            AddAssert("State is analog", () => clockDisplayMode.Value == ToolbarClockDisplayMode.Analog);
            AddStep("Trigger click", () => toolbarClock.TriggerClick());
            AddAssert("State is full", () => clockDisplayMode.Value == ToolbarClockDisplayMode.Full);
        }
    }
}
