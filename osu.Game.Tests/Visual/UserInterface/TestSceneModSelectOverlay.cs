// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Screens;
using osu.Game.Screens.Footer;
using osu.Game.Tests.Mods;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneModSelectOverlay : ScreenTestScene
    {
        protected override bool UseFreshStoragePerRun => true;

        private RulesetStore rulesetStore = null!;

        private TestModSelectOverlayScreen screen = null!;

        [Resolved]
        private OsuConfigManager configManager { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(rulesetStore = new RealmRulesetStore(Realm));
            Dependencies.Cache(Realm);
        }

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("reset ruleset", () => Ruleset.Value = rulesetStore.GetRuleset(0));
            AddStep("reset mods", () => SelectedMods.SetDefault());
            AddStep("reset config", () => configManager.SetValue(OsuSetting.ModSelectTextSearchStartsActive, true));
            AddStep("reset mouse", () => InputManager.MoveMouseTo(Vector2.One));
            AddStep("set beatmap", () => Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo));
            AddStep("set up presets", () =>
            {
                Realm.Write(r =>
                {
                    r.RemoveAll<ModPreset>();
                    r.Add(new ModPreset
                    {
                        Name = "AR0",
                        Description = "Too... many... circles...",
                        Ruleset = r.Find<RulesetInfo>(OsuRuleset.SHORT_NAME)!,
                        Mods = new[]
                        {
                            new OsuModDifficultyAdjust
                            {
                                ApproachRate = { Value = 0 }
                            }
                        }
                    });
                    r.Add(new ModPreset
                    {
                        Name = "Half Time 0.5x",
                        Description = "Very slow",
                        Ruleset = r.Find<RulesetInfo>(OsuRuleset.SHORT_NAME)!,
                        Mods = new[]
                        {
                            new OsuModHalfTime
                            {
                                SpeedChange = { Value = 0.5 }
                            }
                        }
                    });
                });
            });
        }

        private void createScreen()
        {
            AddStep("create screen", () => LoadScreen(screen = new TestModSelectOverlayScreen { SelectedMods = { BindTarget = SelectedMods } }));
            AddUntilStep("wait until screen is loaded", () => screen.IsLoaded, () => Is.True);
            waitForColumnLoad();
        }

        [Test]
        public void TestStateChange()
        {
            createScreen();
            AddStep("toggle state", () => screen.Overlay.ToggleVisibility());
        }

        [Test]
        public void TestPreexistingSelection()
        {
            AddStep("set mods", () => SelectedMods.Value = new Mod[] { new OsuModAlternate(), new OsuModDaycore() });
            createScreen();
            AddUntilStep("two panels active", () => screen.Overlay.ChildrenOfType<ModPanel>().Count(panel => panel.Active.Value) == 2);
            AddAssert("mod multiplier correct", () =>
            {
                double multiplier = SelectedMods.Value.Aggregate(1d, (m, mod) => m * mod.ScoreMultiplier);
                return Precision.AlmostEquals(multiplier, this.ChildrenOfType<RankingInformationDisplay>().Single().ModMultiplier.Value);
            });
            assertCustomisationToggleState(disabled: false, active: false);
            AddAssert("setting items created", () => screen.Overlay.ChildrenOfType<ISettingsItem>().Any());
        }

        [Test]
        public void TestExternalSelection()
        {
            createScreen();
            AddStep("set mods", () => SelectedMods.Value = new Mod[] { new OsuModAlternate(), new OsuModDaycore() });
            AddUntilStep("two panels active", () => screen.Overlay.ChildrenOfType<ModPanel>().Count(panel => panel.Active.Value) == 2);
            AddAssert("mod multiplier correct", () =>
            {
                double multiplier = SelectedMods.Value.Aggregate(1d, (m, mod) => m * mod.ScoreMultiplier);
                return Precision.AlmostEquals(multiplier, this.ChildrenOfType<RankingInformationDisplay>().Single().ModMultiplier.Value);
            });
            assertCustomisationToggleState(disabled: false, active: false);
            AddAssert("setting items created", () => screen.Overlay.ChildrenOfType<ISettingsItem>().Any());
        }

        [Test]
        public void TestRulesetChange()
        {
            createScreen();
            changeRuleset(0);
            changeRuleset(1);
            changeRuleset(2);
            changeRuleset(3);
        }

        [Test]
        public void TestIncompatibilityToggling()
        {
            createScreen();
            changeRuleset(0);

            AddStep("activate DT", () => getPanelForMod(typeof(OsuModDoubleTime)).TriggerClick());
            AddAssert("DT active", () => SelectedMods.Value.Single().GetType() == typeof(OsuModDoubleTime));
            AddAssert("DT panel active", () => getPanelForMod(typeof(OsuModDoubleTime)).Active.Value);

            AddStep("activate NC", () => getPanelForMod(typeof(OsuModNightcore)).TriggerClick());
            AddAssert("only NC active", () => SelectedMods.Value.Single().GetType() == typeof(OsuModNightcore));
            AddAssert("DT panel not active", () => !getPanelForMod(typeof(OsuModDoubleTime)).Active.Value);
            AddAssert("NC panel active", () => getPanelForMod(typeof(OsuModNightcore)).Active.Value);

            AddStep("activate HR", () => getPanelForMod(typeof(OsuModHardRock)).TriggerClick());
            AddAssert("NC+HR active", () => SelectedMods.Value.Any(mod => mod.GetType() == typeof(OsuModNightcore))
                                            && SelectedMods.Value.Any(mod => mod.GetType() == typeof(OsuModHardRock)));
            AddAssert("NC panel active", () => getPanelForMod(typeof(OsuModNightcore)).Active.Value);
            AddAssert("HR panel active", () => getPanelForMod(typeof(OsuModHardRock)).Active.Value);

            AddStep("activate MR", () => getPanelForMod(typeof(OsuModMirror)).TriggerClick());
            AddAssert("NC+MR active", () => SelectedMods.Value.Any(mod => mod.GetType() == typeof(OsuModNightcore))
                                            && SelectedMods.Value.Any(mod => mod.GetType() == typeof(OsuModMirror)));
            AddAssert("NC panel active", () => getPanelForMod(typeof(OsuModNightcore)).Active.Value);
            AddAssert("HR panel not active", () => !getPanelForMod(typeof(OsuModHardRock)).Active.Value);
            AddAssert("MR panel active", () => getPanelForMod(typeof(OsuModMirror)).Active.Value);
        }

        [Test]
        public void TestDimmedState()
        {
            createScreen();
            changeRuleset(0);

            AddUntilStep("any column dimmed", () => this.ChildrenOfType<ModColumn>().Any(column => !column.Active.Value));

            ModSelectColumn lastColumn = null!;

            AddAssert("last column dimmed", () => !this.ChildrenOfType<ModColumn>().Last().Active.Value);
            AddStep("request scroll to last column", () =>
            {
                var lastDimContainer = this.ChildrenOfType<ModSelectOverlay.ColumnDimContainer>().Last();
                lastColumn = lastDimContainer.Column;
                lastDimContainer.RequestScroll?.Invoke(lastDimContainer);
            });
            AddUntilStep("column undimmed", () => lastColumn.Active.Value);

            AddStep("click panel", () =>
            {
                InputManager.MoveMouseTo(lastColumn.ChildrenOfType<ModPanel>().First());
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("panel selected", () => lastColumn.ChildrenOfType<ModPanel>().First().Active.Value);
        }

        [Test]
        public void TestCustomisationToggleState()
        {
            createScreen();
            assertCustomisationToggleState(disabled: true, active: false);

            AddStep("select customisable mod", () => SelectedMods.Value = new[] { new OsuModDoubleTime() });
            assertCustomisationToggleState(disabled: false, active: false);

            AddStep("select mod requiring configuration externally", () => SelectedMods.Value = new[] { new OsuModDifficultyAdjust() });
            assertCustomisationToggleState(disabled: false, active: false);

            AddStep("reset mods", () => SelectedMods.SetDefault());
            AddStep("select difficulty adjust via panel", () => getPanelForMod(typeof(OsuModDifficultyAdjust)).TriggerClick());
            assertCustomisationToggleState(disabled: false, active: true);

            AddStep("move mouse away", () => InputManager.MoveMouseTo(Vector2.Zero));
            assertCustomisationToggleState(disabled: false, active: true);

            AddStep("reset mods", () => SelectedMods.SetDefault());
            AddStep("select difficulty adjust via panel", () => getPanelForMod(typeof(OsuModDifficultyAdjust)).TriggerClick());
            assertCustomisationToggleState(disabled: false, active: true);

            AddStep("dismiss mod customisation via keyboard", () => InputManager.Key(Key.Escape));
            assertCustomisationToggleState(disabled: false, active: false);

            AddStep("append another mod not requiring config", () => SelectedMods.Value = SelectedMods.Value.Append(new OsuModFlashlight()).ToArray());
            assertCustomisationToggleState(disabled: false, active: false);

            AddStep("select mod without configuration", () => SelectedMods.Value = new[] { new OsuModAutoplay() });
            assertCustomisationToggleState(disabled: true, active: false);

            AddStep("select difficulty adjust via panel", () => getPanelForMod(typeof(OsuModDifficultyAdjust)).TriggerClick());
            assertCustomisationToggleState(disabled: false, active: true);

            AddStep("select mod without configuration", () => SelectedMods.Value = new[] { new OsuModAutoplay() });
            assertCustomisationToggleState(disabled: true, active: false); // config was dismissed without explicit user action.

            AddStep("select mod preset with mod requiring configuration", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<ModPresetPanel>().First());
                InputManager.Click(MouseButton.Left);
            });
            assertCustomisationToggleState(disabled: false, active: false);
        }

        [Test]
        public void TestDismissCustomisationViaClickingAway()
        {
            createScreen();
            assertCustomisationToggleState(disabled: true, active: false);

            AddStep("select difficulty adjust via panel", () => getPanelForMod(typeof(OsuModDifficultyAdjust)).TriggerClick());
            assertCustomisationToggleState(disabled: false, active: true);

            AddStep("move mouse to search bar", () => InputManager.MoveMouseTo(screen.Overlay.ChildrenOfType<ShearedSearchTextBox>().Single()));
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            assertCustomisationToggleState(disabled: false, active: false);
        }

        [Test]
        public void TestDismissCustomisationWhenHidingOverlay()
        {
            createScreen();
            assertCustomisationToggleState(disabled: true, active: false);

            AddStep("select difficulty adjust via panel", () => getPanelForMod(typeof(OsuModDifficultyAdjust)).TriggerClick());
            assertCustomisationToggleState(disabled: false, active: true);

            AddStep("hide overlay", () => screen.Overlay.Hide());
            AddStep("show overlay again", () => screen.Overlay.Show());
            assertCustomisationToggleState(disabled: false, active: false);
        }

        /// <summary>
        /// Ensure that two mod overlays are not cross polluting via central settings instances.
        /// </summary>
        [Test]
        public void TestSettingsNotCrossPolluting()
        {
            TestScreenWithTwoOverlays screenWithTwoOverlays = null!;
            Bindable<IReadOnlyList<Mod>> selectedMods2 = null!;

            AddStep("push screen", () =>
            {
                selectedMods2 = new Bindable<IReadOnlyList<Mod>>(new Mod[] { new OsuModDifficultyAdjust() });

                LoadScreen(screen = screenWithTwoOverlays = new TestScreenWithTwoOverlays
                {
                    SelectedMods = { BindTarget = SelectedMods },
                    SelectedMods2 = { BindTarget = selectedMods2 },
                });
            });
            AddStep("wait until screen is loaded", () => screenWithTwoOverlays.IsCurrentScreen());
            waitForColumnLoad();

            AddStep("select difficulty adjust via panel", () => getPanelForMod(typeof(OsuModDifficultyAdjust)).TriggerClick());

            AddStep("set setting", () => screenWithTwoOverlays.Overlay.ChildrenOfType<RoundedSliderBar<float>>().First().Current.Value = 8);

            AddAssert("ensure setting is propagated", () => SelectedMods.Value.OfType<OsuModDifficultyAdjust>().Single().CircleSize.Value == 8);

            AddStep("hide first overlay", () => screenWithTwoOverlays.Overlay.Hide());
            AddStep("show second overlay", () => screenWithTwoOverlays.SecondOverlay.Show());

            AddAssert("ensure first is unchanged", () => SelectedMods.Value.OfType<OsuModDifficultyAdjust>().Single().CircleSize.Value == 8);
            AddAssert("ensure second is default", () => selectedMods2.Value.OfType<OsuModDifficultyAdjust>().Single().CircleSize.Value == null);
        }

        [Test]
        public void TestSettingsResetOnDeselection()
        {
            var osuModDoubleTime = new OsuModDoubleTime { SpeedChange = { Value = 1.2 } };

            createScreen();
            changeRuleset(0);

            AddStep("set dt mod with custom rate", () => { SelectedMods.Value = new[] { osuModDoubleTime }; });

            AddAssert("selected mod matches", () => (SelectedMods.Value.Single() as OsuModDoubleTime)?.SpeedChange.Value == 1.2);

            AddStep("deselect", () => getPanelForMod(typeof(OsuModDoubleTime)).TriggerClick());
            AddAssert("selected mods empty", () => SelectedMods.Value.Count == 0);

            AddStep("reselect", () => getPanelForMod(typeof(OsuModDoubleTime)).TriggerClick());
            AddAssert("selected mod has default value", () => (SelectedMods.Value.Single() as OsuModDoubleTime)?.SpeedChange.IsDefault == true);
        }

        [Test]
        public void TestAnimationFlushOnClose()
        {
            createScreen();
            changeRuleset(0);

            AddStep("Select all difficulty-increase mods", () =>
            {
                screen.Overlay.ChildrenOfType<ModColumn>()
                      .Single(c => c.ModType == ModType.DifficultyIncrease)
                      .SelectAll();
            });

            AddUntilStep("many mods selected", () => SelectedMods.Value.Count >= 5);

            AddStep("trigger deselect and close overlay", () =>
            {
                screen.Overlay.ChildrenOfType<ModColumn>()
                      .Single(c => c.ModType == ModType.DifficultyIncrease)
                      .DeselectAll();

                screen.Overlay.Hide();
            });

            AddAssert("all mods deselected", () => SelectedMods.Value.Count == 0);
        }

        [Test]
        public void TestCommonModsMaintainedOnRulesetChange()
        {
            createScreen();
            changeRuleset(0);

            AddStep("select relax mod", () => SelectedMods.Value = new[] { Ruleset.Value.CreateInstance().CreateMod<ModRelax>() });

            changeRuleset(0);
            AddAssert("ensure mod still selected", () => SelectedMods.Value.SingleOrDefault() is OsuModRelax);

            changeRuleset(2);
            AddAssert("catch variant selected", () => SelectedMods.Value.SingleOrDefault() is CatchModRelax);

            changeRuleset(3);
            AddAssert("no mod selected", () => SelectedMods.Value.Count == 0);
        }

        [Test]
        public void TestUncommonModsDiscardedOnRulesetChange()
        {
            createScreen();
            changeRuleset(0);

            AddStep("select single tap mod", () => SelectedMods.Value = new[] { new OsuModSingleTap() });

            changeRuleset(0);
            AddAssert("ensure mod still selected", () => SelectedMods.Value.SingleOrDefault() is OsuModSingleTap);

            changeRuleset(3);
            AddAssert("no mod selected", () => SelectedMods.Value.Count == 0);
        }

        [Test]
        public void TestKeepSharedSettingsFromSimilarMods()
        {
            const float setting_change = 1.2f;

            createScreen();
            changeRuleset(0);

            AddStep("select difficulty adjust mod", () => SelectedMods.Value = new[] { Ruleset.Value.CreateInstance().CreateMod<ModDifficultyAdjust>()! });

            changeRuleset(0);
            AddAssert("ensure mod still selected", () => SelectedMods.Value.SingleOrDefault() is OsuModDifficultyAdjust);

            AddStep("change mod settings", () =>
            {
                var osuMod = getSelectedMod<OsuModDifficultyAdjust>();

                osuMod.ExtendedLimits.Value = true;
                osuMod.CircleSize.Value = setting_change;
                osuMod.DrainRate.Value = setting_change;
                osuMod.OverallDifficulty.Value = setting_change;
                osuMod.ApproachRate.Value = setting_change;
            });

            changeRuleset(1);
            AddAssert("taiko variant selected", () => SelectedMods.Value.SingleOrDefault() is TaikoModDifficultyAdjust);

            AddAssert("shared settings preserved", () =>
            {
                var taikoMod = getSelectedMod<TaikoModDifficultyAdjust>();

                return taikoMod.ExtendedLimits.Value &&
                       taikoMod.DrainRate.Value == setting_change &&
                       taikoMod.OverallDifficulty.Value == setting_change;
            });

            AddAssert("non-shared settings remain default", () =>
            {
                var taikoMod = getSelectedMod<TaikoModDifficultyAdjust>();

                return taikoMod.ScrollSpeed.IsDefault;
            });
        }

        [Test]
        public void TestExternallySetCustomizedMod()
        {
            createScreen();
            changeRuleset(0);

            AddStep("set customized mod externally", () => SelectedMods.Value = new[] { new OsuModDoubleTime { SpeedChange = { Value = 1.01 } } });

            AddAssert("ensure button is selected and customized accordingly", () =>
            {
                var button = getPanelForMod(SelectedMods.Value.Single().GetType());
                return ((OsuModDoubleTime)button.Mod).SpeedChange.Value == 1.01;
            });
        }

        [Test]
        public void TestSettingsAreRetainedOnReload()
        {
            createScreen();
            changeRuleset(0);

            AddStep("set customized mod externally", () => SelectedMods.Value = new[] { new OsuModDoubleTime { SpeedChange = { Value = 1.01 } } });
            AddAssert("setting remains", () => (SelectedMods.Value.SingleOrDefault() as OsuModDoubleTime)?.SpeedChange.Value == 1.01);

            AddStep("exit screen", () => Stack.Exit());
            createScreen();
            AddAssert("setting remains", () => (SelectedMods.Value.SingleOrDefault() as OsuModDoubleTime)?.SpeedChange.Value == 1.01);
        }

        [Test]
        public void TestExternallySetModIsReplacedByOverlayInstance()
        {
            Mod external = new OsuModDoubleTime();
            Mod overlayButtonMod = null!;

            createScreen();
            changeRuleset(0);

            AddStep("set mod externally", () => { SelectedMods.Value = new[] { external }; });

            AddAssert("ensure button is selected", () =>
            {
                var button = getPanelForMod(SelectedMods.Value.Single().GetType());
                overlayButtonMod = button.Mod;
                return button.Active.Value;
            });

            // Right now, when an external change occurs, the ModSelectOverlay will replace the global instance with its own
            AddAssert("mod instance doesn't match", () => external != overlayButtonMod);

            AddAssert("one mod present in global selected", () => SelectedMods.Value.Count == 1);
            AddAssert("globally selected matches button's mod instance", () => SelectedMods.Value.Any(mod => ReferenceEquals(mod, overlayButtonMod)));
            AddAssert("globally selected doesn't contain original external change", () => !SelectedMods.Value.Any(mod => ReferenceEquals(mod, external)));
        }

        [Test]
        public void TestChangeIsValidChangesButtonVisibility()
        {
            createScreen();
            changeRuleset(0);

            AddAssert("double time visible", () => screen.Overlay.ChildrenOfType<ModPanel>().Where(panel => panel.Mod is OsuModDoubleTime).Any(panel => panel.Visible));

            AddStep("make double time invalid", () => screen.Overlay.IsValidMod = m => !(m is OsuModDoubleTime));
            AddUntilStep("double time not visible", () => screen.Overlay.ChildrenOfType<ModPanel>().Where(panel => panel.Mod is OsuModDoubleTime).All(panel => !panel.Visible));
            AddAssert("nightcore still visible", () => screen.Overlay.ChildrenOfType<ModPanel>().Where(panel => panel.Mod is OsuModNightcore).Any(panel => panel.Visible));

            AddStep("make double time valid again", () => screen.Overlay.IsValidMod = _ => true);
            AddUntilStep("double time visible", () => screen.Overlay.ChildrenOfType<ModPanel>().Where(panel => panel.Mod is OsuModDoubleTime).Any(panel => panel.Visible));
            AddAssert("nightcore still visible", () => screen.Overlay.ChildrenOfType<ModPanel>().Where(b => b.Mod is OsuModNightcore).Any(panel => panel.Visible));
        }

        [Test]
        public void TestChangeIsValidPreservesSelection()
        {
            createScreen();
            changeRuleset(0);

            AddStep("select DT + HD", () => SelectedMods.Value = new Mod[] { new OsuModDoubleTime(), new OsuModHidden() });
            AddAssert("DT + HD selected", () => screen.Overlay.ChildrenOfType<ModPanel>().Count(panel => panel.Active.Value) == 2);

            AddStep("make NF invalid", () => screen.Overlay.IsValidMod = m => !(m is ModNoFail));
            AddAssert("DT + HD still selected", () => screen.Overlay.ChildrenOfType<ModPanel>().Count(panel => panel.Active.Value) == 2);
        }

        [Test]
        public void TestUnimplementedModIsUnselectable()
        {
            var testRuleset = new TestUnimplementedModOsuRuleset();

            createScreen();

            AddStep("set ruleset", () => Ruleset.Value = testRuleset.RulesetInfo);
            waitForColumnLoad();

            AddAssert("unimplemented mod panel is filtered", () => !getPanelForMod(typeof(TestUnimplementedMod)).Visible);
        }

        [Test]
        public void TestFirstModSelectDeselect()
        {
            createScreen();

            AddStep("apply search", () => screen.Overlay.SearchTerm = "HD");

            AddStep("press enter", () => InputManager.Key(Key.Enter));
            AddAssert("hidden selected", () => getPanelForMod(typeof(OsuModHidden)).Active.Value);
            AddAssert("all text selected in textbox", () =>
            {
                var textBox = screen.Overlay.ChildrenOfType<SearchTextBox>().Single();
                return textBox.SelectedText == textBox.Text;
            });

            AddStep("press enter again", () => InputManager.Key(Key.Enter));
            AddAssert("hidden deselected", () => !getPanelForMod(typeof(OsuModHidden)).Active.Value);

            AddStep("apply search matching nothing", () => screen.Overlay.SearchTerm = "ZZZ");
            AddStep("press enter", () => InputManager.Key(Key.Enter));
            AddAssert("all text not selected in textbox", () =>
            {
                var textBox = screen.Overlay.ChildrenOfType<SearchTextBox>().Single();
                return textBox.SelectedText != textBox.Text;
            });

            AddStep("clear search", () => screen.Overlay.SearchTerm = string.Empty);
            AddStep("press enter", () => InputManager.Key(Key.Enter));
            AddAssert("mod select hidden", () => screen.Overlay.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestSearchFocusChangeViaClick()
        {
            createScreen();

            AddStep("click on search", navigateAndClick<ShearedSearchTextBox>);
            AddAssert("focused", () => screen.Overlay.SearchTextBox.HasFocus);

            AddStep("click on mod column", navigateAndClick<ModColumn>);
            AddAssert("lost focus", () => !screen.Overlay.SearchTextBox.HasFocus);

            void navigateAndClick<T>() where T : Drawable
            {
                InputManager.MoveMouseTo(screen.Overlay.ChildrenOfType<T>().First());
                InputManager.Click(MouseButton.Left);
            }
        }

        [Test]
        public void TestTextSearchActiveByDefault()
        {
            AddStep("text search starts active", () => configManager.SetValue(OsuSetting.ModSelectTextSearchStartsActive, true));
            createScreen();

            AddUntilStep("search text box focused", () => screen.Overlay.SearchTextBox.HasFocus);

            AddStep("press tab", () => InputManager.Key(Key.Tab));
            AddAssert("search text box unfocused", () => !screen.Overlay.SearchTextBox.HasFocus);

            AddStep("press tab", () => InputManager.Key(Key.Tab));
            AddAssert("search text box focused", () => screen.Overlay.SearchTextBox.HasFocus);
        }

        [Test]
        public void TestTextSearchNotActiveByDefault()
        {
            AddStep("text search does not start active", () => configManager.SetValue(OsuSetting.ModSelectTextSearchStartsActive, false));
            createScreen();

            AddUntilStep("search text box not focused", () => !screen.Overlay.SearchTextBox.HasFocus);

            AddStep("press tab", () => InputManager.Key(Key.Tab));
            AddAssert("search text box focused", () => screen.Overlay.SearchTextBox.HasFocus);

            AddStep("press tab", () => InputManager.Key(Key.Tab));
            AddAssert("search text box unfocused", () => !screen.Overlay.SearchTextBox.HasFocus);
        }

        [Test]
        public void TestSearchBoxFocusToggleRespondsToExternalChanges()
        {
            AddStep("text search does not start active", () => configManager.SetValue(OsuSetting.ModSelectTextSearchStartsActive, false));
            createScreen();

            AddUntilStep("search text box not focused", () => !screen.Overlay.SearchTextBox.HasFocus);

            AddStep("press tab", () => InputManager.Key(Key.Tab));
            AddAssert("search text box focused", () => screen.Overlay.SearchTextBox.HasFocus);

            AddStep("unfocus search text box externally", () => ((IFocusManager)InputManager).ChangeFocus(null));

            AddStep("press tab", () => InputManager.Key(Key.Tab));
            AddAssert("search text box focused", () => screen.Overlay.SearchTextBox.HasFocus);
        }

        [Test]
        public void TestTextSearchDoesNotBlockCustomisationPanelKeyboardInteractions()
        {
            AddStep("text search starts active", () => configManager.SetValue(OsuSetting.ModSelectTextSearchStartsActive, true));
            createScreen();

            AddUntilStep("search text box focused", () => screen.Overlay.SearchTextBox.HasFocus);

            AddStep("select DT", () => SelectedMods.Value = new Mod[] { new OsuModDoubleTime() });
            AddAssert("DT selected", () => screen.Overlay.ChildrenOfType<ModPanel>().Count(panel => panel.Active.Value), () => Is.EqualTo(1));

            AddStep("open customisation area", () => InputManager.MoveMouseTo(screen.Overlay.ChildrenOfType<ModCustomisationHeader>().Single()));
            assertCustomisationToggleState(disabled: false, active: true);

            AddStep("hover over mod settings slider", () =>
            {
                var slider = screen.Overlay.ChildrenOfType<ModCustomisationPanel>().Single().ChildrenOfType<OsuSliderBar<double>>().First();
                InputManager.MoveMouseTo(slider);
            });

            AddStep("press right arrow", () => InputManager.PressKey(Key.Right));
            AddAssert("DT speed changed", () => !SelectedMods.Value.OfType<OsuModDoubleTime>().Single().SpeedChange.IsDefault);

            AddStep("close customisation area", () => InputManager.PressKey(Key.Escape));
            AddUntilStep("search text box reacquired focus", () => screen.Overlay.SearchTextBox.HasFocus);
        }

        [Test]
        public void TestDeselectAllViaKey()
        {
            createScreen();
            changeRuleset(0);

            AddStep("kill search bar focus", () => screen.Overlay.SearchTextBox.KillFocus());

            AddStep("select DT + HD", () => SelectedMods.Value = new Mod[] { new OsuModDoubleTime(), new OsuModHidden() });
            AddAssert("DT + HD selected", () => screen.Overlay.ChildrenOfType<ModPanel>().Count(panel => panel.Active.Value) == 2);

            AddStep("press backspace", () => InputManager.Key(Key.BackSpace));
            AddUntilStep("all mods deselected", () => !SelectedMods.Value.Any());
        }

        [Test]
        public void TestDeselectAllViaKey_WithSearchApplied()
        {
            createScreen();
            changeRuleset(0);

            AddStep("select DT + HD", () => SelectedMods.Value = new Mod[] { new OsuModDoubleTime(), new OsuModHidden() });
            AddStep("focus on search", () => screen.Overlay.SearchTextBox.TakeFocus());
            AddStep("apply search", () => screen.Overlay.SearchTerm = "Easy");
            AddAssert("DT + HD selected and hidden", () => screen.Overlay.ChildrenOfType<ModPanel>().Count(panel => !panel.Visible && panel.Active.Value) == 2);

            AddStep("press backspace", () => InputManager.Key(Key.BackSpace));
            AddAssert("DT + HD still selected", () => screen.Overlay.ChildrenOfType<ModPanel>().Count(panel => panel.Active.Value) == 2);
            AddAssert("search term changed", () => screen.Overlay.SearchTerm == "Eas");

            AddStep("kill focus", () => screen.Overlay.SearchTextBox.KillFocus());
            AddStep("press backspace", () => InputManager.Key(Key.BackSpace));
            AddUntilStep("all mods deselected", () => !SelectedMods.Value.Any());
        }

        [Test]
        public void TestDeselectAllViaButton()
        {
            createScreen();
            changeRuleset(0);

            AddAssert("deselect all button disabled", () => !this.ChildrenOfType<DeselectAllModsButton>().Single().Enabled.Value);

            AddStep("select DT + HD", () => SelectedMods.Value = new Mod[] { new OsuModDoubleTime(), new OsuModHidden() });
            AddAssert("DT + HD selected", () => screen.Overlay.ChildrenOfType<ModPanel>().Count(panel => panel.Active.Value) == 2);
            AddAssert("deselect all button enabled", () => this.ChildrenOfType<DeselectAllModsButton>().Single().Enabled.Value);

            AddStep("click deselect all button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<DeselectAllModsButton>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("all mods deselected", () => !SelectedMods.Value.Any());
            AddAssert("deselect all button disabled", () => !this.ChildrenOfType<DeselectAllModsButton>().Single().Enabled.Value);
        }

        [Test]
        public void TestDeselectAllViaButton_WithSearchApplied()
        {
            createScreen();
            changeRuleset(0);

            AddAssert("deselect all button disabled", () => !this.ChildrenOfType<DeselectAllModsButton>().Single().Enabled.Value);

            AddStep("select DT + HD + RD", () => SelectedMods.Value = new Mod[] { new OsuModDoubleTime(), new OsuModHidden(), new OsuModRandom() });
            AddAssert("DT + HD + RD selected", () => screen.Overlay.ChildrenOfType<ModPanel>().Count(panel => panel.Active.Value) == 3);
            AddAssert("deselect all button enabled", () => this.ChildrenOfType<DeselectAllModsButton>().Single().Enabled.Value);

            AddStep("apply search", () => screen.Overlay.SearchTerm = "Easy");
            AddAssert("DT + HD + RD are hidden and selected", () => screen.Overlay.ChildrenOfType<ModPanel>().Count(panel => !panel.Visible && panel.Active.Value) == 3);
            AddAssert("deselect all button enabled", () => this.ChildrenOfType<DeselectAllModsButton>().Single().Enabled.Value);

            AddStep("click deselect all button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<DeselectAllModsButton>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("all mods deselected", () => !SelectedMods.Value.Any());
            AddAssert("deselect all button disabled", () => !this.ChildrenOfType<DeselectAllModsButton>().Single().Enabled.Value);
        }

        [Test]
        public void TestCloseViaBackButton()
        {
            createScreen();
            changeRuleset(0);

            AddStep("select difficulty adjust via panel", () => getPanelForMod(typeof(OsuModDifficultyAdjust)).TriggerClick());
            assertCustomisationToggleState(disabled: false, active: true);

            AddStep("dismiss customisation area", () => InputManager.Key(Key.Escape));
            AddAssert("mod select still visible", () => screen.Overlay.State.Value == Visibility.Visible);

            AddStep("click back button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<ScreenBackButton>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("mod select hidden", () => screen.Overlay.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestCloseViaToggleModSelectionBinding()
        {
            createScreen();
            changeRuleset(0);

            AddStep("select difficulty adjust via panel", () => getPanelForMod(typeof(OsuModDifficultyAdjust)).TriggerClick());
            assertCustomisationToggleState(disabled: false, active: true);

            AddStep("press F1", () => InputManager.Key(Key.F1));
            AddAssert("mod select hidden", () => screen.Overlay.State.Value == Visibility.Hidden);
        }

        /// <summary>
        /// Covers columns hiding/unhiding on changes of <see cref="ModSelectOverlay.IsValidMod"/>.
        /// </summary>
        [Test]
        public void TestColumnHidingOnIsValidChange()
        {
            createScreen();
            changeRuleset(0);

            AddStep("set filter for 2 columns", () => screen.Overlay.IsValidMod = mod => mod.Type is ModType.DifficultyIncrease or ModType.Conversion);

            AddAssert("two columns visible", () => this.ChildrenOfType<ModColumn>().Count(col => col.IsPresent) == 2);

            AddStep("unset filter", () => screen.Overlay.IsValidMod = _ => true);
            AddAssert("all columns visible", () => this.ChildrenOfType<ModColumn>().All(col => col.IsPresent));

            AddStep("filter out everything", () => screen.Overlay.IsValidMod = _ => false);
            AddAssert("no columns visible", () => this.ChildrenOfType<ModColumn>().All(col => !col.IsPresent));

            AddStep("hide", () => screen.Overlay.Hide());
            AddStep("set filter for 3 columns", () => screen.Overlay.IsValidMod = mod => mod.Type is ModType.DifficultyReduction or ModType.Automation or ModType.Conversion);

            AddStep("show", () => screen.Overlay.Show());
            AddUntilStep("3 columns visible", () => this.ChildrenOfType<ModColumn>().Count(col => col.IsPresent) == 3);
        }

        /// <summary>
        /// Covers columns hiding/unhiding on changes of <see cref="ModSelectOverlay.SearchTerm"/>.
        /// </summary>
        [Test]
        public void TestColumnHidingOnTextFilterChange()
        {
            createScreen();
            changeRuleset(0);

            AddAssert("all columns visible", () => this.ChildrenOfType<ModColumn>().All(col => col.IsPresent));

            AddStep("set search", () => screen.Overlay.SearchTerm = "HD");
            AddAssert("two columns visible", () => this.ChildrenOfType<ModColumn>().Count(col => col.IsPresent) == 2);

            AddStep("filter out everything", () => screen.Overlay.SearchTerm = "Some long search term with no matches");
            AddAssert("no columns visible", () => this.ChildrenOfType<ModColumn>().All(col => !col.IsPresent));

            AddStep("clear search bar", () => screen.Overlay.SearchTerm = "");
            AddAssert("all columns visible", () => this.ChildrenOfType<ModColumn>().All(col => col.IsPresent));
        }

        [Test]
        public void TestHidingOverlayClearsTextSearch()
        {
            createScreen();
            changeRuleset(0);

            AddAssert("all columns visible", () => this.ChildrenOfType<ModColumn>().All(col => col.IsPresent));

            AddStep("set search", () => screen.Overlay.SearchTerm = "fail");
            AddAssert("one column visible", () => this.ChildrenOfType<ModColumn>().Count(col => col.IsPresent) == 1);

            AddStep("hide", () => screen.Overlay.Hide());
            AddStep("show", () => screen.Overlay.Show());

            AddAssert("all columns visible", () => this.ChildrenOfType<ModColumn>().All(col => col.IsPresent));
        }

        [Test]
        public void TestColumnHidingOnRulesetChange()
        {
            createScreen();

            changeRuleset(0);
            AddAssert("5 columns visible", () => this.ChildrenOfType<ModColumn>().Count(col => col.IsPresent) == 5);

            AddStep("change to ruleset without all mod types", () => Ruleset.Value = TestCustomisableModRuleset.CreateTestRulesetInfo());
            AddUntilStep("1 column visible", () => this.ChildrenOfType<ModColumn>().Count(col => col.IsPresent) == 1);

            changeRuleset(0);
            AddAssert("5 columns visible", () => this.ChildrenOfType<ModColumn>().Count(col => col.IsPresent) == 5);
        }

        [Test]
        public void TestModMultiplierUpdates()
        {
            createScreen();

            AddStep("select mod preset with half time", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<ModPresetPanel>().Single(preset => preset.Preset.Value.Name == "Half Time 0.5x"));
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("difficulty multiplier display shows correct value",
                () => this.ChildrenOfType<RankingInformationDisplay>().Single().ModMultiplier.Value, () => Is.EqualTo(0.1).Within(Precision.DOUBLE_EPSILON));

            // this is highly unorthodox in a test, but because the `ModSettingChangeTracker` machinery heavily leans on events and object disposal and re-creation,
            // it is instrumental in the reproduction of the failure scenario that this test is supposed to cover.
            AddStep("force collection", GC.Collect);

            AddStep("open customisation area", () => screen.Overlay.ChildrenOfType<ModCustomisationHeader>().Single().TriggerClick());
            AddStep("reset half time speed to default", () => screen.Overlay.ChildrenOfType<ModCustomisationPanel>().Single()
                                                                    .ChildrenOfType<RevertToDefaultButton<double>>().Single().TriggerClick());
            AddUntilStep("difficulty multiplier display shows correct value",
                () => this.ChildrenOfType<RankingInformationDisplay>().Single().ModMultiplier.Value, () => Is.EqualTo(0.3).Within(Precision.DOUBLE_EPSILON));
        }

        [Test]
        public void TestModSettingsOrder()
        {
            createScreen();

            AddStep("select DT + HD + DF", () => SelectedMods.Value = new Mod[] { new OsuModDoubleTime(), new OsuModHidden(), new OsuModDeflate() });
            AddStep("open customisation panel", () => this.ChildrenOfType<ModCustomisationHeader>().Single().TriggerClick());
            AddAssert("mod settings order: DT, HD, DF", () =>
            {
                var columns = this.ChildrenOfType<ModCustomisationSection>();
                return columns.ElementAt(0).Mod is OsuModDoubleTime &&
                       columns.ElementAt(1).Mod is OsuModHidden &&
                       columns.ElementAt(2).Mod is OsuModDeflate;
            });

            AddStep("replace DT with NC", () =>
            {
                SelectedMods.Value = SelectedMods.Value.Where(m => m is not ModDoubleTime).Append(new OsuModNightcore()).ToList();
                this.ChildrenOfType<ModCustomisationHeader>().Single().TriggerClick();
            });
            AddAssert("mod settings order: NC, HD, DF", () =>
            {
                var columns = this.ChildrenOfType<ModCustomisationSection>();
                return columns.ElementAt(0).Mod is OsuModNightcore &&
                       columns.ElementAt(1).Mod is OsuModHidden &&
                       columns.ElementAt(2).Mod is OsuModDeflate;
            });
        }

        [Test]
        public void TestOpeningCustomisationHidesPresetPopover()
        {
            createScreen();

            AddStep("select DT", () => SelectedMods.Value = new Mod[] { new OsuModDoubleTime() });
            AddStep("click new preset", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<AddPresetButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("preset popover shown", () => this.ChildrenOfType<AddPresetPopover>().SingleOrDefault()?.IsPresent, () => Is.True);

            AddStep("click customisation header", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<ModCustomisationHeader>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("preset popover hidden", () => this.ChildrenOfType<AddPresetPopover>().SingleOrDefault()?.IsPresent, () => Is.Not.True);
            AddAssert("customisation panel shown", () => this.ChildrenOfType<ModCustomisationPanel>().Single().State.Value, () => Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestCustomisationPanelAbsorbsInput([Values] bool textSearchStartsActive)
        {
            AddStep($"text search starts active = {textSearchStartsActive}", () => configManager.SetValue(OsuSetting.ModSelectTextSearchStartsActive, textSearchStartsActive));
            createScreen();

            AddStep("select DT", () => SelectedMods.Value = new Mod[] { new OsuModDoubleTime() });
            AddStep("open customisation panel", () => InputManager.MoveMouseTo(this.ChildrenOfType<ModCustomisationHeader>().Single()));
            AddAssert("search lost focus", () => !this.ChildrenOfType<ShearedSearchTextBox>().Single().HasFocus);

            AddStep("press tab", () => InputManager.Key(Key.Tab));
            AddAssert("search still not focused", () => !this.ChildrenOfType<ShearedSearchTextBox>().Single().HasFocus);

            AddStep("press q", () => InputManager.Key(Key.Q));
            AddAssert("easy not selected", () => SelectedMods.Value.Single() is OsuModDoubleTime);

            // the "deselect all mods" action is intentionally disabled when customisation panel is open to not conflict with pressing backspace to delete characters in a textbox.
            // this is supposed to be handled by the textbox itself especially since it's focused and thus prioritised in input queue,
            // but it's not for some reason, and figuring out why is probably not going to be a pleasant experience (read TextBox.OnKeyDown for a head start).
            AddStep("press backspace", () => InputManager.Key(Key.BackSpace));
            AddAssert("mods not deselected", () => SelectedMods.Value.Single() is OsuModDoubleTime);

            AddStep("move mouse to customisation panel", () => InputManager.MoveMouseTo(screen.Overlay.ChildrenOfType<ModCustomisationSection>().First()));

            AddStep("scroll down", () => InputManager.ScrollVerticalBy(-10f));
            AddAssert("column not scrolled", () => screen.Overlay.ChildrenOfType<ModSelectOverlay.ColumnScrollContainer>().Single().IsScrolledToStart());

            AddStep("move mouse away", () => InputManager.MoveMouseTo(Vector2.Zero));
            AddUntilStep("customisation panel closed",
                () => this.ChildrenOfType<ModCustomisationPanel>().Single().ExpandedState.Value,
                () => Is.EqualTo(ModCustomisationPanel.ModCustomisationPanelState.Collapsed));

            if (textSearchStartsActive)
                AddAssert("search focused", () => this.ChildrenOfType<ShearedSearchTextBox>().Single().HasFocus);
            else
                AddAssert("search still not focused", () => !this.ChildrenOfType<ShearedSearchTextBox>().Single().HasFocus);
        }

        /// <summary>
        /// Tests that recreating the mod panels (by setting the global available mods) also refreshes the active states.
        /// </summary>
        [Test]
        public void TestActiveStatesRefreshedOnPanelsCreated()
        {
            createScreen();
            changeRuleset(0);

            Bindable<IReadOnlyList<Mod>> selectedMods = null!;

            AddStep("bind mods to local bindable", () =>
            {
                selectedMods = new Bindable<IReadOnlyList<Mod>>([]);

                screen.SelectedMods.UnbindFrom(SelectedMods);
                screen.SelectedMods.BindTo(selectedMods);
            });

            AddStep("activate PF", () => selectedMods.Value = [new OsuModPerfect()]);
            AddAssert("OsuModPerfect panel active", () => getPanelForMod(typeof(OsuModPerfect)).Active.Value);

            changeRuleset(1);
            AddAssert("TaikoModPerfect panel not active", () => !getPanelForMod(typeof(TaikoModPerfect)).Active.Value);

            changeRuleset(0);
            AddAssert("OsuModPerfect panel active", () => getPanelForMod(typeof(OsuModPerfect)).Active.Value);
        }

        private void waitForColumnLoad() => AddUntilStep("all column content loaded", () =>
            screen.Overlay.ChildrenOfType<ModColumn>().Any()
            && screen.Overlay.ChildrenOfType<ModColumn>().All(column => column.IsLoaded && column.ItemsLoaded)
            && screen.Overlay.ChildrenOfType<ModPresetColumn>().Any()
            && screen.Overlay.ChildrenOfType<ModPresetColumn>().All(column => column.IsLoaded));

        private void changeRuleset(int id)
        {
            AddStep($"set ruleset to {id}", () => Ruleset.Value = rulesetStore.GetRuleset(id));
            waitForColumnLoad();
        }

        private void assertCustomisationToggleState(bool disabled, bool active)
        {
            AddUntilStep($"customisation panel is {(disabled ? "" : "not ")}disabled", () => screen.Overlay.ChildrenOfType<ModCustomisationPanel>().Single().Enabled.Value == !disabled);
            AddUntilStep($"customisation panel is {(active ? "" : "not ")}active",
                () => screen.Overlay.ChildrenOfType<ModCustomisationPanel>().Single().ExpandedState.Value,
                () => active ? Is.Not.EqualTo(ModCustomisationPanel.ModCustomisationPanelState.Collapsed) : Is.EqualTo(ModCustomisationPanel.ModCustomisationPanelState.Collapsed));
        }

        private T getSelectedMod<T>() where T : Mod => SelectedMods.Value.OfType<T>().Single();

        private ModPanel getPanelForMod(Type modType)
            => screen.Overlay.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.GetType() == modType);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (rulesetStore.IsNotNull())
                rulesetStore.Dispose();
        }

        private partial class TestModSelectOverlayScreen : OsuScreen
        {
            public readonly Bindable<IReadOnlyList<Mod>> SelectedMods = new Bindable<IReadOnlyList<Mod>>();

            public override bool ShowFooter => true;

            public ModSelectOverlay Overlay = null!;

            private IDisposable? firstOverlayRegistration;

            [Cached]
            private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

            [Resolved]
            protected IOverlayManager? OverlayManager { get; private set; }

            [BackgroundDependencyLoader]
            private void load()
            {
                LoadComponent(Overlay = new UserModSelectOverlay
                {
                    RelativeSizeAxes = Axes.Both,
                    State = { Value = Visibility.Visible },
                    Beatmap = { Value = Beatmap.Value },
                    SelectedMods = { BindTarget = SelectedMods },
                    ShowPresets = true,
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                firstOverlayRegistration = OverlayManager?.RegisterBlockingOverlay(Overlay);
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                firstOverlayRegistration?.Dispose();
            }
        }

        private partial class TestScreenWithTwoOverlays : TestModSelectOverlayScreen
        {
            public readonly Bindable<IReadOnlyList<Mod>> SelectedMods2 = new Bindable<IReadOnlyList<Mod>>([]);

            public ModSelectOverlay SecondOverlay = null!;

            private IDisposable? secondOverlayRegistration;

            [BackgroundDependencyLoader]
            private void load()
            {
                LoadComponent(SecondOverlay = new UserModSelectOverlay
                {
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    SelectedMods = { BindTarget = SelectedMods2 },
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                secondOverlayRegistration = OverlayManager?.RegisterBlockingOverlay(SecondOverlay);
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                secondOverlayRegistration?.Dispose();
            }
        }

        private class TestUnimplementedMod : Mod
        {
            public override string Name => "Unimplemented mod";
            public override string Acronym => "UM";
            public override LocalisableString Description => "A mod that is not implemented.";
            public override double ScoreMultiplier => 1;
            public override ModType Type => ModType.Conversion;
        }

        private class TestUnimplementedModOsuRuleset : OsuRuleset
        {
            public override string ShortName => "unimplemented";

            public override IEnumerable<Mod> GetModsFor(ModType type) =>
                type == ModType.Conversion
                    ? base.GetModsFor(type).Concat(new[] { new TestUnimplementedMod() })
                    : base.GetModsFor(type);
        }
    }
}
