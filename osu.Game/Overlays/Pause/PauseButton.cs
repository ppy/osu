// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Containers;
using osu.Framework.Audio.Sample;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Pause
{
    public class PauseButton : ClickableContainer
    {
        private const float hover_width = 0.9f;
        private const float hover_duration = 500;
        private const float glow_fade_duration = 250;
        private const float click_duration = 200;

        private Color4 backgroundColour = OsuColour.Gray(34);

        private Color4 buttonColour;
        public Color4 ButtonColour
        {
            get
            {
                return buttonColour;
            }
            set
            {
                buttonColour = value;
                updateGlow();
                if (colourContainer == null) return;
                colourContainer.Colour = ButtonColour;
            }
        }

        private string text;
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
                if (spriteText == null) return;
                spriteText.Text = Text;
            }
        }

        public AudioSample SampleClick, SampleHover;

        private Container backgroundContainer, colourContainer, glowContainer;
        private Box leftGlow, centerGlow, rightGlow;
        private SpriteText spriteText;

        private bool didClick; // Used for making sure that the OnMouseDown animation can call instead of OnHoverLost's when clicking

        public override bool Contains(Vector2 screenSpacePos) => backgroundContainer.Contains(screenSpacePos);

        protected override bool OnClick(Framework.Input.InputState state)
        {
            didClick = true;
            colourContainer.ResizeTo(new Vector2(1.5f, 1f), click_duration, EasingTypes.In);
            flash();
            SampleClick?.Play();
            Action?.Invoke();

            Delay(click_duration);
            Schedule(delegate {
                colourContainer.ResizeTo(new Vector2(0.8f, 1f), 0, EasingTypes.None);
                spriteText.Spacing = Vector2.Zero;
                glowContainer.FadeOut();
            });

            return true;
        }

        protected override bool OnHover(Framework.Input.InputState state)
        {
            colourContainer.ResizeTo(new Vector2(hover_width, 1f), hover_duration, EasingTypes.OutElastic);
            spriteText.TransformSpacingTo(new Vector2(3f, 0f), hover_duration, EasingTypes.OutElastic);
            glowContainer.FadeIn(glow_fade_duration, EasingTypes.Out);
            SampleHover?.Play();
            return true;
        }

        protected override void OnHoverLost(Framework.Input.InputState state)
        {
            if (!didClick)
            {
                colourContainer.ResizeTo(new Vector2(0.8f, 1f), hover_duration, EasingTypes.OutElastic);
                spriteText.TransformSpacingTo(Vector2.Zero, hover_duration, EasingTypes.OutElastic);
                glowContainer.FadeOut(glow_fade_duration, EasingTypes.Out);
            }

            didClick = false;
        }

        private void flash()
        {
            var flash = new Box
            {
                RelativeSizeAxes = Axes.Both
            };

            colourContainer.Add(flash);

            flash.Colour = ButtonColour;
            flash.BlendingMode = BlendingMode.Additive;
            flash.Alpha = 0.3f;
            flash.FadeOutFromOne(click_duration);
            flash.Expire();
        }

        private void updateGlow()
        {
            leftGlow.ColourInfo = ColourInfo.GradientHorizontal(new Color4(ButtonColour.R, ButtonColour.G, ButtonColour.B, 0f), ButtonColour);
            centerGlow.Colour = ButtonColour;
            rightGlow.ColourInfo = ColourInfo.GradientHorizontal(ButtonColour, new Color4(ButtonColour.R, ButtonColour.G, ButtonColour.B, 0f));
        }

        public PauseButton()
        {
            Children = new Drawable[]
            {
                backgroundContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 1f,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = backgroundColour
                        }
                    }
                },
                glowContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 1f,
                    Alpha = 0f,
                    Children = new Drawable[]
                    {
                        leftGlow = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.TopLeft,
                            Anchor = Anchor.TopLeft,
                            Width = 0.125f
                        },
                        centerGlow = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Width = 0.75f
                        },
                        rightGlow = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.TopRight,
                            Anchor = Anchor.TopRight,
                            Width = 0.125f
                        }
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        colourContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Width = 0.8f,
                            Masking = true,
                            MaskingSmoothness = 2,
                            EdgeEffect = new EdgeEffect
                            {
                                Type = EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(0.2f),
                                Radius = 5
                            },
                            Colour = ButtonColour,
                            Shear = new Vector2(0.2f, 0),
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    EdgeSmoothness = new Vector2(2, 0),
                                    RelativeSizeAxes = Axes.Both
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Masking = true,
                                    MaskingSmoothness = 0,
                                    Children = new[]
                                    {
                                        new Triangles
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            TriangleScale = 4,
                                            ColourDark = OsuColour.Gray(0.88f),
                                            Shear = new Vector2(-0.2f, 0)
                                        }
                                    }
                                },
                            }
                        }
                    }
                },
                spriteText = new OsuSpriteText
                {
                    Text = Text,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    TextSize = 28,
                    Font = "Exo2.0-Bold",
                    Shadow = true,
                    ShadowColour = new Color4(0, 0, 0, 0.1f),
                    Colour = Color4.White
                }
            };

            updateGlow();
        }
    }
}
