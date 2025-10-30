// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
            Queue<(IWorkingBeatmap working, TrackChangeDirection changeDirection)> trackChangeQueue = null!;

            AddStep("disable shuffle", () => Game.MusicController.Shuffle.Value = false);

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
            AddUntilStep("track changed to previous", () =>
                trackChangeQueue.Count == 1 &&
                trackChangeQueue.Dequeue().changeDirection == TrackChangeDirection.Prev);

            AddStep("press next", () => globalActionContainer.TriggerPressed(GlobalAction.MusicNext));
            AddUntilStep("track changed to next", () =>
                trackChangeQueue.Count == 1 &&
                trackChangeQueue.Peek().changeDirection == TrackChangeDirection.Next);

            AddUntilStep("wait until track switches", () => trackChangeQueue.Count == 2);

            AddStep("press next", () => globalActionContainer.TriggerPressed(GlobalAction.MusicNext));
            AddUntilStep("track changed to next", () =>
                trackChangeQueue.Count == 3 &&
                trackChangeQueue.Peek().changeDirection == TrackChangeDirection.Next);
            AddAssert("track actually changed", () => !trackChangeQueue.First().working.BeatmapInfo.Equals(trackChangeQueue.Last().working.BeatmapInfo));
        }

        [Test]
        public void TestShuffleBackwards()
        {
            Queue<(IWorkingBeatmap working, TrackChangeDirection changeDirection)> trackChangeQueue = null!;

            AddStep("enable shuffle", () => Game.MusicController.Shuffle.Value = true);

            // ensure we have at least two beatmaps available to identify the direction the music controller navigated to.
            AddRepeatStep("import beatmap", () => Game.BeatmapManager.Import(TestResources.CreateTestBeatmapSetInfo()), 5);
            AddStep("ensure nonzero track duration", () => Game.Realm.Write(r =>
            {
                // this was already supposed to be non-zero (see innards of `TestResources.CreateTestBeatmapSetInfo()`),
                // but the non-zero value is being overwritten *to* zero by `BeatmapUpdater`.
                // do a bit of a hack to change it back again - otherwise tracks are going to switch instantly and we won't be able to assert anything sane anymore.
                foreach (var beatmap in r.All<BeatmapInfo>().Where(b => b.Length == 0))
                    beatmap.Length = 60_000;
            }));

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
            AddUntilStep("track changed", () => trackChangeQueue.Count == 1);

            AddStep("press previous", () => globalActionContainer.TriggerPressed(GlobalAction.MusicPrev));
            AddUntilStep("track changed", () => trackChangeQueue.Count == 2);

            AddStep("press next", () => globalActionContainer.TriggerPressed(GlobalAction.MusicNext));
            AddUntilStep("track changed", () =>
                trackChangeQueue.Count == 3 && !trackChangeQueue.ElementAt(1).working.BeatmapInfo.Equals(trackChangeQueue.Last().working.BeatmapInfo));
        }

        [Test]
        public void TestShuffleForwards()
        {
            Queue<(IWorkingBeatmap working, TrackChangeDirection changeDirection)> trackChangeQueue = null!;

            AddStep("enable shuffle", () => Game.MusicController.Shuffle.Value = true);

            // ensure we have at least two beatmaps available to identify the direction the music controller navigated to.
            AddRepeatStep("import beatmap", () => Game.BeatmapManager.Import(TestResources.CreateTestBeatmapSetInfo()), 5);
            AddStep("ensure nonzero track duration", () => Game.Realm.Write(r =>
            {
                // this was already supposed to be non-zero (see innards of `TestResources.CreateTestBeatmapSetInfo()`),
                // but the non-zero value is being overwritten *to* zero by `BeatmapUpdater`.
                // do a bit of a hack to change it back again - otherwise tracks are going to switch instantly and we won't be able to assert anything sane anymore.
                foreach (var beatmap in r.All<BeatmapInfo>().Where(b => b.Length == 0))
                    beatmap.Length = 60_000;
            }));

            AddStep("bind to track change", () =>
            {
                trackChangeQueue = new Queue<(IWorkingBeatmap, TrackChangeDirection)>();
                Game.MusicController.TrackChanged += (working, changeDirection) => trackChangeQueue.Enqueue((working, changeDirection));
            });

            AddStep("press next", () => globalActionContainer.TriggerPressed(GlobalAction.MusicNext));
            AddUntilStep("track changed", () => trackChangeQueue.Count == 1);

            AddStep("press next", () => globalActionContainer.TriggerPressed(GlobalAction.MusicNext));
            AddUntilStep("track changed", () => trackChangeQueue.Count == 2);

            AddStep("press previous", () => globalActionContainer.TriggerPressed(GlobalAction.MusicPrev));
            AddUntilStep("track changed", () =>
                trackChangeQueue.Count == 3 && !trackChangeQueue.ElementAt(1).working.BeatmapInfo.Equals(trackChangeQueue.Last().working.BeatmapInfo));
        }

        [Test]
        public void TestShuffleBackAndForth()
        {
            Queue<(IWorkingBeatmap working, TrackChangeDirection changeDirection)> trackChangeQueue = null!;

            AddStep("enable shuffle", () => Game.MusicController.Shuffle.Value = true);

            // ensure we have at least two beatmaps available to identify the direction the music controller navigated to.
            AddRepeatStep("import beatmap", () => Game.BeatmapManager.Import(TestResources.CreateTestBeatmapSetInfo()), 5);
            AddStep("ensure nonzero track duration", () => Game.Realm.Write(r =>
            {
                // this was already supposed to be non-zero (see innards of `TestResources.CreateTestBeatmapSetInfo()`),
                // but the non-zero value is being overwritten *to* zero by `BeatmapUpdater`.
                // do a bit of a hack to change it back again - otherwise tracks are going to switch instantly and we won't be able to assert anything sane anymore.
                foreach (var beatmap in r.All<BeatmapInfo>().Where(b => b.Length == 0))
                    beatmap.Length = 60_000;
            }));

            AddStep("bind to track change", () =>
            {
                trackChangeQueue = new Queue<(IWorkingBeatmap, TrackChangeDirection)>();
                Game.MusicController.TrackChanged += (working, changeDirection) => trackChangeQueue.Enqueue((working, changeDirection));
            });

            AddStep("press next", () => globalActionContainer.TriggerPressed(GlobalAction.MusicNext));
            AddUntilStep("track changed", () => trackChangeQueue.Count == 1);

            AddStep("press previous", () => globalActionContainer.TriggerPressed(GlobalAction.MusicPrev));
            AddUntilStep("new track selected", () =>
                trackChangeQueue.Count == 2 && !trackChangeQueue.First().working.BeatmapInfo.Equals(trackChangeQueue.Last().working.BeatmapInfo));

            AddStep("press next", () => globalActionContainer.TriggerPressed(GlobalAction.MusicNext));
            AddUntilStep("first track selected",
                () => trackChangeQueue.Count == 3 && trackChangeQueue.First().working.BeatmapInfo.Equals(trackChangeQueue.Last().working.BeatmapInfo));
        }
    }
}
