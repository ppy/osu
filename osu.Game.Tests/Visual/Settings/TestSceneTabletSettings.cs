// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Handlers.Tablet;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Settings.Sections.Input;
using osuTK;

namespace osu.Game.Tests.Visual.Settings
{
    [TestFixture]
    public partial class TestSceneTabletSettings : OsuTestScene
    {
        private TestTabletHandler tabletHandler;
        private TabletSettings settings;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create settings", () =>
            {
                tabletHandler = new TestTabletHandler();

                Children = new Drawable[]
                {
                    new OsuScrollContainer(Direction.Vertical)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = settings = new TabletSettings(tabletHandler)
                        {
                            RelativeSizeAxes = Axes.None,
                            Width = SettingsPanel.PANEL_WIDTH,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                        }
                    }
                };
            });

            AddStep("set square size", () => tabletHandler.SetTabletSize(new Vector2(100, 100)));
        }

        [Test]
        public void TestVariousTabletSizes()
        {
            AddStep("Test with wide tablet", () => tabletHandler.SetTabletSize(new Vector2(160, 100)));
            AddStep("Test with square tablet", () => tabletHandler.SetTabletSize(new Vector2(300, 300)));
            AddStep("Test with tall tablet", () => tabletHandler.SetTabletSize(new Vector2(100, 300)));
            AddStep("Test with very tall tablet", () => tabletHandler.SetTabletSize(new Vector2(100, 700)));
            AddStep("Test no tablet present", () => tabletHandler.SetTabletSize(Vector2.Zero));
        }

        [Test]
        public void TestWideAspectRatioValidity()
        {
            AddStep("Test with wide tablet", () => tabletHandler.SetTabletSize(new Vector2(160, 100)));

            AddStep("Reset to full area", () => settings.ChildrenOfType<DangerousSettingsButton>().First().TriggerClick());
            ensureValid();

            AddStep("rotate 10", () => tabletHandler.Rotation.Value = 10);
            ensureInvalid();

            AddStep("scale down", () => tabletHandler.AreaSize.Value *= 0.9f);
            ensureInvalid();

            AddStep("scale down", () => tabletHandler.AreaSize.Value *= 0.9f);
            ensureInvalid();

            AddStep("scale down", () => tabletHandler.AreaSize.Value *= 0.9f);
            ensureValid();
        }

        [Test]
        public void TestRotationValidity()
        {
            AddAssert("area valid", () => settings.AreaSelection.IsWithinBounds);

            AddStep("rotate 90", () => tabletHandler.Rotation.Value = 90);
            ensureValid();

            AddStep("rotate 180", () => tabletHandler.Rotation.Value = 180);

            ensureValid();

            AddStep("rotate 270", () => tabletHandler.Rotation.Value = 270);

            ensureValid();

            AddStep("rotate 360", () => tabletHandler.Rotation.Value = 360);

            ensureValid();

            AddStep("rotate 0", () => tabletHandler.Rotation.Value = 0);
            ensureValid();

            AddStep("rotate 45", () => tabletHandler.Rotation.Value = 45);
            ensureInvalid();

            AddStep("rotate 0", () => tabletHandler.Rotation.Value = 0);
            ensureValid();
        }

        [Test]
        public void TestOffsetValidity()
        {
            ensureValid();
            AddStep("move right", () => tabletHandler.AreaOffset.Value = Vector2.Zero);
            ensureInvalid();
            AddStep("move back", () => tabletHandler.AreaOffset.Value = tabletHandler.AreaSize.Value / 2);
            ensureValid();
        }

        private void ensureValid() => AddAssert("area valid", () => settings.AreaSelection.IsWithinBounds);

        private void ensureInvalid() => AddAssert("area invalid", () => !settings.AreaSelection.IsWithinBounds);

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
