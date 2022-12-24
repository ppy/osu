// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneModsEffectDisplay : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Test]
        public void TestModsEffectDisplay()
        {
            TestDisplay testDisplay = null!;
            Box background = null!;

            AddStep("add display", () =>
            {
                Add(testDisplay = new TestDisplay
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                });
                var boxes = testDisplay.ChildrenOfType<Box>();
                background = boxes.First();
            });

            AddStep("set value to default", () => testDisplay.Current.Value = 50);
            AddUntilStep("colours are correct", () => testDisplay.Container.Colour == Color4.White && background.Colour == colourProvider.Background3);

            AddStep("set value to less", () => testDisplay.Current.Value = 40);
            AddUntilStep("colours are correct", () => testDisplay.Container.Colour == colourProvider.Background5 && background.Colour == colours.ForModType(ModType.DifficultyReduction));

            AddStep("set value to bigger", () => testDisplay.Current.Value = 60);
            AddUntilStep("colours are correct", () => testDisplay.Container.Colour == colourProvider.Background5 && background.Colour == colours.ForModType(ModType.DifficultyIncrease));
        }

        private partial class TestDisplay : ModsEffectDisplay
        {
            public Container<Drawable> Container => Content;

            protected override LocalisableString Label => "Test display";

            public TestDisplay()
            {
                Current.Default = 50;
            }
        }
    }
}
