//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarButton : Container
    {
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

        public Action Action;
        protected TextAwesome DrawableIcon;
        protected SpriteText DrawableText;
        protected Box HoverBackground;
        private FlowContainer tooltipContainer;
        private SpriteText tooltip1;
        private SpriteText tooltip2;
        protected FlowContainer Flow;
        private AudioSample sampleClick;

        public ToolbarButton()
        {
            Children = new Drawable[]
            {
                HoverBackground = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(80).Opacity(180),
                    BlendingMode = BlendingMode.Additive,
                    Alpha = 0,
                },
                Flow = new FlowContainer
                {
                    Direction = FlowDirection.HorizontalOnly,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Padding = new MarginPadding { Left = 20, Right = 20 },
                    Spacing = new Vector2(5),
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        DrawableIcon = new TextAwesome
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                        DrawableText = new SpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                    },
                },
                tooltipContainer = new FlowContainer
                {
                    Direction = FlowDirection.VerticalOnly,
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomLeft,
                    Position = new Vector2(5, 5),
                    Alpha = 0,
                    Children = new[]
                    {
                        tooltip1 = new SpriteText
                        {
                            Shadow = true,
                            TextSize = 22,
                            Font = @"Exo2.0-Bold",
                        },
                        tooltip2 = new SpriteText
                        {
                            Shadow = true,
                            TextSize = 16
                        }
                    }
                }
            };

            RelativeSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleClick = audio.Sample.Get(@"Menu/menuclick");
        }

        protected override void Update()
        {
            base.Update();

            //todo: find a way to avoid using this (autosize needs to be able to ignore certain drawables.. in this case the tooltip)
            Size = new Vector2(Flow.DrawSize.X, 1);
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
                    Alpha = 0.05f,
                },
            };
        }
    }
}