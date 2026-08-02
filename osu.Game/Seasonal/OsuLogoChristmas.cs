// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Screens.Menu;
using osuTK;

namespace osu.Game.Seasonal
{
    public partial class OsuLogoChristmas : OsuLogo
    {
        protected override double BeatSampleVariance => 0.02;

        private Sprite? hat;

        private bool hasHat;

        protected override MenuLogoVisualisation CreateMenuLogoVisualisation() => new SeasonalMenuLogoVisualisation();

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, AudioManager audio)
        {
            LogoElements.Add(hat = new Sprite
            {
                BypassAutoSizeAxes = Axes.Both,
                Alpha = 0,
                Origin = Anchor.BottomCentre,
                Scale = new Vector2(-1, 1),
                Texture = textures.Get(@"Menu/hat"),
            });

            // override base samples with our preferred ones.
            SampleDownbeat = SampleBeat = audio.Samples.Get(@"Menu/osu-logo-heartbeat-bell");
        }

        protected override void Update()
        {
            base.Update();
            updateHat();
        }

        private void updateHat()
        {
            if (hat == null)
                return;

            bool shouldHat = DrawWidth * Scale.X < 400;

            if (shouldHat != hasHat)
            {
                hasHat = shouldHat;

                if (hasHat)
                {
                    hat.Delay(400)
                       .Then()
                       .MoveTo(new Vector2(120, 160))
                       .RotateTo(0)
                       .RotateTo(-20, 500, Easing.OutQuint)
                       .FadeIn(250, Easing.OutQuint);
                }
                else
                {
                    hat.Delay(100)
                       .Then()
                       .MoveToOffset(new Vector2(0, -5), 500, Easing.OutQuint)
                       .FadeOut(500, Easing.OutQuint);
                }
            }
        }
    }
}
