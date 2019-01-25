﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Storyboards.Drawables;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseStoryboard : OsuTestCase
    {
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
        private void load()
        {
            Beatmap.ValueChanged += beatmapChanged;
        }

        private void beatmapChanged(WorkingBeatmap working)
            => loadStoryboard(working);

        private void restart()
        {
            var track = Beatmap.Value.Track;

            track.Reset();
            loadStoryboard(Beatmap);
            track.Start();
        }

        private void loadStoryboard(WorkingBeatmap working)
        {
            if (storyboard != null)
                storyboardContainer.Remove(storyboard);

            var decoupledClock = new DecoupleableInterpolatingFramedClock { IsCoupled = true };
            storyboardContainer.Clock = decoupledClock;

            storyboard = working.Storyboard.CreateDrawable(Beatmap);
            storyboard.Passing = false;

            storyboardContainer.Add(storyboard);
            decoupledClock.ChangeSource(working.Track);
        }
    }
}
