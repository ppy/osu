// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.Chat
{
    public class ChannelSelectionOverlay : OverlayContainer
    {
        public static readonly float WIDTH_PADDING = 170;

        private readonly Box bg;
        private readonly Box headerBg;
        private readonly SearchTextBox search;

        public ChannelSelectionOverlay()
        {
            RelativeSizeAxes = Axes.X;
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    EdgeEffect = new EdgeEffect
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(0.25f),
                        Radius = 5,
                    },
                    Children = new[]
                    {
                        bg = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
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
                                },
                            },
                        },
                    },
                },
                new ChannelSection
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Padding = new MarginPadding { Left = WIDTH_PADDING, Right = WIDTH_PADDING },
                    Header = @"GENERAL CHANNELS",
                    Channels = new[]
                    {
                        new Channel { Name = @"announcements", Topic = @"Automated announcement of stuff going on in osu!" },
                        new Channel { Name = @"osu!", Topic = @"I dunno, the default channel I guess?" },
                        new Channel { Name = @"lobby", Topic = @"Look for trouble here" },
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            bg.Colour = colours.Gray3;
            headerBg.Colour = colours.Gray2.Opacity(0.75f);
        }

        protected override void PopIn()
        {
            search.HoldFocus = true;
            Schedule(() => search.TriggerFocus());
        }

        protected override void PopOut()
        {
            search.HoldFocus = false;
        }

        private class HeaderSearchTextBox : SearchTextBox
        {
            protected override Color4 BackgroundFocused => Color4.Black.Opacity(0.2f);
            protected override Color4 BackgroundUnfocused => Color4.Black.Opacity(0.2f);
        }
    }
}
