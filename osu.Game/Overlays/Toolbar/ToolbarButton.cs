// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarButton : Container
    {
        public const float WIDTH = Toolbar.HEIGHT * 1.4f;

        public FontAwesome Icon
        {
            get { return DrawableIcon.Icon; }
            set { DrawableIcon.Icon = value; }
        }

        public string Text
        {
            get { return DrawableText.Text; }
            set
            {
                DrawableText.Text = value;
            }
        }

        public string TooltipMain
        {
            get { return tooltip1.Text; }
            set
            {
                tooltip1.Text = value;
            }
        }

        public string TooltipSub
        {
            get { return tooltip2.Text; }
            set
            {
                tooltip2.Text = value;
            }
        }

        protected virtual Anchor TooltipAnchor => Anchor.TopLeft;

        public Action Action;
        protected TextAwesome DrawableIcon;
        protected SpriteText DrawableText;
        protected Box HoverBackground;
        private readonly FillFlowContainer tooltipContainer;
        private readonly SpriteText tooltip1;
        private readonly SpriteText tooltip2;
        protected FillFlowContainer Flow;
        private SampleChannel sampleClick;

        public ToolbarButton()
        {
            Width = WIDTH;
            RelativeSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                HoverBackground = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(80).Opacity(180),
                    BlendingMode = BlendingMode.Additive,
                    Alpha = 0,
                },
                Flow = new FillFlowContainer
                {
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Padding = new MarginPadding { Left = Toolbar.HEIGHT / 2, Right = Toolbar.HEIGHT / 2 },
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        DrawableIcon = new TextAwesome
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            TextSize = 20
                        },
                        DrawableText = new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                    },
                },
                tooltipContainer = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.Both, //stops us being considered in parent's autosize
                    Anchor = (TooltipAnchor & Anchor.x0) > 0 ? Anchor.BottomLeft : Anchor.BottomRight,
                    Origin = TooltipAnchor,
                    Position = new Vector2((TooltipAnchor & Anchor.x0) > 0 ? 5 : -5, 5),
                    Alpha = 0,
                    Children = new[]
                    {
                        tooltip1 = new OsuSpriteText
                        {
                            Anchor = TooltipAnchor,
                            Origin = TooltipAnchor,
                            Shadow = true,
                            TextSize = 22,
                            Font = @"Exo2.0-Bold",
                        },
                        tooltip2 = new OsuSpriteText
                        {
                            Anchor = TooltipAnchor,
                            Origin = TooltipAnchor,
                            Shadow = true,
                            TextSize = 16
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleClick = audio.Sample.Get(@"Menu/menuclick");
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => true;

        protected override bool OnClick(InputState state)
        {
            Action?.Invoke();
            sampleClick.Play();
            HoverBackground.FlashColour(Color4.White.Opacity(100), 500, EasingTypes.OutQuint);
            return true;
        }

        protected override bool OnHover(InputState state)
        {
            HoverBackground.FadeIn(200);
            tooltipContainer.FadeIn(100);
            return false;
        }

        protected override void OnHoverLost(InputState state)
        {
            HoverBackground.FadeOut(200);
            tooltipContainer.FadeOut(100);
        }
    }

    public class OpaqueBackground : Container
    {
        public OpaqueBackground()
        {
            RelativeSizeAxes = Axes.Both;
            Masking = true;
            MaskingSmoothness = 0;
            EdgeEffect = new EdgeEffect
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(40),
                Radius = 5,
            };

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(30)
                },
                new Triangles
                {
                    RelativeSizeAxes = Axes.Both,
                    ColourLight = OsuColour.Gray(40),
                    ColourDark = OsuColour.Gray(20),
                },
            };
        }
    }
}