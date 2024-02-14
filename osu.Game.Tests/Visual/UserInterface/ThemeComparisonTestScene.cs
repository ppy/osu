// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.UserInterface
{
    public abstract partial class ThemeComparisonTestScene : OsuGridTestScene
    {
        private readonly bool showWithoutColourProvider;

        protected ThemeComparisonTestScene(bool showWithoutColourProvider = true)
            : base(1, showWithoutColourProvider ? 2 : 1)
        {
            this.showWithoutColourProvider = showWithoutColourProvider;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (showWithoutColourProvider)
            {
                Cell(0, 0).AddRange(new[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.GreySeaFoam
                    },
                    CreateContent()
                });
            }
        }

        protected void CreateThemedContent(OverlayColourScheme colourScheme)
        {
            var colourProvider = new OverlayColourProvider(colourScheme);

            int col = showWithoutColourProvider ? 1 : 0;

            Cell(0, col).Clear();
            Cell(0, col).Add(new DependencyProvidingContainer
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
                        Colour = colourProvider.Background4
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
