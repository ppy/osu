// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Chat
{
    public class ChannelSelectionOverlay : OverlayContainer
    {
        public static readonly float WIDTH_PADDING = 170;

        private readonly Box bg;
        private readonly Triangles triangles;
        private readonly Box headerBg;
        private readonly SearchTextBox search;
        private readonly SearchContainer<ChannelSection> sectionsFlow;

        public IEnumerable<ChannelSection> Sections
        {
            set
            {
                sectionsFlow.Children = value;
            }
        }

        public ChannelSelectionOverlay()
        {
            RelativeSizeAxes = Axes.X;

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        bg = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        triangles = new Triangles
                        {
                            RelativeSizeAxes = Axes.Both,
                            TriangleScale = 5,
                        },
                    },
                },
                new ScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollDraggerVisible = false,
                    Padding = new MarginPadding { Top = 85 },
                    Children = new[]
                    {
                        sectionsFlow = new SearchContainer<ChannelSection>
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            LayoutDuration = 200,
                            LayoutEasing = EasingTypes.OutQuint,
                            Spacing = new Vector2(0f, 20f),
                            Padding = new MarginPadding { Top = 20, Left = WIDTH_PADDING, Right = WIDTH_PADDING },
                        },
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        headerBg = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0f, 10f),
                            Padding = new MarginPadding { Top = 10f, Bottom = 10f, Left = WIDTH_PADDING, Right = WIDTH_PADDING },
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = @"Chat Channels",
                                    TextSize = 20,
                                },
                                search = new HeaderSearchTextBox
                                {
                                    RelativeSizeAxes = Axes.X,
                                    PlaceholderText = @"Search",
                                    Exit = Hide,
                                },
                            },
                        },
                    },
                },
                new SettingsButton
                {
                    Margin = new MarginPadding { Top = 160 },
                },
            };

            search.Current.ValueChanged += newValue => sectionsFlow.SearchTerm = newValue;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            bg.Colour = colours.Gray3;
            triangles.ColourDark = colours.Gray3;
            triangles.ColourLight = OsuColour.FromHex(@"353535");

            headerBg.Colour = colours.Gray2.Opacity(0.75f);
        }

        protected override void PopIn()
        {
            search.HoldFocus = true;
            Schedule(() => search.TriggerFocus());

            FadeIn(100, EasingTypes.OutQuint);
            MoveToY(0, 800, EasingTypes.OutQuint);
        }

        protected override void PopOut()
        {
            search.HoldFocus = false;

            FadeOut(500, EasingTypes.InQuint);
            MoveToY(DrawHeight, 500, EasingTypes.In);
        }

        private class HeaderSearchTextBox : SearchTextBox
        {
            protected override Color4 BackgroundFocused => Color4.Black.Opacity(0.2f);
            protected override Color4 BackgroundUnfocused => Color4.Black.Opacity(0.2f);
        }

        private class SettingsButton : ClickableContainer
        {
            private const float width = 60f;

            private readonly Box bg;
            private readonly Container bgContainer;

            private SampleChannel clickSample;

            public SettingsButton()
            {
                Size = new Vector2(width, 50f);

                Children = new Drawable[]
                {
                    bgContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Shear = new Vector2(0.2f, 0f),
                        Masking = true,
                        MaskingSmoothness = 2,
                        EdgeEffect = new EdgeEffect
                        {
                            Type = EdgeEffectType.Shadow,
                            Colour = Color4.Black.Opacity(0.2f),
                            Offset = new Vector2(2, 0),
                            Radius = 2,
                        },
                        Children = new[]
                        {
                            bg = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                EdgeSmoothness = new Vector2(2f, 0f),
                            },
                        },
                    },
                    new TextAwesome
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Icon = FontAwesome.fa_osu_gear,
                        TextSize = 20,
                        Shadow = true,
                        UseFullGlyphHeight = false,
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, AudioManager audio)
            {
            	bg.Colour = colours.Pink;
                clickSample = audio.Sample.Get(@"Menu/menuclick");
            }

            protected override bool OnHover(InputState state)
            {
                ResizeWidthTo(width + 20f, 500, EasingTypes.OutElastic);

                return base.OnHover(state);
            }

            protected override void OnHoverLost(InputState state)
            {
                ResizeWidthTo(width, 500, EasingTypes.OutElastic);
            }

            protected override bool OnClick(InputState state)
            {
                var flash = new Box
                {
                	RelativeSizeAxes = Axes.Both,
                	Colour = Color4.White.Opacity(0.5f),
                };

    			bgContainer.Add(flash);

                flash.Alpha = 1;
                flash.FadeOut(500, EasingTypes.OutQuint);
                flash.Expire();

                clickSample.Play();
                return base.OnClick(state);
            }
        }
    }
}
