// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.SelectV2;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneBeatmapFilterControl : SongSelectComponentsTestScene
    {
        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Child = new BeatmapFilterControl
                {
                    State = { Value = Visibility.Visible },
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                },
            };
        });
    }
}
