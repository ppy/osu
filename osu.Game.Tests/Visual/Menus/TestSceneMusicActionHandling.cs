// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual.Navigation;

namespace osu.Game.Tests.Visual.Menus
{
    public class TestSceneMusicActionHandling : OsuGameTestScene
    {
        [Test]
        public void TestMusicPlayAction()
        {
            AddStep("ensure playing something", () => Game.MusicController.EnsurePlayingSomething());
            AddStep("trigger music playback toggle action", () => Game.GlobalBinding.TriggerPressed(GlobalAction.MusicPlay));
            AddAssert("music paused", () => !Game.MusicController.IsPlaying && Game.MusicController.IsUserPaused);
            AddStep("trigger music playback toggle action", () => Game.GlobalBinding.TriggerPressed(GlobalAction.MusicPlay));
            AddAssert("music resumed", () => Game.MusicController.IsPlaying && !Game.MusicController.IsUserPaused);
        }

        [Test]
        public void TestMusicNavigationActions()
        {
            int importId = 0;
            Queue<(WorkingBeatmap working, TrackChangeDirection dir)> trackChangeQueue = null;

            // ensure we have at least two beatmaps available to identify the direction the music controller navigated to.
            AddRepeatStep("import beatmap", () => Game.BeatmapManager.Import(new BeatmapSetInfo
            {
                Beatmaps = new List<BeatmapInfo>
                {
                    new BeatmapInfo
                    {
                        BaseDifficulty = new BeatmapDifficulty(),
                    }
                },
                Metadata = new BeatmapMetadata
                {
                    Artist = $"a test map {importId++}",
                    Title = "title",
                }
            }).Wait(), 5);

            AddStep("import beatmap with track", () =>
            {
                var setWithTrack = Game.BeatmapManager.Import(TestResources.GetTestBeatmapForImport()).Result;
                Beatmap.Value = Game.BeatmapManager.GetWorkingBeatmap(setWithTrack.Beatmaps.First());
            });

            AddStep("bind to track change", () =>
            {
                trackChangeQueue = new Queue<(WorkingBeatmap working, TrackChangeDirection dir)>();
                Game.MusicController.TrackChanged += (working, dir) => trackChangeQueue.Enqueue((working, dir));
            });

            AddStep("seek track to 6 second", () => Game.MusicController.SeekTo(6000));
            AddUntilStep("wait for current time to update", () => Game.MusicController.CurrentTrack.CurrentTime > 5000);

            AddStep("trigger music prev action", () => Game.GlobalBinding.TriggerPressed(GlobalAction.MusicPrev));
            AddAssert("no track change", () => trackChangeQueue.Count == 0);
            AddUntilStep("track restarted", () => Game.MusicController.CurrentTrack.CurrentTime < 5000);

            AddStep("trigger music prev action", () => Game.GlobalBinding.TriggerPressed(GlobalAction.MusicPrev));
            AddAssert("track changed to previous", () =>
                trackChangeQueue.Count == 1 &&
                trackChangeQueue.Dequeue().dir == TrackChangeDirection.Prev);

            AddStep("trigger music next action", () => Game.GlobalBinding.TriggerPressed(GlobalAction.MusicNext));
            AddAssert("track changed to next", () =>
                trackChangeQueue.Count == 1 &&
                trackChangeQueue.Dequeue().dir == TrackChangeDirection.Next);
        }
    }
}
