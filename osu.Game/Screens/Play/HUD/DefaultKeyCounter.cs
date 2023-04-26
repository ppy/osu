// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class DefaultKeyCounter : KeyCounter
    {
        private Sprite buttonSprite = null!;
        private Sprite glowSprite = null!;
        private Container textLayer = null!;
        private SpriteText countSpriteText = null!;

        //further: change default values here and in KeyCounterCollection if needed, instead of passing them in every constructor
        public Color4 KeyDownTextColor { get; set; } = Color4.DarkGray;
        public Color4 KeyUpTextColor { get; set; } = Color4.White;
        public double FadeTime { get; set; }

        public DefaultKeyCounter(InputTrigger trigger)
            : base(trigger)
        {
        }

        [BackgroundDependencyLoader(true)]
        private void load(TextureStore textures)
        {
            Children = new Drawable[]
            {
                buttonSprite = new Sprite
                {
                    Texture = textures.Get(@"KeyCounter/key-up"),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                glowSprite = new Sprite
                {
                    Texture = textures.Get(@"KeyCounter/key-glow"),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0
                },
                textLayer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = Trigger.Name,
                            Font = OsuFont.Numeric.With(size: 12),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativePositionAxes = Axes.Both,
                            Position = new Vector2(0, -0.25f),
                            Colour = KeyUpTextColor
                        },
                        countSpriteText = new OsuSpriteText
                        {
                            Text = CountPresses.Value.ToString(@"#,0"),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativePositionAxes = Axes.Both,
                            Position = new Vector2(0, 0.25f),
                            Colour = KeyUpTextColor
                        }
                    }
                }
            };
            // Set this manually because an element with Alpha=0 won't take it size to AutoSizeContainer,
            // so the size can be changing between buttonSprite and glowSprite.
            Height = buttonSprite.DrawHeight;
            Width = buttonSprite.DrawWidth;

            IsActive.BindValueChanged(e => updateGlowSprite(e.NewValue), true);
            CountPresses.BindValueChanged(e => countSpriteText.Text = e.NewValue.ToString(@"#,0"), true);
        }

        private void updateGlowSprite(bool show)
        {
            if (show)
            {
                double remainingFadeTime = FadeTime * (1 - glowSprite.Alpha);
                glowSprite.FadeIn(remainingFadeTime, Easing.OutQuint);
                textLayer.FadeColour(KeyDownTextColor, remainingFadeTime, Easing.OutQuint);
            }
            else
            {
                double remainingFadeTime = 8 * FadeTime * glowSprite.Alpha;
                glowSprite.FadeOut(remainingFadeTime, Easing.OutQuint);
                textLayer.FadeColour(KeyUpTextColor, remainingFadeTime, Easing.OutQuint);
            }
        }
    }
}
