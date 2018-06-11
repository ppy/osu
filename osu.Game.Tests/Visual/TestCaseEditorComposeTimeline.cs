// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Screens.Edit.Screens.Compose.Timeline;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseEditorComposeTimeline : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(TimelineArea), typeof(Timeline), typeof(TimelineButton) };

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
                new TimelineArea
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    Size = new Vector2(0.8f, 100)
                }
            };
        }
    }
}
