// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.MathUtils;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Overlays.Dialog
{
    public class PopupDialog : OverlayContainer
    {
        private const float enter_duration = 500;
        private const float exit_duration = 200;
        private readonly Vector2 ring_size = new Vector2(100f);
        private readonly Vector2 ring_minified_size = new Vector2(20f);
        private readonly Vector2 buttons_spacing = new Vector2(0f, 50f);

        private Container content, ring;
        private FlowContainer<PopupDialogButton> buttonsContainer;
        private TextAwesome iconText;
        private SpriteText header, body;

        public FontAwesome Icon
        {
            get
            {
                return iconText.Icon;
            }
            set
            {
                iconText.Icon = value;
            }
        }

        public string HeaderText
        {
            get
            {
                return header.Text;
            }
            set
            {
                header.Text = value;
            }
        }

        public string BodyText
        {
            get
            {
                return body.Text;
            }
            set
            {
                body.Text = value;
            }
        }

        public PopupDialogButton[] Buttons
        {
            get
            {
                return buttonsContainer.Children.ToArray();
            }
            set
            {
                buttonsContainer.Children = value;
                foreach (PopupDialogButton b in value)
                {
                    var action = b.Action;
                    b.Action = () =>
                    {
                        Hide();
                        action?.Invoke();
                    };
                }
            }
        }

        protected override void PopIn()
        {
            // Reset various animations but only if the dialog animation fully completed
            if (content.Alpha == 0)
            {
                buttonsContainer.TransformSpacingTo(buttons_spacing);
                buttonsContainer.MoveToY(buttons_spacing.Y);
                ring.ResizeTo(ring_minified_size);
            }

            content.FadeIn(enter_duration, EasingTypes.OutQuint);
            ring.ResizeTo(ring_size, enter_duration, EasingTypes.OutQuint);
            buttonsContainer.TransformSpacingTo(Vector2.Zero, enter_duration, EasingTypes.OutQuint);
            buttonsContainer.MoveToY(0, enter_duration, EasingTypes.OutQuint);
        }

        protected override void PopOut()
        {
            content.FadeOut(exit_duration, EasingTypes.InSine);
        }

        public PopupDialog()
        {
            Children = new Drawable[]
            {
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Width = 0.4f,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            EdgeEffect = new EdgeEffect
                            {
                                Type = EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(0.5f),
                                Radius = 8,
                            },
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = OsuColour.FromHex(@"221a21"),
                                },
                                new Triangles
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ColourLight = OsuColour.FromHex(@"271e26"),
                                    ColourDark = OsuColour.FromHex(@"1e171e"),
                                    TriangleScale = 4,
                                },
                            },
                        },
                        new FlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Position = new Vector2(0f, -50f),
                            Direction = FlowDirections.Vertical,
                            Spacing = new Vector2(0f, 10f),
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Size = ring_size,
                                    Margin = new MarginPadding
                                    {
                                        Bottom = 30,
                                    },
                                    Children = new Drawable[]
                                    {
                                        ring = new CircularContainer
                                        {
                                            Origin = Anchor.Centre,
                                            Anchor = Anchor.Centre,
                                            BorderColour = Color4.White,
                                            BorderThickness = 5f,
                                            Children = new Drawable[]
                                            {
                                                new Box
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Colour = Color4.Black.Opacity(0),
                                                },
                                                iconText = new TextAwesome
                                                {
                                                    Origin = Anchor.Centre,
                                                    Anchor = Anchor.Centre,
                                                    Icon = FontAwesome.fa_close,
                                                    TextSize = 50,
                                                },
                                            },
                                        },
                                    },
                                },
                                header = new SpriteText
                                {
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Text = @"Header",
                                    TextSize = 25,
                                    Shadow = true,
                                },
                                body = new SpriteText
                                {
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Text = @"Body",
                                    TextSize = 18,
                                    Shadow = true,
                                },
                            },
                        },
                        buttonsContainer = new FlowContainer<PopupDialogButton>
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FlowDirections.Vertical,
                        },
                    },
                },
            };
        }
    }
}