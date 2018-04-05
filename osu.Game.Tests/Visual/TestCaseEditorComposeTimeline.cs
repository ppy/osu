// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Screens.Edit.Screens.Compose.Timeline;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseEditorComposeTimeline : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(TimelineContainer), typeof(Timeline), typeof(BeatmapWaveformGraph), typeof(TimelineButton) };

        private readonly TimelineContainer timelineContainer;

        public TestCaseEditorComposeTimeline()
        {
            Children = new Drawable[]
            {
                new MusicController
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    State = Visibility.Visible
                },
                timelineContainer = new TimelineContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Size = new Vector2(0.8f, 100)
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame)
        {
            timelineContainer.Beatmap.BindTo(osuGame.Beatmap);
        }
    }
}
