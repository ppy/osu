// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Chat;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Chat.Selection
{
    public class ChannelSelectionOverlay : WaveOverlayContainer
    {
        public static readonly float WIDTH_PADDING = 170;

        private const float transition_duration = 500;

        private readonly Box bg;
        private readonly Triangles triangles;
        private readonly Box headerBg;
        private readonly SearchTextBox search;
        private readonly SearchContainer<ChannelSection> sectionsFlow;

        protected override bool DimMainContent => false;

        public Action<Channel> OnRequestJoin;
        public Action<Channel> OnRequestLeave;

        public ChannelSelectionOverlay()
        {
            RelativeSizeAxes = Axes.X;

            Waves.FirstWaveColour = OsuColour.FromHex("353535");
            Waves.SecondWaveColour = OsuColour.FromHex("434343");
            Waves.ThirdWaveColour = OsuColour.FromHex("515151");
            Waves.FourthWaveColour = OsuColour.FromHex("595959");

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
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = 85, Right = WIDTH_PADDING },
                    Children = new[]
                    {
                        new OsuScrollContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new[]
                            {
                                sectionsFlow = new SearchContainer<ChannelSection>
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    LayoutDuration = 200,
                                    LayoutEasing = Easing.OutQuint,
                                    Spacing = new Vector2(0f, 20f),
                                    Padding = new MarginPadding { Vertical = 20, Left = WIDTH_PADDING },
                                },
                            },
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
                                    Font = OsuFont.GetFont(size: 20),
                                    Shadow = false,
                                },
                                search = new HeaderSearchTextBox
                                {
                                    RelativeSizeAxes = Axes.X,
                                    PlaceholderText = @"Search",
                                },
                            },
                        },
                    },
                },
            };

            search.Current.ValueChanged += term => sectionsFlow.SearchTerm = term.NewValue;
        }

        public void UpdateAvailableChannels(IEnumerable<Channel> channels)
        {
            Scheduler.Add(() =>
            {
                sectionsFlow.ChildrenEnumerable = new[]
                {
                    new ChannelSection
                    {
                        Header = "All Channels",
                        Channels = channels,
                    },
                };

                foreach (ChannelSection s in sectionsFlow.Children)
                {
                    foreach (ChannelListItem c in s.ChannelFlow.Children)
                    {
                        c.OnRequestJoin = channel => { OnRequestJoin?.Invoke(channel); };
                        c.OnRequestLeave = channel => { OnRequestLeave?.Invoke(channel); };
                    }
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            bg.Colour = colours.Gray3;
            triangles.ColourDark = colours.Gray3;
            triangles.ColourLight = OsuColour.FromHex(@"353535");

            headerBg.Colour = colours.Gray2.Opacity(0.75f);
        }

        protected override void OnFocus(FocusEvent e)
        {
            search.TakeFocus();
            base.OnFocus(e);
        }

        protected override void PopIn()
        {
            if (Alpha == 0) this.MoveToY(DrawHeight);

            this.FadeIn(transition_duration, Easing.OutQuint);
            this.MoveToY(0, transition_duration, Easing.OutQuint);

            search.HoldFocus = true;
            base.PopIn();
        }

        protected override void PopOut()
        {
            this.FadeOut(transition_duration, Easing.InSine);
            this.MoveToY(DrawHeight, transition_duration, Easing.InSine);

            search.HoldFocus = false;
            base.PopOut();
        }

        private class HeaderSearchTextBox : SearchTextBox
        {
            [BackgroundDependencyLoader]
            private void load()
            {
                BackgroundFocused = Color4.Black.Opacity(0.2f);
                BackgroundUnfocused = Color4.Black.Opacity(0.2f);
            }
        }
    }
}
