// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Setup;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneDesignSection : OsuManualInputManagerTestScene
    {
        private TestDesignSection designSection;
        private EditorBeatmap editorBeatmap { get; set; }

        [SetUpSteps]
        public void SetUp()
        {
            AddStep("create blank beatmap", () => editorBeatmap = new EditorBeatmap(new Beatmap
            {
                BeatmapInfo =
                {
                    Ruleset = new OsuRuleset().RulesetInfo
                }
            }));
            AddStep("create section", () => Child = new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies = new (Type, object)[]
                {
                    (typeof(EditorBeatmap), editorBeatmap)
                },
                Child = designSection = new TestDesignSection()
            });
        }

        [Test]
        public void TestCountdownOff()
        {
            AddStep("turn countdown off", () => designSection.EnableCountdown.Current.Value = false);

            AddAssert("beatmap has correct type", () => editorBeatmap.BeatmapInfo.Countdown == CountdownType.None);
            AddUntilStep("other controls hidden", () => !designSection.CountdownSettings.IsPresent);
        }

        [Test]
        public void TestCountdownOn()
        {
            AddStep("turn countdown on", () => designSection.EnableCountdown.Current.Value = true);

            AddAssert("beatmap has correct type", () => editorBeatmap.BeatmapInfo.Countdown == CountdownType.Normal);
            AddUntilStep("other controls shown", () => designSection.CountdownSettings.IsPresent);

            AddStep("change countdown speed", () => designSection.CountdownSpeed.Current.Value = CountdownType.DoubleSpeed);

            AddAssert("beatmap has correct type", () => editorBeatmap.BeatmapInfo.Countdown == CountdownType.DoubleSpeed);
            AddUntilStep("other controls still shown", () => designSection.CountdownSettings.IsPresent);
        }

        [Test]
        public void TestCountdownOffset()
        {
            AddStep("turn countdown on", () => designSection.EnableCountdown.Current.Value = true);

            AddAssert("beatmap has correct type", () => editorBeatmap.BeatmapInfo.Countdown == CountdownType.Normal);

            checkOffsetAfter("1", 1);
            checkOffsetAfter(string.Empty, 0);
            checkOffsetAfter("123", 123);
            checkOffsetAfter("0", 0);
        }

        private void checkOffsetAfter(string userInput, int expectedFinalValue)
        {
            AddStep("click text box", () =>
            {
                var textBox = designSection.CountdownOffset.ChildrenOfType<TextBox>().Single();
                InputManager.MoveMouseTo(textBox);
                InputManager.Click(MouseButton.Left);
            });
            AddStep("set offset text", () => designSection.CountdownOffset.Current.Value = userInput);
            AddStep("commit text", () => InputManager.Key(Key.Enter));

            AddAssert($"displayed value is {expectedFinalValue}", () => designSection.CountdownOffset.Current.Value == expectedFinalValue.ToString(CultureInfo.InvariantCulture));
            AddAssert($"beatmap value is {expectedFinalValue}", () => editorBeatmap.BeatmapInfo.CountdownOffset == expectedFinalValue);
        }

        private partial class TestDesignSection : DesignSection
        {
            public new LabelledSwitchButton EnableCountdown => base.EnableCountdown;

            public new FillFlowContainer CountdownSettings => base.CountdownSettings;
            public new LabelledEnumDropdown<CountdownType> CountdownSpeed => base.CountdownSpeed;
            public new LabelledNumberBox CountdownOffset => base.CountdownOffset;
        }
    }
}
