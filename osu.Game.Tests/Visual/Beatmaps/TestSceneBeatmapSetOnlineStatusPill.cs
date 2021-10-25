// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Overlays;
using osu.Game.Tests.Visual.UserInterface;
using osuTK;

namespace osu.Game.Tests.Visual.Beatmaps
{
    public class TestSceneBeatmapSetOnlineStatusPill : ThemeComparisonTestScene
    {
        protected override Drawable CreateContent() => new FillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(0, 10),
            ChildrenEnumerable = Enum.GetValues(typeof(BeatmapSetOnlineStatus)).Cast<BeatmapSetOnlineStatus>().Select(status => new BeatmapSetOnlineStatusPill
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Status = status
            })
        };

        private IEnumerable<BeatmapSetOnlineStatusPill> statusPills => this.ChildrenOfType<BeatmapSetOnlineStatusPill>();

        [Test]
        public void TestFixedWidth()
        {
            AddStep("create themed content", () => CreateThemedContent(OverlayColourScheme.Red));

            AddStep("set fixed width", () => statusPills.ForEach(pill =>
            {
                pill.AutoSizeAxes = Axes.Y;
                pill.Width = 90;
            }));
            AddStep("unset fixed width", () => statusPills.ForEach(pill => pill.AutoSizeAxes = Axes.Both));
        }
    }
}
