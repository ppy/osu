// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osuTK;

namespace osu.Game.Tests.Visual.Settings
{
    [TestFixture]
    public class TestSceneSettingsSource : OsuTestScene
    {
        public TestSceneSettingsSource()
        {
            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(20),
                    Width = 0.5f,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Padding = new MarginPadding(50),
                    ChildrenEnumerable = new TestTargetClass().CreateSettingsControls()
                },
            };
        }

        private class TestTargetClass
        {
            [SettingSource("Sample bool", "Clicking this changes a setting")]
            public BindableBool TickBindable { get; } = new BindableBool();

            [SettingSource("Sample float", "Change something for a mod")]
            public BindableFloat SliderBindable { get; } = new BindableFloat
            {
                MinValue = 0,
                MaxValue = 10,
                Default = 5,
                Value = 7
            };

            [SettingSource("Sample enum", "Change something for a mod")]
            public Bindable<TestEnum> EnumBindable { get; } = new Bindable<TestEnum>
            {
                Default = TestEnum.Value1,
                Value = TestEnum.Value2
            };

            [SettingSource("Sample string", "Change something for a mod")]
            public Bindable<string> StringBindable { get; } = new Bindable<string>
            {
                Default = string.Empty,
                Value = "Sample text"
            };

            [SettingSource("Sample number textbox", "Textbox number entry", SettingControlType = typeof(SettingsNumberBox))]
            public Bindable<int?> IntTextBoxBindable { get; } = new Bindable<int?>
            {
                Default = null,
                Value = null
            };
        }

        private enum TestEnum
        {
            Value1,
            Value2
        }
    }
}
