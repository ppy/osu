// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Handlers.Tablet;
using osu.Framework.Platform;
using osu.Framework.Utils;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings.Sections.Input;

namespace osu.Game.Tests.Visual.Settings
{
    [TestFixture]
    public class TestSceneTabletSettings : OsuTestScene
    {
        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            var tabletHandler = new TestTabletHandler();

            tabletHandler.AreaOffset.Value = new Size(10, 10);
            tabletHandler.AreaSize.Value = new Size(100, 80);

            AddRange(new Drawable[]
            {
                new TabletSettings(tabletHandler)
                {
                    RelativeSizeAxes = Axes.None,
                    Width = SettingsPanel.WIDTH,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                }
            });

            AddStep("Test with wide tablet", () => tabletHandler.SetTabletSize(new Size(160, 100)));
            AddStep("Test with square tablet", () => tabletHandler.SetTabletSize(new Size(300, 300)));
            AddStep("Test with tall tablet", () => tabletHandler.SetTabletSize(new Size(100, 300)));
            AddStep("Test with very tall tablet", () => tabletHandler.SetTabletSize(new Size(100, 700)));
            AddStep("Test no tablet present", () => tabletHandler.SetTabletSize(System.Drawing.Size.Empty));
        }

        public class TestTabletHandler : ITabletHandler
        {
            private readonly Bindable<Size> tabletSize = new Bindable<Size>();

            public BindableSize AreaOffset { get; } = new BindableSize();
            public BindableSize AreaSize { get; } = new BindableSize();
            public IBindable<Size> TabletSize => tabletSize;
            public string DeviceName { get; private set; }
            public BindableBool Enabled { get; } = new BindableBool(true);

            public void SetTabletSize(Size size)
            {
                DeviceName = size != System.Drawing.Size.Empty ? $"test tablet T-{RNG.Next(999):000}" : string.Empty;
                tabletSize.Value = size;
            }
        }
    }
}
