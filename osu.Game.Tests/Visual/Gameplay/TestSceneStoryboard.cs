// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Overlays;
using osu.Game.Storyboards;
using osu.Game.Storyboards.Drawables;
using osu.Game.Tests.Resources;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneStoryboard : OsuTestScene
    {
        private Container<DrawableStoryboard> storyboardContainer;
        private DrawableStoryboard storyboard;

        [Test]
        public void TestStoryboard()
        {
            AddStep("Restart", restart);
            AddToggleStep("Passing", passing =>
            {
                if (storyboard != null) storyboard.Passing = passing;
            });
        }

        [Test]
        public void TestStoryboardMissingVideo()
        {
            AddStep("Load storyboard with missing video", loadStoryboardNoVideo);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Clock = new FramedClock();

            AddRange(new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                        },
                        storyboardContainer = new Container<DrawableStoryboard>
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    },
                },
                new NowPlayingOverlay
                {
                    Origin = Anchor.TopRight,
                    Anchor = Anchor.TopRight,
                    State = { Value = Visibility.Visible },
                }
            });

            Beatmap.BindValueChanged(beatmapChanged, true);
        }

        private void beatmapChanged(ValueChangedEvent<WorkingBeatmap> e) => loadStoryboard(e.NewValue);

        private void restart()
        {
            var track = Beatmap.Value.Track;

            track.Reset();
            loadStoryboard(Beatmap.Value);
            track.Start();
        }

        private void loadStoryboard(IWorkingBeatmap working)
        {
            if (storyboard != null)
                storyboardContainer.Remove(storyboard);

            var decoupledClock = new DecoupleableInterpolatingFramedClock { IsCoupled = true };
            storyboardContainer.Clock = decoupledClock;

            storyboard = working.Storyboard.CreateDrawable(Beatmap.Value);
            storyboard.Passing = false;

            storyboardContainer.Add(storyboard);
            decoupledClock.ChangeSource(working.Track);
        }

        private void loadStoryboardNoVideo()
        {
            if (storyboard != null)
                storyboardContainer.Remove(storyboard);

            var decoupledClock = new DecoupleableInterpolatingFramedClock { IsCoupled = true };
            storyboardContainer.Clock = decoupledClock;

            Storyboard sb;

            using (var str = TestResources.OpenResource("storyboard_no_video.osu"))
            using (var bfr = new LineBufferedReader(str))
            {
                var decoder = new LegacyStoryboardDecoder();
                sb = decoder.Decode(bfr);
            }

            storyboard = sb.CreateDrawable(Beatmap.Value);

            storyboardContainer.Add(storyboard);
            decoupledClock.ChangeSource(Beatmap.Value.Track);
        }
    }
}
