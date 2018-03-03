// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Storyboards.Drawables;
using OpenTK.Graphics;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseStoryboard : OsuTestCase
    {
        private readonly Bindable<WorkingBeatmap> beatmapBacking = new Bindable<WorkingBeatmap>();

        private readonly Container<DrawableStoryboard> storyboardContainer;
        private DrawableStoryboard storyboard;

        public TestCaseStoryboard()
        {
            Clock = new FramedClock();

            Add(new Container
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
            });
            Add(new MusicController
            {
                Origin = Anchor.TopRight,
                Anchor = Anchor.TopRight,
                State = Visibility.Visible,
            });

            AddStep("Restart", restart);
            AddToggleStep("Passing", passing => { if (storyboard != null) storyboard.Passing = passing; });
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            beatmapBacking.BindTo(game.Beatmap);
            beatmapBacking.ValueChanged += beatmapChanged;
        }

        private void beatmapChanged(WorkingBeatmap working)
            => loadStoryboard(working);

        private void restart()
        {
            var track = beatmapBacking.Value.Track;

            track.Reset();
            loadStoryboard(beatmapBacking.Value);
            track.Start();
        }

        private void loadStoryboard(WorkingBeatmap working)
        {
            if (storyboard != null)
                storyboardContainer.Remove(storyboard);

            var decoupledClock = new DecoupleableInterpolatingFramedClock { IsCoupled = true };
            storyboardContainer.Clock = decoupledClock;

            storyboard = working.Storyboard.CreateDrawable(beatmapBacking);
            storyboard.Passing = false;

            storyboardContainer.Add(storyboard);
            decoupledClock.ChangeSource(working.Track);
        }
    }
}
