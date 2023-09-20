// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Database;
using osu.Game.Overlays;
using osu.Game.Tests.Scores.IO;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneMissingBeatmapNotification : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new Container
            {
                Width = 280,
                AutoSizeAxes = Axes.Y,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Child = new MissingBeatmapNotification(CreateAPIBeatmapSet(Ruleset.Value).Beatmaps.First(), new ImportScoreTest.TestArchiveReader(), "deadbeef")
            };
        }
    }
}
