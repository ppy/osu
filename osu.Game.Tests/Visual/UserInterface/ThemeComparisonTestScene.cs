// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public abstract partial class ThemeComparisonTestScene : OsuManualInputManagerTestScene
    {
        private readonly bool showWithoutColourProvider;

        public Container ContentContainer { get; private set; } = null!;

        protected ThemeComparisonTestScene(bool showWithoutColourProvider = true)
        {
            this.showWithoutColourProvider = showWithoutColourProvider;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Child = ContentContainer = new Container
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                RelativeSizeAxes = Axes.Both,
            };

            if (showWithoutColourProvider)
            {
                ContentContainer.Size = new Vector2(0.5f, 1f);

                Add(new Container
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.5f, 1f),
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.GreySeaFoam
                        },
                        CreateContent()
                    }
                });
            }
        }

        protected void CreateThemedContent(OverlayColourScheme colourScheme)
        {
            var colourProvider = new OverlayColourProvider(colourScheme);

            ContentContainer.Clear();
            ContentContainer.Add(new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies = new (Type, object)[]
                {
                    (typeof(OverlayColourProvider), colourProvider)
                },
                Children = new[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background3
                    },
                    CreateContent()
                }
            });
        }

        protected abstract Drawable CreateContent();

        [Test]
        public void TestAllColourSchemes()
        {
            foreach (var scheme in Enum.GetValues(typeof(OverlayColourScheme)).Cast<OverlayColourScheme>())
                AddStep($"set {scheme} scheme", () => CreateThemedContent(scheme));
        }
    }
}
