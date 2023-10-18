// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Timing;
using osu.Framework.Utils;
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
    public partial class TestSceneStoryboard : OsuTestScene
    {
        private Container<DrawableStoryboard> storyboardContainer = null!;

        private DrawableStoryboard? storyboard;

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
            AddStep("Load storyboard with missing video", () => loadStoryboard("storyboard_no_video.osu"));
        }

        [Test]
        public void TestVideoSize()
        {
            AddStep("load storyboard with only video", () =>
            {
                // LegacyStoryboardDecoder doesn't parse WidescreenStoryboard, so it is set manually
                loadStoryboard("storyboard_only_video.osu", s => s.BeatmapInfo.WidescreenStoryboard = false);
            });

            AddAssert("storyboard is correct width", () => Precision.AlmostEquals(storyboard?.Width ?? 0f, 480 * 16 / 9f));
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

        private void beatmapChanged(ValueChangedEvent<WorkingBeatmap> e) => loadStoryboard(e.NewValue.Storyboard);

        private void restart()
        {
            var track = Beatmap.Value.Track;

            track.Reset();
            loadStoryboard(Beatmap.Value.Storyboard);
            track.Start();
        }

        private void loadStoryboard(Storyboard toLoad)
        {
            if (storyboard != null)
                storyboardContainer.Remove(storyboard, true);

            storyboardContainer.Clock = new FramedClock(Beatmap.Value.Track);

            storyboard = toLoad.CreateDrawable(SelectedMods.Value);
            storyboard.Passing = false;

            storyboardContainer.Add(storyboard);
        }

        private void loadStoryboard(string filename, Action<Storyboard>? setUpStoryboard = null)
        {
            Storyboard loaded;

            using (var str = TestResources.OpenResource(filename))
            using (var bfr = new LineBufferedReader(str))
            {
                var decoder = new LegacyStoryboardDecoder();
                loaded = decoder.Decode(bfr);
            }

            setUpStoryboard?.Invoke(loaded);

            loadStoryboard(loaded);
        }
    }
}
