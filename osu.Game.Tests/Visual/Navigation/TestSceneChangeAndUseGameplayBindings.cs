// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Input.Bindings;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Overlays.Settings.Sections.Input;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select;
using osu.Game.Tests.Beatmaps.IO;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Navigation
{
    public class TestSceneChangeAndUseGameplayBindings : OsuGameTestScene
    {
        [Test]
        public void TestGameplayKeyBindings()
        {
            AddAssert("databased key is default", () => firstOsuRulesetKeyBindings.KeyCombination.Keys.SequenceEqual(new[] { InputKey.Z }));

            AddStep("open settings", () => { Game.Settings.Show(); });

            // Until step requires as settings has a delayed load.
            AddUntilStep("wait for button", () => configureBindingsButton?.Enabled.Value == true);
            AddStep("scroll to section", () => Game.Settings.SectionsContainer.ScrollTo(configureBindingsButton));
            AddStep("press button", () => configureBindingsButton.TriggerClick());
            AddUntilStep("wait for panel", () => keyBindingPanel?.IsLoaded == true);
            AddUntilStep("wait for osu subsection", () => osuBindingSubsection?.IsLoaded == true);
            AddStep("scroll to section", () => keyBindingPanel.SectionsContainer.ScrollTo(osuBindingSubsection));
            AddWaitStep("wait for scroll to end", 3);
            AddStep("start rebinding first osu! key", () =>
            {
                var button = osuBindingSubsection.ChildrenOfType<KeyBindingRow>().First();

                InputManager.MoveMouseTo(button);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("Press 's'", () => InputManager.Key(Key.S));

            AddUntilStep("wait for database updated", () => firstOsuRulesetKeyBindings.KeyCombination.Keys.SequenceEqual(new[] { InputKey.S }));

            AddStep("close settings", () => Game.Settings.Hide());

            AddStep("import beatmap", () => BeatmapImportHelper.LoadQuickOszIntoOsu(Game).WaitSafely());

            PushAndConfirm(() => new PlaySongSelect());

            AddUntilStep("wait for selection", () => !Game.Beatmap.IsDefault);

            AddStep("enter gameplay", () => InputManager.Key(Key.Enter));

            AddUntilStep("wait for player", () =>
            {
                // dismiss any notifications that may appear (ie. muted notification).
                clickMouseInCentre();
                return player != null;
            });

            AddUntilStep("wait for gameplay", () => player?.IsBreakTime.Value == false);

            AddStep("press 'z'", () => InputManager.Key(Key.Z));
            AddAssert("key counter didn't increase", () => keyCounter.CountPresses == 0);

            AddStep("press 's'", () => InputManager.Key(Key.S));
            AddAssert("key counter did increase", () => keyCounter.CountPresses == 1);
        }

        private void clickMouseInCentre()
        {
            InputManager.MoveMouseTo(Game.ScreenSpaceDrawQuad.Centre);
            InputManager.Click(MouseButton.Left);
        }

        private KeyBindingsSubsection osuBindingSubsection => keyBindingPanel
                                                              .ChildrenOfType<VariantBindingsSubsection>()
                                                              .FirstOrDefault(s => s.Ruleset.ShortName == "osu");

        private OsuButton configureBindingsButton => Game.Settings
                                                         .ChildrenOfType<BindingSettings>().SingleOrDefault()?
                                                         .ChildrenOfType<OsuButton>()?
                                                         .First(b => b.Text.ToString() == "Configure");

        private KeyBindingPanel keyBindingPanel => Game.Settings
                                                       .ChildrenOfType<KeyBindingPanel>().SingleOrDefault();

        private RealmKeyBinding firstOsuRulesetKeyBindings => Game.Dependencies
                                                                  .Get<RealmAccess>().Realm
                                                                  .All<RealmKeyBinding>()
                                                                  .AsEnumerable()
                                                                  .First(k => k.RulesetName == "osu" && k.ActionInt == 0);

        private Player player => Game.ScreenStack.CurrentScreen as Player;

        private KeyCounter keyCounter => player.ChildrenOfType<KeyCounter>().First();
    }
}
