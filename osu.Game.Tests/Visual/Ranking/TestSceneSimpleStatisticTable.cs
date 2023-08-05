// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using Humanizer;
using NUnit.Framework;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Screens.Ranking.Statistics;

namespace osu.Game.Tests.Visual.Ranking
{
    public partial class TestSceneSimpleStatisticTable : OsuTestScene
    {
        private Container container;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = new Container
            {
                AutoSizeAxes = Axes.Y,
                Width = 700,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4Extensions.FromHex("#333"),
                    },
                    container = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding(20)
                    }
                }
            };
        });

        [Test]
        public void TestEmpty()
        {
            AddStep("create with no items",
                () => container.Add(new SimpleStatisticTable(2, Enumerable.Empty<SimpleStatisticItem>())));
        }

        [Test]
        public void TestManyItems(
            [Values(1, 2, 3, 4, 12)] int itemCount,
            [Values(1, 3, 5)] int columnCount)
        {
            AddStep($"create with {"item".ToQuantity(itemCount)}", () =>
            {
                var items = Enumerable.Range(1, itemCount)
                                      .Select(i => new SimpleStatisticItem<int>($"Statistic #{i}")
                                      {
                                          Value = RNG.Next(100)
                                      });

                container.Add(new SimpleStatisticTable(columnCount, items));
            });
        }
    }
}
