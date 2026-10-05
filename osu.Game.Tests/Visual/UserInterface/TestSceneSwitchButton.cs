// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneSwitchButton : OsuManualInputManagerTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);

        private SwitchButton switchButton;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = switchButton = new SwitchButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        });

        [Test]
        public void TestChangeThroughInput()
        {
            AddStep("move to switch button", () => InputManager.MoveMouseTo(switchButton));
            AddStep("click on", () => InputManager.Click(MouseButton.Left));
            AddStep("click off", () => InputManager.Click(MouseButton.Left));
        }

        [Test]
        public void TestChangeThroughBindable()
        {
            BindableBool bindable = null;

            AddStep("bind bindable", () => switchButton.Current.BindTo(bindable = new BindableBool()));
            AddStep("toggle bindable", () => bindable.Toggle());
            AddStep("toggle bindable", () => bindable.Toggle());
        }

        [Test]
        public void TestDisabledState()
        {
            AddToggleStep("toggle disabled", v =>
            {
                if (switchButton.IsNotNull())
                    switchButton.Current.Disabled = v;
            });
        }
    }
}
