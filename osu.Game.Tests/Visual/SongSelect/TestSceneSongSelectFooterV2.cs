// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Select.FooterV2;
using osuTK.Input;

namespace osu.Game.Tests.Visual.SongSelect
{
    public partial class TestSceneSongSelectFooterV2 : OsuManualInputManagerTestScene
    {
        private FooterButtonRandomV2 randomButton = null!;
        private FooterButtonModsV2 modsButton = null!;

        private bool nextRandomCalled;
        private bool previousRandomCalled;

        private DummyOverlay overlay = null!;

        [Cached]
        private OverlayColourProvider colourProvider { get; set; } = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            nextRandomCalled = false;
            previousRandomCalled = false;

            FooterV2 footer;

            Children = new Drawable[]
            {
                new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = footer = new FooterV2(),
                },
                overlay = new DummyOverlay()
            };

            footer.AddButton(modsButton = new FooterButtonModsV2 { Current = SelectedMods }, overlay);
            footer.AddButton(randomButton = new FooterButtonRandomV2
            {
                NextRandom = () => nextRandomCalled = true,
                PreviousRandom = () => previousRandomCalled = true
            });
            footer.AddButton(new FooterButtonOptionsV2());

            overlay.Hide();
        });

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("clear mods", () => SelectedMods.Value = Array.Empty<Mod>());
            AddStep("set beatmap", () => Beatmap.Value = CreateWorkingBeatmap(CreateBeatmap(new OsuRuleset().RulesetInfo)));
        }

        [Test]
        public void TestMods()
        {
            AddStep("one mod", () => SelectedMods.Value = new List<Mod> { new OsuModHidden() });
            AddStep("two mods", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModHardRock() });
            AddStep("three mods", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime() });
            AddStep("four mods", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime(), new OsuModClassic() });
            AddStep("five mods", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime(), new OsuModClassic(), new OsuModDifficultyAdjust() });

            AddStep("modified", () => SelectedMods.Value = new List<Mod> { new OsuModDoubleTime { SpeedChange = { Value = 1.2 } } });
            AddStep("modified + one", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModDoubleTime { SpeedChange = { Value = 1.2 } } });
            AddStep("modified + two", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime { SpeedChange = { Value = 1.2 } } });
            AddStep("modified + three", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModClassic(), new OsuModDoubleTime { SpeedChange = { Value = 1.2 } } });
            AddStep("modified + four", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModClassic(), new OsuModDifficultyAdjust(), new OsuModDoubleTime { SpeedChange = { Value = 1.2 } } });

            AddStep("clear mods", () => SelectedMods.Value = Array.Empty<Mod>());
            AddWaitStep("wait", 3);
            AddStep("one mod", () => SelectedMods.Value = new List<Mod> { new OsuModHidden() });

            AddStep("clear mods", () => SelectedMods.Value = Array.Empty<Mod>());
            AddWaitStep("wait", 3);
            AddStep("five mods", () => SelectedMods.Value = new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime(), new OsuModClassic(), new OsuModDifficultyAdjust() });
        }

        [Test]
        public void TestShowOptions()
        {
            AddStep("enable options", () =>
            {
                var optionsButton = this.ChildrenOfType<FooterButtonV2>().Last();

                optionsButton.Enabled.Value = true;
                optionsButton.TriggerClick();
            });
        }

        [Test]
        public void TestState()
        {
            AddToggleStep("set options enabled state", state => this.ChildrenOfType<FooterButtonV2>().Last().Enabled.Value = state);
        }

        [Test]
        public void TestFooterRandom()
        {
            AddStep("press F2", () => InputManager.Key(Key.F2));
            AddAssert("next random invoked", () => nextRandomCalled && !previousRandomCalled);
        }

        [Test]
        public void TestFooterRandomViaMouse()
        {
            AddStep("click button", () =>
            {
                InputManager.MoveMouseTo(randomButton);
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("next random invoked", () => nextRandomCalled && !previousRandomCalled);
        }

        [Test]
        public void TestFooterRewind()
        {
            AddStep("press Shift+F2", () =>
            {
                InputManager.PressKey(Key.LShift);
                InputManager.PressKey(Key.F2);
                InputManager.ReleaseKey(Key.F2);
                InputManager.ReleaseKey(Key.LShift);
            });
            AddAssert("previous random invoked", () => previousRandomCalled && !nextRandomCalled);
        }

        [Test]
        public void TestFooterRewindViaShiftMouseLeft()
        {
            AddStep("shift + click button", () =>
            {
                InputManager.PressKey(Key.LShift);
                InputManager.MoveMouseTo(randomButton);
                InputManager.Click(MouseButton.Left);
                InputManager.ReleaseKey(Key.LShift);
            });
            AddAssert("previous random invoked", () => previousRandomCalled && !nextRandomCalled);
        }

        [Test]
        public void TestFooterRewindViaMouseRight()
        {
            AddStep("right click button", () =>
            {
                InputManager.MoveMouseTo(randomButton);
                InputManager.Click(MouseButton.Right);
            });
            AddAssert("previous random invoked", () => previousRandomCalled && !nextRandomCalled);
        }

        [Test]
        public void TestOverlayPresent()
        {
            AddStep("Press F1", () =>
            {
                InputManager.MoveMouseTo(modsButton);
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("Overlay visible", () => overlay.State.Value == Visibility.Visible);
            AddStep("Hide", () => overlay.Hide());
        }

        private partial class DummyOverlay : ShearedOverlayContainer
        {
            public DummyOverlay()
                : base(OverlayColourScheme.Green)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Header.Title = "An overlay";
            }
        }
    }
}
