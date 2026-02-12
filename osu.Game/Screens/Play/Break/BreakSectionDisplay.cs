// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.Break
{
    public partial class BreakSectionDisplay : SkinReloadableDrawable
    {
        private const float fixed_size = 100;

        private Sprite textureSprite = null!;
        private OsuSpriteText fallbackText = null!;
        private SkinnableSound sectionSound = null!;
        private Container contentContainer = null!;

        private readonly string textureName;
        private readonly string sampleName;
        private readonly string text;

        public BreakSectionDisplay(bool isPass)
        {
            textureName = isPass ? "section-pass" : "section-fail";
            sampleName = isPass ? "sectionpass" : "sectionfail";
            text = isPass ? "PASS" : "FAIL";

            Size = new Vector2(fixed_size);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = contentContainer = new Container
            {
                RelativeSizeAxes = Axes.None,
                AutoSizeAxes = Axes.X,
                Height = fixed_size,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new Drawable[]
                {
                    textureSprite = new Sprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fill,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    fallbackText = new OsuSpriteText
                    {
                        Alpha = 0,
                        Text = text,
                        Font = OsuFont.Torus.With(size: 64, weight: FontWeight.Bold),
                        Spacing = new Vector2(-1, 0),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Shadow = true,
                    },
                    sectionSound = new SkinnableSound(new[]
                    {
                        new SampleInfo(sampleName),
                        new SampleInfo($"Gameplay/{sampleName}")
                    })
                }
            };
        }

        protected override void SkinChanged(ISkinSource skin)
        {
            base.SkinChanged(skin);

            var tex = skin.GetTexture(textureName);

            if (tex != null)
            {
                textureSprite.Texture = tex;
                textureSprite.Alpha = 1;
                fallbackText.Alpha = 0;
            }
            else
            {
                textureSprite.Alpha = 0;
                fallbackText.Alpha = 1;
            }
        }

        public void PlayAnimation()
        {
            Stop();

            this.FadeIn();

            const byte blink_duration = 60;
            const byte pause_duration = 100;

            contentContainer.ScaleTo(0.8f).ScaleTo(1.05f, blink_duration * 4, Easing.OutElasticHalf);

            // Полная анимация со звуком
            this.FadeIn(blink_duration)
                .Delay(pause_duration).FadeOut(blink_duration)
                .Delay(pause_duration).FadeIn(blink_duration)
                .Delay(1000)
                .FadeOut(800, Easing.OutQuint);

            sectionSound.Play();
        }

        public void PlayQuickAnimation()
        {
            Stop();

            contentContainer.ScaleTo(0.8f).ScaleTo(1.05f, 50 * 4, Easing.OutElasticHalf);

            // Быстрая анимация без звука
            this.FadeIn(50)
                .Delay(500)
                .FadeOut(250, Easing.OutQuint);
        }

        public void Stop()
        {
            FinishTransforms(true);
            contentContainer.FinishTransforms(true);
            sectionSound.Stop();
            Alpha = 0;
            contentContainer.Scale = new Vector2(1f);
        }
    }
}
