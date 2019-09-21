// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Effects;
using osu.Game.Graphics.Containers;
using osu.Framework.Input.Events;

namespace osu.Game.Graphics.UserInterface
{
    public class DialogButton : OsuClickableContainer
    {
        private const float hover_width = 0.9f;
        private const float hover_duration = 500;
        private const float glow_fade_duration = 250;
        private const float click_duration = 200;

        public readonly BindableBool Selected = new BindableBool();

        private readonly Container backgroundContainer;
        private readonly Container colourContainer;
        private readonly Container glowContainer;
        private readonly Box leftGlow;
        private readonly Box centerGlow;
        private readonly Box rightGlow;
        private readonly Box background;
        private readonly SpriteText spriteText;
        private Vector2 hoverSpacing => new Vector2(3f, 0f);

        public DialogButton()
        {
            RelativeSizeAxes = Axes.X;

            Children = new Drawable[]
            {
                backgroundContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 1f,
                    Children = new Drawable[]
                    {
                        background = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = backgroundColour,
                        },
                    },
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
                            Width = 0.125f,
                        },
                        centerGlow = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Width = 0.75f,
                        },
                        rightGlow = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.TopRight,
                            Anchor = Anchor.TopRight,
                            Width = 0.125f,
                        },
                    },
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
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Type = EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(0.2f),
                                Radius = 5,
                            },
                            Colour = ButtonColour,
                            Shear = new Vector2(0.2f, 0),
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    EdgeSmoothness = new Vector2(2, 0),
                                    RelativeSizeAxes = Axes.Both,
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
                                            Shear = new Vector2(-0.2f, 0),
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
                spriteText = new OsuSpriteText
                {
                    Text = Text,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.GetFont(size: 28, weight: FontWeight.Bold),
                    Shadow = true,
                    ShadowColour = new Color4(0, 0, 0, 0.1f),
                    Colour = Color4.White,
                },
            };

            updateGlow();

            Selected.ValueChanged += selectionChanged;
        }

        private Color4 buttonColour;

        public Color4 ButtonColour
        {
            get => buttonColour;
            set
            {
                buttonColour = value;
                updateGlow();
                colourContainer.Colour = value;
            }
        }

        private Color4 backgroundColour = OsuColour.Gray(34);

        public Color4 BackgroundColour
        {
            get => backgroundColour;
            set
            {
                backgroundColour = value;
                background.Colour = value;
            }
        }

        private string text;

        public string Text
        {
            get => text;
            set
            {
                text = value;
                spriteText.Text = Text;
            }
        }

        public float TextSize
        {
            get => spriteText.Font.Size;
            set => spriteText.Font = spriteText.Font.With(size: value);
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => backgroundContainer.ReceivePositionalInputAt(screenSpacePos);

        protected override bool OnClick(ClickEvent e)
        {
            colourContainer.ResizeTo(new Vector2(1.5f, 1f), click_duration, Easing.In);
            flash();

            this.Delay(click_duration).Schedule(delegate
            {
                colourContainer.ResizeTo(new Vector2(0.8f, 1f));
                spriteText.Spacing = Vector2.Zero;
                glowContainer.FadeOut();
            });

            return base.OnClick(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            base.OnHover(e);

            Selected.Value = true;
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            Selected.Value = false;
        }

        private void selectionChanged(ValueChangedEvent<bool> args)
        {
            if (args.NewValue)
            {
                spriteText.TransformSpacingTo(hoverSpacing, hover_duration, Easing.OutElastic);
                colourContainer.ResizeTo(new Vector2(hover_width, 1f), hover_duration, Easing.OutElastic);
                glowContainer.FadeIn(glow_fade_duration, Easing.Out);
            }
            else
            {
                colourContainer.ResizeTo(new Vector2(0.8f, 1f), hover_duration, Easing.OutElastic);
                spriteText.TransformSpacingTo(Vector2.Zero, hover_duration, Easing.OutElastic);
                glowContainer.FadeOut(glow_fade_duration, Easing.Out);
            }
        }

        private void flash()
        {
            var flash = new Box
            {
                RelativeSizeAxes = Axes.Both
            };

            colourContainer.Add(flash);

            flash.Colour = ButtonColour;
            flash.Blending = BlendingParameters.Additive;
            flash.Alpha = 0.3f;
            flash.FadeOutFromOne(click_duration);
            flash.Expire();
        }

        private void updateGlow()
        {
            leftGlow.Colour = ColourInfo.GradientHorizontal(new Color4(ButtonColour.R, ButtonColour.G, ButtonColour.B, 0f), ButtonColour);
            centerGlow.Colour = ButtonColour;
            rightGlow.Colour = ColourInfo.GradientHorizontal(ButtonColour, new Color4(ButtonColour.R, ButtonColour.G, ButtonColour.B, 0f));
        }
    }
}
