using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Screens.Select;

namespace osu.Game.Tests.Visual.Mvis
{
    public class TestSceneShareScreen : ScreenTestScene
    {
        [Cached]
        private MusicController musicController = new MusicController();

        [Resolved]
        private BeatmapManager manager { get; set; }

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(manager);
        }

        [Test]
        public void CreateMvisScreen()
        {
            AddStep("Create screen", () =>
            {
                if (Stack.CurrentScreen != null)
                    Stack?.Exit();

                LoadScreen(new BeatmapShareSongSelect());
            });
        }
    }
}
