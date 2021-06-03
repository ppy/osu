// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Handlers.Tablet;
using osu.Framework.Platform;
using osu.Framework.Utils;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings.Sections.Input;
using osuTK;

namespace osu.Game.Tests.Visual.Settings
{
    [TestFixture]
    public class TestSceneTabletSettings : OsuTestScene
    {
        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            var tabletHandler = new TestTabletHandler();

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

            AddStep("Test with wide tablet", () => tabletHandler.SetTabletSize(new Vector2(160, 100)));
            AddStep("Test with square tablet", () => tabletHandler.SetTabletSize(new Vector2(300, 300)));
            AddStep("Test with tall tablet", () => tabletHandler.SetTabletSize(new Vector2(100, 300)));
            AddStep("Test with very tall tablet", () => tabletHandler.SetTabletSize(new Vector2(100, 700)));
            AddStep("Test no tablet present", () => tabletHandler.SetTabletSize(Vector2.Zero));
        }

        public class TestTabletHandler : ITabletHandler
        {
            public Bindable<Vector2> AreaOffset { get; } = new Bindable<Vector2>();
            public Bindable<Vector2> AreaSize { get; } = new Bindable<Vector2>();

            public Bindable<float> Rotation { get; } = new Bindable<float>();

            public IBindable<TabletInfo> Tablet => tablet;

            private readonly Bindable<TabletInfo> tablet = new Bindable<TabletInfo>();

            public BindableBool Enabled { get; } = new BindableBool(true);

            public void SetTabletSize(Vector2 size)
            {
                tablet.Value = size != Vector2.Zero ? new TabletInfo($"test tablet T-{RNG.Next(999):000}", size) : null;

                AreaSize.Default = new Vector2(size.X, size.Y);

                // if it's clear the user has not configured the area, take the full area from the tablet that was just found.
                if (AreaSize.Value == Vector2.Zero)
                    AreaSize.SetDefault();

                AreaOffset.Default = new Vector2(size.X / 2, size.Y / 2);

                // likewise with the position, use the centre point if it has not been configured.
                // it's safe to assume no user would set their centre point to 0,0 for now.
                if (AreaOffset.Value == Vector2.Zero)
                    AreaOffset.SetDefault();
            }
        }
    }
}
