// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Menu;
using osu.Game.Screens.SelectV2.Footer;
using osu.Game.Tests.Resources;
using osuTK.Input;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneSongSelectV2 : ScreenTestScene
    {
        [Cached]
        private readonly ScreenFooter screenScreenFooter;

        [Cached]
        private readonly OsuLogo logo;

        private BeatmapManager manager = null!;
        private RulesetStore rulesets = null!;

        public TestSceneSongSelectV2()
        {
            Children = new Drawable[]
            {
                new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = screenScreenFooter = new ScreenFooter
                    {
                        OnBack = () => Stack.CurrentScreen.Exit(),
                    },
                },
                logo = new OsuLogo
                {
                    Alpha = 0f,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            // These DI caches are required to ensure for interactive runs this test scene doesn't nuke all user beatmaps in the local install.
            // At a point we have isolated interactive test runs enough, this can likely be removed.
            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(Realm);
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, Realm, null, audio, Resources, host, Beatmap.Default));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Stack.ScreenPushed += updateFooter;
            Stack.ScreenExited += updateFooter;
        }

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("load screen", () => Stack.Push(new Screens.SelectV2.SongSelectV2()));
            AddUntilStep("wait for load", () => Stack.CurrentScreen is Screens.SelectV2.SongSelectV2 songSelect && songSelect.IsLoaded);
        }

        [Test]
        public void TestBeatmap()
        {
            Live<BeatmapSetInfo> testBeatmap = null!;

            AddStep("import test beatmap", () => testBeatmap = manager.Import(new ImportTask(TestResources.GetTestBeatmapForImport())).GetResultSafely().AsNonNull());
            AddStep("select random diff", () => selectBeatmap(manager.GetWorkingBeatmap(testBeatmap.Value.Beatmaps.OrderBy(_ => RNG.Next()).First())));

            addManyTestMaps(5);

            AddStep("select beatmap randomly", () => selectBeatmap(manager.GetWorkingBeatmap(manager.GetAllUsableBeatmapSets().SelectMany(bs => bs.Beatmaps).OrderBy(_ => RNG.Next()).First())));

            AddStep("select easy", () => SelectedMods.Value = new[] { Ruleset.Value.CreateInstance().CreateMod<ModEasy>() });
            AddStep("select hard rock", () => SelectedMods.Value = new[] { Ruleset.Value.CreateInstance().CreateMod<ModHardRock>() });
            AddStep("select half time", () => SelectedMods.Value = new[] { Ruleset.Value.CreateInstance().CreateMod<ModHalfTime>() });
            AddStep("select double time", () => SelectedMods.Value = new[] { Ruleset.Value.CreateInstance().CreateMod<ModDoubleTime>() });
            AddStep("clear mods", () => SelectedMods.Value = Array.Empty<Mod>());
        }

        private void selectBeatmap(WorkingBeatmap beatmap)
        {
            Ruleset.Value = beatmap.BeatmapInfo.Ruleset;
            Beatmap.Value = beatmap;
        }

        #region Footer

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
                var optionsButton = this.ChildrenOfType<ScreenFooterButton>().Last();

                optionsButton.Enabled.Value = true;
                optionsButton.TriggerClick();
            });
        }

        [Test]
        public void TestState()
        {
            AddToggleStep("set options enabled state", state => this.ChildrenOfType<ScreenFooterButton>().Last().Enabled.Value = state);
        }

        // add these test cases when functionality is implemented.
        // [Test]
        // public void TestFooterRandom()
        // {
        //     AddStep("press F2", () => InputManager.Key(Key.F2));
        //     AddAssert("next random invoked", () => nextRandomCalled && !previousRandomCalled);
        // }
        //
        // [Test]
        // public void TestFooterRandomViaMouse()
        // {
        //     AddStep("click button", () =>
        //     {
        //         InputManager.MoveMouseTo(randomButton);
        //         InputManager.Click(MouseButton.Left);
        //     });
        //     AddAssert("next random invoked", () => nextRandomCalled && !previousRandomCalled);
        // }
        //
        // [Test]
        // public void TestFooterRewind()
        // {
        //     AddStep("press Shift+F2", () =>
        //     {
        //         InputManager.PressKey(Key.LShift);
        //         InputManager.PressKey(Key.F2);
        //         InputManager.ReleaseKey(Key.F2);
        //         InputManager.ReleaseKey(Key.LShift);
        //     });
        //     AddAssert("previous random invoked", () => previousRandomCalled && !nextRandomCalled);
        // }
        //
        // [Test]
        // public void TestFooterRewindViaShiftMouseLeft()
        // {
        //     AddStep("shift + click button", () =>
        //     {
        //         InputManager.PressKey(Key.LShift);
        //         InputManager.MoveMouseTo(randomButton);
        //         InputManager.Click(MouseButton.Left);
        //         InputManager.ReleaseKey(Key.LShift);
        //     });
        //     AddAssert("previous random invoked", () => previousRandomCalled && !nextRandomCalled);
        // }
        //
        // [Test]
        // public void TestFooterRewindViaMouseRight()
        // {
        //     AddStep("right click button", () =>
        //     {
        //         InputManager.MoveMouseTo(randomButton);
        //         InputManager.Click(MouseButton.Right);
        //     });
        //     AddAssert("previous random invoked", () => previousRandomCalled && !nextRandomCalled);
        // }

        [Test]
        public void TestOverlayPresent()
        {
            AddStep("Press F1", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<ScreenFooterButtonMods>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("Overlay visible", () => this.ChildrenOfType<ModSelectOverlay>().Single().State.Value == Visibility.Visible);
            AddStep("Hide", () => this.ChildrenOfType<ModSelectOverlay>().Single().Hide());
        }

        #endregion

        /// <summary>
        /// Imports test beatmap sets to show in the carousel.
        /// </summary>
        /// <param name="difficultyCountPerSet">
        /// The exact count of difficulties to create for each beatmap set.
        /// A <see langword="null"/> value causes the count of difficulties to be selected randomly.
        /// </param>
        private void addManyTestMaps(int? difficultyCountPerSet = null)
        {
            AddStep("import test maps", () =>
            {
                var usableRulesets = rulesets.AvailableRulesets.Where(r => r.OnlineID != 2).ToArray();

                for (int i = 0; i < 10; i++)
                    manager.Import(TestResources.CreateTestBeatmapSetInfo(difficultyCountPerSet, usableRulesets));
            });
        }

        protected override void Update()
        {
            base.Update();
            Stack.Padding = new MarginPadding { Bottom = screenScreenFooter.DrawHeight - screenScreenFooter.Y };
        }

        private void updateFooter(IScreen? _, IScreen? newScreen)
        {
            if (newScreen is IOsuScreen osuScreen && osuScreen.ShowFooter)
            {
                screenScreenFooter.Show();
                screenScreenFooter.SetButtons(osuScreen.CreateFooterButtons());
            }
            else
            {
                screenScreenFooter.Hide();
                screenScreenFooter.SetButtons(Array.Empty<ScreenFooterButton>());
            }
        }
    }
}
