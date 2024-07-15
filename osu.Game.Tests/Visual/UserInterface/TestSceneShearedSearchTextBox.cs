// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneShearedSearchTextBox : OsuTestScene
    {
        [Test]
        public void TestAllColourSchemes()
        {
            foreach (var scheme in Enum.GetValues(typeof(OverlayColourScheme)).Cast<OverlayColourScheme>())
                AddStep($"set {scheme} scheme", () => Child = createContent(scheme));
        }

        private Drawable createContent(OverlayColourScheme colourScheme)
        {
            var colourProvider = new OverlayColourProvider(colourScheme);

            return new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies = new (Type, object)[]
                {
                    (typeof(OverlayColourProvider), colourProvider)
                },
                Children = new Drawable[]
                {
                    new ShearedSearchTextBox
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        RelativeSizeAxes = Axes.X,
                        Width = 0.5f
                    }
                }
            };
        }
    }
}
