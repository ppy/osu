// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Screens.Menu;

namespace osu.Game.Tests.Visual.Menus
{
    [HeadlessTest]
    [TestFixture]
    public partial class TestSceneIntroIntegrity : IntroTestScene
    {
        [Test]
        public virtual void TestDeletedFilesRestored()
        {
            RestartIntro();
            WaitForMenu();

            AddStep("delete game files unexpectedly", () => LocalStorage.DeleteDirectory("files"));
            AddStep("reset game beatmap", () => Dependencies.Get<Bindable<WorkingBeatmap>>().Value = new DummyWorkingBeatmap(Audio, null));
            AddStep("invalidate beatmap from cache", () => Dependencies.Get<IWorkingBeatmapCache>().Invalidate(Intro.Beatmap.Value.BeatmapSetInfo));

            RestartIntro();
            WaitForMenu();

            AddUntilStep("ensure track is not virtual", () => Intro.Beatmap.Value.Track is TrackBass);
        }

        protected override bool IntroReliesOnTrack => true;
        protected override IntroScreen CreateScreen() => new IntroTriangles();
    }
}
