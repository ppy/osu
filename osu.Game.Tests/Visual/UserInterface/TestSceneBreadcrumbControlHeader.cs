// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneBreadcrumbControlHeader : OsuTestScene
    {
        private static readonly string[] items = { "first", "second", "third", "fourth", "fifth" };

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Red);

        private TestHeader header;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = header = new TestHeader
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        });

        [Test]
        public void TestAddAndRemoveItem()
        {
            foreach (string item in items.Skip(1))
                AddStep($"Add {item} item", () => header.AddItem(item));

            foreach (string item in items.Reverse().SkipLast(3))
                AddStep($"Remove {item} item", () => header.RemoveItem(item));

            AddStep("Clear items", () => header.ClearItems());

            foreach (string item in items)
                AddStep($"Add {item} item", () => header.AddItem(item));

            foreach (string item in items)
                AddStep($"Remove {item} item", () => header.RemoveItem(item));
        }

        private class TestHeader : BreadcrumbControlOverlayHeader
        {
            public TestHeader()
            {
                TabControl.AddItem(items[0]);
                Current.Value = items[0];
            }

            public void AddItem(string value)
            {
                TabControl.AddItem(value);
                Current.Value = TabControl.Items.LastOrDefault();
            }

            public void RemoveItem(string value)
            {
                TabControl.RemoveItem(value);
                Current.Value = TabControl.Items.LastOrDefault();
            }

            public void ClearItems()
            {
                TabControl.Clear();
                Current.Value = null;
            }

            protected override OverlayTitle CreateTitle() => new TestTitle();
        }

        private class TestTitle : OverlayTitle
        {
            public TestTitle()
            {
                Title = "Test Title";
            }
        }
    }
}
