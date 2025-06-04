// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Screens.Play;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Beatmaps.IO;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Navigation
{
    /// <summary>
    /// Tests copied out of `TestSceneScreenNavigation` which are specific to song select.
    /// These are for SongSelectV2. Eventually, the tests in the above class should be deleted along with old song select.
    /// </summary>
    public class TestSceneSongSelectNavigation : OsuGameTestScene
    {
        [TestCase(true)]
        [TestCase(false)]
        public void TestSongContinuesAfterExitPlayer(bool withUserPause)
        {
            Player? player = null;

            IWorkingBeatmap beatmap() => Game.Beatmap.Value;

            PushAndConfirm(() => new SoloSongSelect());

            AddStep("import beatmap", () => BeatmapImportHelper.LoadOszIntoOsu(Game, virtualTrack: true).WaitSafely());

            AddUntilStep("wait for selected", () => !Game.Beatmap.IsDefault);

            if (withUserPause)
                AddStep("pause", () => Game.Dependencies.Get<MusicController>().Stop(true));

            AddStep("press enter", () => InputManager.Key(Key.Enter));

            AddUntilStep("wait for player", () =>
            {
                DismissAnyNotifications();
                return (player = Game.ScreenStack.CurrentScreen as Player) != null;
            });

            AddUntilStep("wait for fail", () => player?.GameplayState.HasFailed, () => Is.True);

            AddUntilStep("wait for track stop", () => !Game.MusicController.IsPlaying);
            AddAssert("Ensure time before preview point", () => Game.MusicController.CurrentTrack.CurrentTime < beatmap().BeatmapInfo.Metadata.PreviewTime);

            pushEscape();

            AddUntilStep("wait for track playing", () => Game.MusicController.IsPlaying);
            AddAssert("Ensure time wasn't reset to preview point", () => Game.MusicController.CurrentTrack.CurrentTime < beatmap().BeatmapInfo.Metadata.PreviewTime);
        }

        private void pushEscape() =>
            AddStep("Press escape", () => InputManager.Key(Key.Escape));
    }
}
