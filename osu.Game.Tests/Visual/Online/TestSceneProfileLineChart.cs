// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays.Profile.Sections.Historical;
using osu.Framework.Graphics;
using System;
using osu.Game.Overlays;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Containers;
using static osu.Game.Users.User;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneProfileLineChart : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);

        public TestSceneProfileLineChart()
        {
            var values = new[]
            {
                new UserHistoryCount { Date = new DateTime(2010, 5, 1), Count = 1000 },
                new UserHistoryCount { Date = new DateTime(2010, 6, 1), Count = 20 },
                new UserHistoryCount { Date = new DateTime(2010, 7, 1), Count = 20000 },
                new UserHistoryCount { Date = new DateTime(2010, 8, 1), Count = 30 },
                new UserHistoryCount { Date = new DateTime(2010, 9, 1), Count = 50 },
                new UserHistoryCount { Date = new DateTime(2010, 10, 1), Count = 2000 },
                new UserHistoryCount { Date = new DateTime(2010, 11, 1), Count = 2100 }
            };

            AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Padding = new MarginPadding { Horizontal = 50 },
                    Child = new ProfileLineChart
                    {
                        Values = values
                    }
                }
            });
        }
    }
}
