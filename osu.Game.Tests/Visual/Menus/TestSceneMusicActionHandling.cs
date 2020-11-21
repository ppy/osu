// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual.Navigation;

namespace osu.Game.Tests.Visual.Menus
{
    public class TestSceneMusicActionHandling : OsuGameTestScene
    {
        private GlobalActionContainer globalActionContainer => Game.ChildrenOfType<GlobalActionContainer>().First();

        [Test]
        public void TestMusicPlayAction()
        {
            AddStep("ensure playing something", () => Game.MusicController.EnsurePlayingSomething());
            AddStep("toggle playback", () => globalActionContainer.TriggerPressed(GlobalAction.MusicPlay));
            AddAssert("music paused", () => !Game.MusicController.IsPlaying && Game.MusicController.UserPauseRequested);
            AddStep("toggle playback", () => globalActionContainer.TriggerPressed(GlobalAction.MusicPlay));
            AddAssert("music resumed", () => Game.MusicController.IsPlaying && !Game.MusicController.UserPauseRequested);
        }

        [Test]
        public void TestMusicNavigationActions()
        {
            int importId = 0;
            Queue<(WorkingBeatmap working, TrackChangeDirection changeDirection)> trackChangeQueue = null;

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
                trackChangeQueue = new Queue<(WorkingBeatmap, TrackChangeDirection)>();
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
