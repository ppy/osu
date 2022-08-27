// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneModsEffectDisplay : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        [Test]
        public void TestEffectDisplay()
        {
            TestDisplay dsp;
            Add(dsp = new TestDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
            AddSliderStep("value", 40, 60, 50, i => dsp.Value = i);
        }

        private class TestDisplay : ModsEffectDisplay
        {
            private readonly OsuSpriteText text;

            protected override LocalisableString Label => "Test display";

            public int Value
            {
                set
                {
                    text.Text = value.ToString();
                    SetColours(value.CompareTo(50));
                }
            }

            public TestDisplay()
            {
                Add(text = new OsuSpriteText
                {
                    Font = OsuFont.Default.With(size: 17, weight: FontWeight.SemiBold),
                    Text = "50"
                });
            }

            [BackgroundDependencyLoader]
            private void load() => SetColours(0);
        }
    }
}
