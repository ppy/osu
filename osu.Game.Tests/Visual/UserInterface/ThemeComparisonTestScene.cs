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
    public abstract class ThemeComparisonTestScene : OsuGridTestScene
    {
        protected ThemeComparisonTestScene()
            : base(1, 2)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
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

        protected void CreateThemedContent(OverlayColourScheme colourScheme)
        {
            var colourProvider = new OverlayColourProvider(colourScheme);

            Cell(0, 1).Clear();
            Cell(0, 1).Add(new DependencyProvidingContainer
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
