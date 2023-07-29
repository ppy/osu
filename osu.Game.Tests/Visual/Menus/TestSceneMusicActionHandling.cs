// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.Menus
{
    public partial class TestSceneMusicActionHandling : OsuGameTestScene
    {
        private GlobalActionContainer globalActionContainer => Game.ChildrenOfType<GlobalActionContainer>().First();

        [Test]
        public void TestMusicPlayAction()
        {
            AddStep("ensure playing something", () => Game.MusicController.EnsurePlayingSomething());
            AddUntilStep("music playing", () => Game.MusicController.IsPlaying);
            AddStep("toggle playback", () => globalActionContainer.TriggerPressed(GlobalAction.MusicPlay));
            AddUntilStep("music paused", () => !Game.MusicController.IsPlaying && Game.MusicController.UserPauseRequested);
            AddStep("toggle playback", () => globalActionContainer.TriggerPressed(GlobalAction.MusicPlay));
            AddUntilStep("music resumed", () => Game.MusicController.IsPlaying && !Game.MusicController.UserPauseRequested);
        }

        [Test]
        public void TestMusicNavigationActions()
        {
            Queue<(IWorkingBeatmap working, TrackChangeDirection changeDirection)> trackChangeQueue = null;

            // ensure we have at least two beatmaps available to identify the direction the music controller navigated to.
            AddRepeatStep("import beatmap", () => Game.BeatmapManager.Import(TestResources.CreateTestBeatmapSetInfo()), 5);

            AddStep("import beatmap with track", () =>
            {
                var setWithTrack = Game.BeatmapManager.Import(new ImportTask(TestResources.GetTestBeatmapForImport())).GetResultSafely();
                setWithTrack?.PerformRead(s =>
                {
                    Beatmap.Value = Game.BeatmapManager.GetWorkingBeatmap(s.Beatmaps.First());
                });
            });

            AddStep("bind to track change", () =>
            {
                trackChangeQueue = new Queue<(IWorkingBeatmap, TrackChangeDirection)>();
                Game.MusicController.TrackChanged += (working, changeDirection) => trackChangeQueue.Enqueue((working, changeDirection));
            });

            AddStep("seek track to 6 second", () => Game.MusicController.SeekTo(6000));
            AddUntilStep("wait for current time to update", () => Game.MusicController.CurrentTrack.CurrentTime > 5000);

            AddStep("press previous", () => globalActionContainer.TriggerPressed(GlobalAction.MusicPrev));
            AddAssert("no track change", () => trackChangeQueue.Count == 0);
            AddUntilStep("track restarted", () => Game.MusicController.CurrentTrack.CurrentTime < 5000);

            AddStep("press previous", () => globalActionContainer.TriggerPressed(GlobalAction.MusicPrev));
            AddAssert("track changed to previous", () =>
                trackChangeQueue.Count == 1 &&
                trackChangeQueue.Dequeue().changeDirection == TrackChangeDirection.Prev);

            AddStep("press next", () => globalActionContainer.TriggerPressed(GlobalAction.MusicNext));
            AddAssert("track changed to next", () =>
                trackChangeQueue.Count == 1 &&
                trackChangeQueue.Dequeue().changeDirection == TrackChangeDirection.Next);
        }
    }
}
