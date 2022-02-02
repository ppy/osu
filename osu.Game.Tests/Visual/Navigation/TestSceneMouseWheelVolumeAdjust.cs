// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Game.Configuration;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps.IO;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Navigation
{
    public class TestSceneMouseWheelVolumeAdjust : OsuGameTestScene
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            // Headless tests are always at minimum volume. This covers interactive tests, matching that initial value.
            AddStep("Set volume to min", () => Game.Audio.Volume.Value = 0);
            AddAssert("Volume is min", () => Game.Audio.AggregateVolume.Value == 0);
            AddStep("Move mouse to centre", () => InputManager.MoveMouseTo(Game.ScreenSpaceDrawQuad.Centre));
        }

        [Test]
        public void TestAdjustVolumeFromMainMenu()
        {
            // First scroll makes volume controls appear, second adjusts volume.
            AddUntilStep("Adjust volume using mouse wheel", () =>
            {
                InputManager.ScrollVerticalBy(5);
                return Game.Audio.AggregateVolume.Value > 0;
            });
        }

        [Test]
        public void TestAdjustVolumeFromPlayerWheelEnabled()
        {
            loadToPlayerNonBreakTime();

            // First scroll makes volume controls appear, second adjusts volume.
            AddUntilStep("Adjust volume using mouse wheel", () =>
            {
                InputManager.ScrollVerticalBy(5);
                return Game.Audio.AggregateVolume.Value > 0;
            });
            AddAssert("Volume is above zero", () => Game.Audio.Volume.Value > 0);
        }

        [Test]
        public void TestAdjustVolumeFromPlayerWheelDisabled()
        {
            AddStep("disable wheel volume adjust", () => Game.LocalConfig.SetValue(OsuSetting.MouseDisableWheel, true));

            loadToPlayerNonBreakTime();

            // First scroll makes volume controls appear, second adjusts volume.
            AddRepeatStep("Adjust volume using mouse wheel", () => InputManager.ScrollVerticalBy(5), 10);
            AddAssert("Volume is still zero", () => Game.Audio.Volume.Value == 0);
        }

        [Test]
        public void TestAdjustVolumeFromPlayerWheelDisabledHoldingAlt()
        {
            AddStep("disable wheel volume adjust", () => Game.LocalConfig.SetValue(OsuSetting.MouseDisableWheel, true));

            loadToPlayerNonBreakTime();

            // First scroll makes volume controls appear, second adjusts volume.
            AddUntilStep("Adjust volume using mouse wheel holding alt", () =>
            {
                InputManager.PressKey(Key.AltLeft);
                InputManager.ScrollVerticalBy(5);
                InputManager.ReleaseKey(Key.AltLeft);
                return Game.Audio.AggregateVolume.Value > 0;
            });
        }

        private void loadToPlayerNonBreakTime()
        {
            Player player = null;
            Screens.Select.SongSelect songSelect = null;
            PushAndConfirm(() => songSelect = new TestSceneScreenNavigation.TestPlaySongSelect());
            AddUntilStep("wait for song select", () => songSelect.BeatmapSetsLoaded);

            AddStep("import beatmap", () => BeatmapImportHelper.LoadOszIntoOsu(Game, virtualTrack: true).WaitSafely());
            AddUntilStep("wait for selected", () => !Game.Beatmap.IsDefault);
            AddStep("press enter", () => InputManager.Key(Key.Enter));

            AddUntilStep("wait for player", () =>
            {
                // dismiss any notifications that may appear (ie. muted notification).
                clickMouseInCentre();
                return (player = Game.ScreenStack.CurrentScreen as Player) != null;
            });

            AddUntilStep("wait for play time active", () => !player.IsBreakTime.Value);
        }

        private void clickMouseInCentre()
        {
            InputManager.MoveMouseTo(Game.ScreenSpaceDrawQuad.Centre);
            InputManager.Click(MouseButton.Left);
        }
    }
}
